using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.VisualPartPackRuleStack;

public static class VisualPartPackRuleStackValidator
{
    private static readonly Regex StableIdRegex = new("^[a-z0-9][a-z0-9_./-]*$", RegexOptions.Compiled);

    public static VisualRuleStackValidationResult Validate(VisualPartPackManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        var diagnostics = new List<VisualRuleStackDiagnostic>();

        ValidateId(manifest.ManifestId, "visual_part_pack.manifest_id.invalid", "manifest", "Manifest id must be stable and lowercase.", diagnostics);
        ValidateRequiredText(manifest.GeneratorVersion, "visual_part_pack.generator_version.missing", manifest.ManifestId, "Manifest generator version is required.", diagnostics);
        if (manifest.PromptTextIsSourceOfTruth || string.Equals(manifest.SourceOfTruthKind, "provider_prompt_text", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error("visual_part_pack.prompt.source_of_truth", manifest.ManifestId, "Prompt text must not be treated as visual source of truth."));
        }

        ValidateDuplicates(manifest.PartPacks, item => item.PackId, "visual_part_pack.pack_id.duplicate", "Part-pack ids must be unique.", diagnostics);
        ValidateDuplicates(manifest.Recipes, item => item.RecipeId, "visual_part_pack.recipe_id.duplicate", "Recipe ids must be unique.", diagnostics);

        var packIds = manifest.PartPacks
            .Where(item => !string.IsNullOrWhiteSpace(item.PackId))
            .Select(item => item.PackId)
            .ToHashSet(StringComparer.Ordinal);
        var recipeIds = manifest.Recipes
            .Where(item => !string.IsNullOrWhiteSpace(item.RecipeId))
            .Select(item => item.RecipeId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var pack in manifest.PartPacks.OrderBy(item => item.PackId, StringComparer.Ordinal))
        {
            ValidatePack(pack, packIds, manifest.StrictReferenceValidation, diagnostics);
        }

        foreach (var recipe in manifest.Recipes.OrderBy(item => item.RecipeId, StringComparer.Ordinal))
        {
            var pack = manifest.PartPacks.FirstOrDefault(item => item.PackId == recipe.PackId);
            ValidateRecipe(recipe, pack, packIds, recipeIds, manifest.StrictReferenceValidation, diagnostics);
        }

        ValidateRecipeCycles(manifest.Recipes, diagnostics);

        return new VisualRuleStackValidationResult
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            DiagnosticCount = diagnostics.Count,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<VisualRuleStackDiagnostic> SortDiagnostics(IEnumerable<VisualRuleStackDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    private static void ValidatePack(
        VisualPartPackDefinition pack,
        HashSet<string> packIds,
        bool strictReferenceValidation,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        ValidateId(pack.PackId, "visual_part_pack.pack_id.invalid", pack.PackId, "Part-pack id must be stable and lowercase.", diagnostics);
        ValidateRequiredText(pack.ProvenanceRef, "visual_part_pack.provenance.missing", pack.PackId, "Part-pack provenance is required.", diagnostics);
        ValidateSha256(pack.Sha256, "visual_part_pack.sha256.missing", pack.PackId, "Part-pack metadata hash must be a 64-character sha256.", diagnostics);
        ValidateRelativePath(pack.MetadataRelativePath, "visual_part_pack.path.invalid", pack.PackId, "Part-pack metadata path must be relative and safe.", diagnostics);

        if (pack.PromptTextIsSourceOfTruth)
        {
            diagnostics.Add(Error("visual_part_pack.prompt.source_of_truth", pack.PackId, "Prompt text must not be treated as visual source of truth."));
        }

        if (pack.ProviderState == VisualPartProviderState.CandidateQuarantine
            && pack.ReviewStatus == VisualPartReviewStatus.ApprovedMetadata)
        {
            diagnostics.Add(Error("visual_part_pack.provider_candidate.treated_as_approved", pack.PackId, "Quarantined provider candidates must not be treated as approved metadata."));
        }

        if (strictReferenceValidation && !string.IsNullOrWhiteSpace(pack.SafeFallbackPackId) && !packIds.Contains(pack.SafeFallbackPackId))
        {
            diagnostics.Add(Error("visual_part_pack.fallback_pack.unknown", pack.SafeFallbackPackId, "Strict mode rejects unknown fallback pack refs."));
        }

        ValidateDuplicates(pack.Parts, item => item.PartId, "visual_part_pack.part_id.duplicate", "Part ids must be unique within a pack.", diagnostics);
        ValidateDuplicates(pack.Layers, item => item.LayerId, "visual_part_pack.layer_id.duplicate", "Layer ids must be unique within a pack.", diagnostics);
        ValidateDuplicates(pack.Masks, item => item.MaskId, "visual_part_pack.mask_id.duplicate", "Mask ids must be unique within a pack.", diagnostics);
        ValidateDuplicates(pack.Sockets, item => item.SocketId, "visual_part_pack.socket_id.duplicate", "Socket ids must be unique within a pack.", diagnostics);
        ValidateDuplicates(pack.Anchors, item => item.AnchorId, "visual_part_pack.anchor_id.duplicate", "Anchor ids must be unique within a pack.", diagnostics);
        ValidateDuplicates(pack.PaletteProfiles, item => item.PaletteProfileId, "visual_part_pack.palette_id.duplicate", "Palette ids must be unique within a pack.", diagnostics);

        var layerIds = pack.Layers.Select(item => item.LayerId).ToHashSet(StringComparer.Ordinal);
        var maskIds = pack.Masks.Select(item => item.MaskId).ToHashSet(StringComparer.Ordinal);
        var socketIds = pack.Sockets.Select(item => item.SocketId).ToHashSet(StringComparer.Ordinal);
        var anchorIds = pack.Anchors.Select(item => item.AnchorId).ToHashSet(StringComparer.Ordinal);
        var paletteIds = pack.PaletteProfiles.Select(item => item.PaletteProfileId).ToHashSet(StringComparer.Ordinal);
        var bodyPlanIds = pack.CreatureBodyPlanProfiles.Select(item => item.BodyPlanProfileId).ToHashSet(StringComparer.Ordinal);

        foreach (var mask in pack.Masks)
        {
            ValidateId(mask.MaskId, "visual_part_pack.mask_id.invalid", mask.MaskId, "Mask id must be stable and lowercase.", diagnostics);
            ValidateRelativePath(mask.RelativePath, "visual_part_pack.mask.path.invalid", mask.MaskId, "Mask path must be relative and safe.", diagnostics);
        }

        foreach (var part in pack.Parts)
        {
            ValidatePart(part, layerIds, maskIds, socketIds, anchorIds, paletteIds, diagnostics);
        }

        foreach (var paletteRule in pack.PaletteSwapRules)
        {
            if (strictReferenceValidation && !paletteIds.Contains(paletteRule.PaletteProfileId))
            {
                diagnostics.Add(Error("visual_part_pack.palette_ref.unknown", paletteRule.PaletteProfileId, "Palette swap rules must reference a known palette profile."));
            }
        }

        foreach (var transition in pack.TerrainTransitionRules)
        {
            if (strictReferenceValidation && !maskIds.Contains(transition.MaskId))
            {
                diagnostics.Add(Error("visual_part_pack.transition.mask_ref.unknown", transition.RuleId, "Terrain transition rules must reference a known mask."));
            }
        }

        foreach (var autoTile in pack.AutoTileRules)
        {
            if (strictReferenceValidation && !maskIds.Contains(autoTile.EdgeMaskId))
            {
                diagnostics.Add(Error("visual_part_pack.autotile.mask_ref.unknown", autoTile.RuleId, "Autotile rules must reference a known edge mask."));
            }
        }

        foreach (var overlay in pack.EquipmentOverlayProfiles)
        {
            ValidateEquipmentOverlay(overlay, socketIds, bodyPlanIds, strictReferenceValidation, diagnostics);
        }

        ValidateKindSpecificRules(pack, diagnostics);
    }

    private static void ValidatePart(
        VisualPartDefinition part,
        HashSet<string> layerIds,
        HashSet<string> maskIds,
        HashSet<string> socketIds,
        HashSet<string> anchorIds,
        HashSet<string> paletteIds,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        ValidateId(part.PartId, "visual_part_pack.part_id.invalid", part.PartId, "Part id must be stable and lowercase.", diagnostics);
        ValidateRelativePath(part.RelativePath, "visual_part_pack.part.path.invalid", part.PartId, "Part metadata path must be relative and safe.", diagnostics);

        if (!string.IsNullOrWhiteSpace(part.PaletteProfileId) && !paletteIds.Contains(part.PaletteProfileId))
        {
            diagnostics.Add(Error("visual_part_pack.palette_ref.unknown", part.PaletteProfileId, "Parts must reference a known palette profile."));
        }

        var layered = part.RequiresLayeredComposition || part.LayerIds.Count > 0;
        if (layered && (part.MaskIds.Count == 0 || part.SocketIds.Count == 0 || part.AnchorIds.Count == 0))
        {
            diagnostics.Add(Error("visual_part_pack.layered_part.binding_missing", part.PartId, "Layered parts require mask, socket and anchor refs."));
        }

        ValidateKnownRefs(part.LayerIds, layerIds, "visual_part_pack.layer_ref.unknown", part.PartId, "Layered parts must reference known layers.", diagnostics);
        ValidateKnownRefs(part.MaskIds, maskIds, "visual_part_pack.mask_ref.unknown", part.PartId, "Layered parts must reference known masks.", diagnostics);
        ValidateKnownRefs(part.SocketIds, socketIds, "visual_part_pack.socket_ref.unknown", part.PartId, "Layered parts must reference known sockets.", diagnostics);
        ValidateKnownRefs(part.AnchorIds, anchorIds, "visual_part_pack.anchor_ref.unknown", part.PartId, "Layered parts must reference known anchors.", diagnostics);
    }

    private static void ValidateEquipmentOverlay(
        VisualEquipmentOverlayProfile overlay,
        HashSet<string> socketIds,
        HashSet<string> bodyPlanIds,
        bool strictReferenceValidation,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        if (overlay.CompatibleSocketIds.Count == 0)
        {
            diagnostics.Add(Error("visual_part_pack.equipment.socket_compatibility_missing", overlay.EquipmentOverlayProfileId, "Equipment overlays require compatible socket ids."));
        }

        if (strictReferenceValidation)
        {
            ValidateKnownRefs(overlay.CompatibleSocketIds, socketIds, "visual_part_pack.equipment.socket_ref.unknown", overlay.EquipmentOverlayProfileId, "Equipment overlays must reference known sockets.", diagnostics);
            ValidateKnownRefs(overlay.CompatibleBodyPlanProfileIds, bodyPlanIds, "visual_part_pack.equipment.body_plan_ref.unknown", overlay.EquipmentOverlayProfileId, "Equipment overlays must reference known body plans.", diagnostics);
        }
    }

    private static void ValidateKindSpecificRules(
        VisualPartPackDefinition pack,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        if (pack.Kind == VisualPartPackKind.WaterBiome)
        {
            var waterKinds = pack.WaterProfiles
                .SelectMany(item => item.WaterKinds)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var coast = waterKinds.Contains("coast") || pack.WaterProfiles.Any(item => item.CoastAware);
            var river = waterKinds.Contains("river") || pack.WaterProfiles.Any(item => item.RiverAware);
            var lake = waterKinds.Contains("lake") || pack.WaterProfiles.Any(item => item.LakeAware);
            var marsh = waterKinds.Contains("marsh") || pack.WaterProfiles.Any(item => item.MarshAware);
            if (!coast || !river || !lake || !marsh)
            {
                diagnostics.Add(Error("visual_part_pack.water.coverage_missing", pack.PackId, "Water/coast packs require coast, river, lake and marsh coverage."));
            }
        }

        if (pack.Kind == VisualPartPackKind.TileTerrain
            && (pack.TerrainTransitionRules.Count == 0 || pack.AutoTileRules.Count == 0))
        {
            diagnostics.Add(Error("visual_part_pack.tile.transition_autotile_missing", pack.PackId, "Tile packs require transition and autotile rules."));
        }

        if (pack.Kind == VisualPartPackKind.CreatureBodyPlanEquipment
            && pack.CreatureBodyPlanProfiles.Count == 0)
        {
            diagnostics.Add(Error("visual_part_pack.creature.body_plan_rules_missing", pack.PackId, "Creature packs require body-plan compatibility rules."));
        }

        if ((pack.Kind == VisualPartPackKind.UiThemeEffect || pack.UiThemeProfiles.Count > 0 || pack.EffectProfiles.Count > 0)
            && string.IsNullOrWhiteSpace(pack.SafeFallbackPackId))
        {
            diagnostics.Add(Error("visual_part_pack.ui_effect.fallback_missing", pack.PackId, "UI/effect packs require safe fallback metadata."));
        }

        foreach (var effect in pack.EffectProfiles.Where(item => !item.HasSafeFallback))
        {
            diagnostics.Add(Error("visual_part_pack.effect.fallback_missing", effect.EffectProfileId, "Effect profiles require safe fallback metadata."));
        }

        if (pack.IsAdultRatingExtension)
        {
            if (string.IsNullOrWhiteSpace(pack.SafeFallbackPackId))
            {
                diagnostics.Add(Error("visual_part_pack.adult.fallback_missing", pack.PackId, "Rating-gated adult metadata extensions require a safe fallback pack."));
            }

            if (!pack.CreatureBodyPlanProfiles.Any(IsAdultEligibleBodyPlan))
            {
                diagnostics.Add(Error("visual_part_pack.adult.body_plan.ineligible", pack.PackId, "Adult metadata requires adult, sapient and humanoid-compatible body-plan metadata."));
            }

            if (pack.ExportPolicy == VisualPartExportPolicy.PublicSafe || pack.Rating == VisualContentRating.AdultMetadataOnly && pack.ExportPolicy == VisualPartExportPolicy.PublicSafe)
            {
                diagnostics.Add(Error("visual_part_pack.export_policy.contradiction", pack.PackId, "Adult or rating-gated metadata must not be exported through public-safe policy."));
            }
        }

        if (pack.Rating == VisualContentRating.AdultMetadataOnly && pack.ExportPolicy == VisualPartExportPolicy.PublicSafe)
        {
            diagnostics.Add(Error("visual_part_pack.export_policy.contradiction", pack.PackId, "Adult metadata must not use public-safe export policy."));
        }
    }

    private static void ValidateRecipe(
        VisualPartPackRecipe recipe,
        VisualPartPackDefinition? pack,
        HashSet<string> packIds,
        HashSet<string> recipeIds,
        bool strictReferenceValidation,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        ValidateId(recipe.RecipeId, "visual_part_pack.recipe_id.invalid", recipe.RecipeId, "Recipe id must be stable and lowercase.", diagnostics);
        if (strictReferenceValidation && !packIds.Contains(recipe.PackId))
        {
            diagnostics.Add(Error("visual_part_pack.recipe.pack_ref.unknown", recipe.PackId, "Recipes must reference a known part pack."));
        }

        if (pack != null)
        {
            var partIds = pack.Parts.Select(item => item.PartId).ToHashSet(StringComparer.Ordinal);
            var paletteIds = pack.PaletteProfiles.Select(item => item.PaletteProfileId).ToHashSet(StringComparer.Ordinal);
            if (!paletteIds.Contains(recipe.PaletteProfileId))
            {
                diagnostics.Add(Error("visual_part_pack.recipe.palette_ref.unknown", recipe.PaletteProfileId, "Recipes must reference a known palette profile."));
            }

            ValidateKnownRefs(recipe.PartIds, partIds, "visual_part_pack.recipe.part_ref.unknown", recipe.RecipeId, "Recipes must reference known parts.", diagnostics);
        }

        foreach (var dependency in recipe.DependsOnRecipeIds)
        {
            if (strictReferenceValidation && !recipeIds.Contains(dependency))
            {
                diagnostics.Add(Error("visual_part_pack.recipe_ref.unknown", dependency, "Recipes must reference known recipe dependencies."));
            }
        }

        if (!string.IsNullOrWhiteSpace(recipe.SafeFallbackRecipeId)
            && strictReferenceValidation
            && !recipeIds.Contains(recipe.SafeFallbackRecipeId))
        {
            diagnostics.Add(Error("visual_part_pack.recipe_fallback_ref.unknown", recipe.SafeFallbackRecipeId, "Recipe fallback refs must be known."));
        }
    }

    private static void ValidateRecipeCycles(
        IReadOnlyList<VisualPartPackRecipe> recipes,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        var byId = recipes
            .Where(item => !string.IsNullOrWhiteSpace(item.RecipeId))
            .GroupBy(item => item.RecipeId, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.First(), StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var reported = new HashSet<string>(StringComparer.Ordinal);

        foreach (var recipeId in byId.Keys.Order(StringComparer.Ordinal))
        {
            if (HasCycle(recipeId, byId, visiting, visited) && reported.Add(recipeId))
            {
                diagnostics.Add(Error("visual_part_pack.recipe_dependency.cycle", recipeId, "Recipe dependencies must be acyclic."));
            }
        }
    }

    private static bool HasCycle(
        string recipeId,
        IReadOnlyDictionary<string, VisualPartPackRecipe> byId,
        HashSet<string> visiting,
        HashSet<string> visited)
    {
        if (visited.Contains(recipeId))
        {
            return false;
        }

        if (!visiting.Add(recipeId))
        {
            return true;
        }

        if (byId.TryGetValue(recipeId, out var recipe))
        {
            foreach (var dependency in recipe.DependsOnRecipeIds.Where(byId.ContainsKey))
            {
                if (HasCycle(dependency, byId, visiting, visited))
                {
                    return true;
                }
            }
        }

        visiting.Remove(recipeId);
        visited.Add(recipeId);
        return false;
    }

    private static bool IsAdultEligibleBodyPlan(VisualCreatureBodyPlanProfile profile) =>
        profile.AdultEligible
        && profile.AgeKnownAdult
        && profile.Sapient
        && profile.HumanoidCompatible
        && !profile.AgeAmbiguous
        && !profile.NonSapient;

    private static void ValidateDuplicates<T>(
        IEnumerable<T> items,
        Func<T, string> idSelector,
        string code,
        string message,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        foreach (var duplicate in items
            .Where(item => !string.IsNullOrWhiteSpace(idSelector(item)))
            .GroupBy(idSelector, StringComparer.Ordinal)
            .Where(item => item.Count() > 1))
        {
            diagnostics.Add(Error(code, duplicate.Key, message));
        }
    }

    private static void ValidateKnownRefs(
        IEnumerable<string> refs,
        HashSet<string> known,
        string code,
        string target,
        string message,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        foreach (var reference in refs.Where(item => !known.Contains(item)))
        {
            diagnostics.Add(Error(code, string.IsNullOrWhiteSpace(reference) ? target : reference, message));
        }
    }

    private static void ValidateId(
        string id,
        string code,
        string target,
        string message,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(id) || !StableIdRegex.IsMatch(id))
        {
            diagnostics.Add(Error(code, string.IsNullOrWhiteSpace(target) ? "<empty>" : target, message));
        }
    }

    private static void ValidateRequiredText(
        string text,
        string code,
        string target,
        string message,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static void ValidateSha256(
        string sha256,
        string code,
        string target,
        string message,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(sha256) || sha256.Length != 64 || !sha256.All(Uri.IsHexDigit))
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static void ValidateRelativePath(
        string relativePath,
        string code,
        string target,
        string message,
        List<VisualRuleStackDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(relativePath)
            || Path.IsPathRooted(relativePath)
            || relativePath.Contains(':', StringComparison.Ordinal)
            || relativePath.Contains('\\', StringComparison.Ordinal)
            || relativePath.Split('/').Any(segment => segment == ".."))
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static VisualRuleStackDiagnostic Error(string code, string target, string message) =>
        VisualRuleStackDiagnostic.Error(code, target, message);
}
