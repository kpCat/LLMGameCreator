using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.VisualAssetContractRatingMetadata;

public static partial class VisualAssetContractRatingMetadataValidator
{
    private static readonly HashSet<VisualReviewStatus> ApprovedReviewStatuses = new()
    {
        VisualReviewStatus.ApprovedSafe,
        VisualReviewStatus.ApprovedAdult
    };

    private static readonly HashSet<VisualRating> AdultRatings = new()
    {
        VisualRating.AdultNudeReference,
        VisualRating.AdultEroticScene,
        VisualRating.AdultPrivateExplicit
    };

    public static VisualAssetContractValidationResult Validate(VisualAssetContract contract)
    {
        ArgumentNullException.ThrowIfNull(contract);
        var diagnostics = new List<VisualAssetContractDiagnostic>();

        ValidateId(contract.ContractId, "visual_contract.contract_id.invalid", "contract", "Contract id must be a stable lowercase id.", diagnostics);
        if (string.IsNullOrWhiteSpace(contract.GeneratorVersion))
        {
            diagnostics.Add(Error("visual_contract.generator_version.missing", contract.ContractId, "Contract generator version is required."));
        }

        var recipeIds = contract.RecipeRefs
            .Where(item => !string.IsNullOrWhiteSpace(item.RecipeId))
            .Select(item => item.RecipeId)
            .ToHashSet(StringComparer.Ordinal);
        var partPackIds = contract.PartPackRefs
            .Where(item => !string.IsNullOrWhiteSpace(item.PartPackId))
            .Select(item => item.PartPackId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var recipe in contract.RecipeRefs)
        {
            ValidateId(recipe.RecipeId, "visual_contract.recipe_ref.id.invalid", recipe.RecipeId, "Recipe refs require stable ids.", diagnostics);
            ValidateRequiredText(recipe.ProvenanceRef, "visual_contract.recipe_ref.provenance.missing", recipe.RecipeId, "Recipe refs require provenance.", diagnostics);
            ValidateRequiredText(recipe.GeneratorVersion, "visual_contract.recipe_ref.generator_version.missing", recipe.RecipeId, "Recipe refs require generator version.", diagnostics);
        }

        foreach (var partPack in contract.PartPackRefs)
        {
            ValidateId(partPack.PartPackId, "visual_contract.part_pack_ref.id.invalid", partPack.PartPackId, "Part-pack refs require stable ids.", diagnostics);
            ValidateRequiredText(partPack.ProvenanceRef, "visual_contract.part_pack_ref.provenance.missing", partPack.PartPackId, "Part-pack refs require provenance.", diagnostics);
            ValidateRequiredText(partPack.GeneratorVersion, "visual_contract.part_pack_ref.generator_version.missing", partPack.PartPackId, "Part-pack refs require generator version.", diagnostics);
        }

        var slotIds = contract.Slots
            .Where(item => !string.IsNullOrWhiteSpace(item.AssetSlot))
            .GroupBy(item => item.AssetSlot, StringComparer.Ordinal)
            .ToDictionary(item => item.Key, item => item.ToList(), StringComparer.Ordinal);
        foreach (var duplicate in slotIds.Where(item => item.Value.Count > 1))
        {
            diagnostics.Add(Error("visual_contract.slot_id.duplicate", duplicate.Key, "Asset slot ids must be unique."));
        }

        foreach (var slot in contract.Slots.OrderBy(item => item.AssetSlot, StringComparer.Ordinal))
        {
            ValidateSlot(slot, recipeIds, partPackIds, contract.StrictReferenceValidation, diagnostics);
        }

        var knownSlotIds = slotIds.Keys.ToHashSet(StringComparer.Ordinal);
        foreach (var candidate in contract.CandidateRecords.OrderBy(item => item.CandidateId, StringComparer.Ordinal))
        {
            ValidateCandidate(candidate, knownSlotIds, diagnostics);
        }

        var sorted = SortDiagnostics(diagnostics);
        return new VisualAssetContractValidationResult
        {
            Passed = sorted.All(item => item.Severity != "error"),
            DiagnosticCount = sorted.Count,
            Diagnostics = sorted
        };
    }

    public static IReadOnlyList<VisualAssetContractDiagnostic> SortDiagnostics(IEnumerable<VisualAssetContractDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ThenBy(item => item.Message, StringComparer.Ordinal)
            .ToList();

    public static bool HasDeterministicSafeFallback(VisualAssetSlot slot) =>
        slot.SafeFallbackRef is { Deterministic: true, Rating: VisualRating.Safe } fallback
        && !string.IsNullOrWhiteSpace(fallback.FallbackId)
        && IsSafeRelativePath(fallback.RelativePath)
        && IsSha256(fallback.Sha256)
        && !string.IsNullOrWhiteSpace(fallback.ProvenanceRef);

    public static bool HasSafeApprovedAssetRef(VisualAssetSlot slot) =>
        slot.ApprovedAssetRef is
        {
            Rating: VisualRating.Safe,
            ReviewStatus: VisualReviewStatus.ApprovedSafe,
            ExportPolicy: VisualExportPolicy.PublicSafe
        } asset
        && IsSafeRelativePath(asset.RelativePath)
        && IsSha256(asset.Sha256)
        && !string.IsNullOrWhiteSpace(asset.ProvenanceRef);

    private static void ValidateSlot(
        VisualAssetSlot slot,
        IReadOnlySet<string> recipeIds,
        IReadOnlySet<string> partPackIds,
        bool strictReferenceValidation,
        ICollection<VisualAssetContractDiagnostic> diagnostics)
    {
        ValidateId(slot.AssetSlot, "visual_contract.slot_id.invalid", slot.AssetSlot, "Asset slot id must be a stable lowercase id.", diagnostics);
        ValidateRequiredText(slot.ProvenanceRef, "visual_contract.slot.provenance.missing", slot.AssetSlot, "Asset slot provenance ref is required.", diagnostics);
        ValidateRequiredText(slot.GeneratorVersion, "visual_contract.slot.generator_version.missing", slot.AssetSlot, "Asset slot generator version is required.", diagnostics);
        ValidateRequiredText(slot.Seed, "visual_contract.slot.seed.missing", slot.AssetSlot, "Asset slot deterministic seed is required.", diagnostics);

        if (!string.IsNullOrWhiteSpace(slot.RelativePath) && !IsSafeRelativePath(slot.RelativePath))
        {
            diagnostics.Add(Error("visual_contract.path.absolute_or_traversal", slot.AssetSlot, "Slot relativePath must be a safe relative path when present."));
        }

        if (!string.IsNullOrWhiteSpace(slot.Sha256) && !IsSha256(slot.Sha256))
        {
            diagnostics.Add(Error("visual_contract.sha256.invalid", slot.AssetSlot, "Slot sha256 must be a lowercase or uppercase 64-character hex hash when present."));
        }

        if (slot.Rating == VisualRating.Unspecified)
        {
            diagnostics.Add(Error("visual_contract.rating.missing", slot.AssetSlot, "Every visual asset slot requires an explicit rating."));
        }

        if (slot.ExportPolicy == VisualExportPolicy.Unspecified)
        {
            diagnostics.Add(Error("visual_contract.export_policy.missing", slot.AssetSlot, "Every visual asset slot requires an explicit export policy."));
        }

        if (slot.ApprovedAssetRef != null)
        {
            ValidateApprovedRef(slot.AssetSlot, slot.ApprovedAssetRef, diagnostics);
        }

        if (slot.SafeFallbackRef != null)
        {
            ValidateFallbackRef(slot.AssetSlot, slot.SafeFallbackRef, diagnostics);
        }

        if (slot.SafeFallbackRequired && !HasDeterministicSafeFallback(slot))
        {
            diagnostics.Add(Error("visual_contract.fallback.required_missing", slot.AssetSlot, "Slots that require a safe fallback must declare a deterministic safe fallback ref."));
        }

        if (slot.ExportPolicy == VisualExportPolicy.PublicSafe && !HasSafeApprovedAssetRef(slot) && !HasDeterministicSafeFallback(slot))
        {
            diagnostics.Add(Error("visual_contract.public_export.safe_ref_or_fallback_missing", slot.AssetSlot, "Public-safe export requires a safe approved asset ref or deterministic safe fallback."));
        }

        ValidateRatingExportCombination(slot, diagnostics);
        ValidateAdultEligibility(slot, diagnostics);

        if (strictReferenceValidation)
        {
            if (slot.RecipeRef == null || string.IsNullOrWhiteSpace(slot.RecipeRef.RecipeId))
            {
                diagnostics.Add(Error("visual_contract.recipe_ref.missing", slot.AssetSlot, "Strict mode requires a recipe ref."));
            }
            else if (!recipeIds.Contains(slot.RecipeRef.RecipeId))
            {
                diagnostics.Add(Error("visual_contract.recipe_ref.unknown", slot.RecipeRef.RecipeId, "Strict mode rejects unknown recipe refs."));
            }

            if (slot.PartPackRef == null || string.IsNullOrWhiteSpace(slot.PartPackRef.PartPackId))
            {
                diagnostics.Add(Error("visual_contract.part_pack_ref.missing", slot.AssetSlot, "Strict mode requires a part-pack ref."));
            }
            else if (!partPackIds.Contains(slot.PartPackRef.PartPackId))
            {
                diagnostics.Add(Error("visual_contract.part_pack_ref.unknown", slot.PartPackRef.PartPackId, "Strict mode rejects unknown part-pack refs."));
            }
        }
    }

    private static void ValidateApprovedRef(
        string slotId,
        VisualApprovedAssetRef asset,
        ICollection<VisualAssetContractDiagnostic> diagnostics)
    {
        var target = string.IsNullOrWhiteSpace(asset.AssetId) ? slotId : asset.AssetId;
        ValidateId(asset.AssetId, "visual_contract.approved_ref.id.invalid", target, "Approved asset refs require stable ids.", diagnostics);
        ValidateRequiredText(asset.ProvenanceRef, "visual_contract.approved_ref.provenance.missing", target, "Approved asset refs require provenance.", diagnostics);

        if (!IsSafeRelativePath(asset.RelativePath))
        {
            diagnostics.Add(Error("visual_contract.approved_ref.path.invalid", target, "Approved asset refs require a safe relative path."));
        }

        if (!IsSha256(asset.Sha256))
        {
            diagnostics.Add(Error("visual_contract.approved_ref.sha256.missing", target, "Approved asset refs require a 64-character sha256 hash."));
        }

        if (!ApprovedReviewStatuses.Contains(asset.ReviewStatus))
        {
            diagnostics.Add(Error("visual_contract.approved_ref.review_status.invalid", target, "Approved asset refs must have an approved review status."));
        }

        if (asset.Rating == VisualRating.Unspecified || asset.ExportPolicy == VisualExportPolicy.Unspecified)
        {
            diagnostics.Add(Error("visual_contract.approved_ref.rating_export.missing", target, "Approved asset refs require explicit rating and export policy."));
        }
    }

    private static void ValidateFallbackRef(
        string slotId,
        VisualSafeFallbackRef fallback,
        ICollection<VisualAssetContractDiagnostic> diagnostics)
    {
        var target = string.IsNullOrWhiteSpace(fallback.FallbackId) ? slotId : fallback.FallbackId;
        ValidateId(fallback.FallbackId, "visual_contract.fallback.id.invalid", target, "Safe fallback refs require stable ids.", diagnostics);
        ValidateRequiredText(fallback.ProvenanceRef, "visual_contract.fallback.provenance.missing", target, "Safe fallback refs require provenance.", diagnostics);

        if (!fallback.Deterministic)
        {
            diagnostics.Add(Error("visual_contract.fallback.not_deterministic", target, "Safe fallback refs must be deterministic."));
        }

        if (fallback.Rating != VisualRating.Safe)
        {
            diagnostics.Add(Error("visual_contract.fallback.rating.not_safe", target, "Safe fallback refs must be rated safe."));
        }

        if (!IsSafeRelativePath(fallback.RelativePath))
        {
            diagnostics.Add(Error("visual_contract.fallback.path.invalid", target, "Safe fallback refs require a safe relative path."));
        }

        if (!IsSha256(fallback.Sha256))
        {
            diagnostics.Add(Error("visual_contract.fallback.sha256.missing", target, "Safe fallback refs require a 64-character sha256 hash."));
        }
    }

    private static void ValidateRatingExportCombination(
        VisualAssetSlot slot,
        ICollection<VisualAssetContractDiagnostic> diagnostics)
    {
        if (slot.ExportPolicy == VisualExportPolicy.PublicSafe && slot.Rating != VisualRating.Safe)
        {
            diagnostics.Add(Error("visual_contract.rating_export.contradiction", slot.AssetSlot, "Public-safe export can only carry safe-rated refs."));
        }

        if (AdultRatings.Contains(slot.Rating) && slot.ExportPolicy == VisualExportPolicy.PublicSafe)
        {
            diagnostics.Add(Error("visual_contract.rating_export.adult_public_contradiction", slot.AssetSlot, "Adult ratings must not be exported through public-safe policy."));
        }

        if (slot.Rating == VisualRating.AdultPrivateExplicit && slot.ExportPolicy != VisualExportPolicy.PrivateLocalOnly)
        {
            diagnostics.Add(Error("visual_contract.rating_export.private_policy_required", slot.AssetSlot, "Private explicit metadata must stay private-local-only."));
        }

        if (slot.ExportPolicy == VisualExportPolicy.Blocked && ApprovedReviewStatuses.Contains(slot.ReviewStatus))
        {
            diagnostics.Add(Error("visual_contract.rating_export.blocked_approved_contradiction", slot.AssetSlot, "Blocked export policy must not be approved for promotion."));
        }
    }

    private static void ValidateAdultEligibility(
        VisualAssetSlot slot,
        ICollection<VisualAssetContractDiagnostic> diagnostics)
    {
        if (!slot.AdultEnabled)
        {
            return;
        }

        if (slot.Rating == VisualRating.Unspecified || slot.ExportPolicy == VisualExportPolicy.Unspecified)
        {
            diagnostics.Add(Error("visual_contract.adult.policy_missing", slot.AssetSlot, "Adult-enabled slots require explicit rating and export policy."));
        }

        if (!slot.SafeFallbackRequired)
        {
            diagnostics.Add(Error("visual_contract.adult.safe_fallback_flag_missing", slot.AssetSlot, "Adult-enabled slots must require a safe fallback."));
        }

        if (!HasDeterministicSafeFallback(slot))
        {
            diagnostics.Add(Error("visual_contract.adult.fallback_missing", slot.AssetSlot, "Adult-enabled slots require a deterministic safe fallback."));
        }

        if (slot.ExportPolicy == VisualExportPolicy.PublicSafe && !HasDeterministicSafeFallback(slot))
        {
            diagnostics.Add(Error("visual_contract.adult.public_export_fallback_missing", slot.AssetSlot, "Adult-enabled public export must have a deterministic safe fallback."));
        }

        if (slot.BodyPlanEligibility != VisualBodyPlanEligibility.AdultEligibleHumanoidSapient)
        {
            diagnostics.Add(Error("visual_contract.adult.body_plan.ineligible", slot.AssetSlot, "Adult metadata requires adult, sapient and humanoid-compatible eligibility."));
        }

        var facts = slot.BodyPlanEligibilityFacts;
        if (!facts.AdultCharacter || !facts.AgeKnownAdult || facts.AgeAmbiguous)
        {
            diagnostics.Add(Error("visual_contract.adult.age_ambiguous_or_not_adult", slot.AssetSlot, "Adult metadata rejects age-ambiguous or non-adult eligibility facts."));
        }

        if (!facts.Sapient || facts.NonSapient)
        {
            diagnostics.Add(Error("visual_contract.adult.non_sapient", slot.AssetSlot, "Adult metadata rejects non-sapient eligibility facts."));
        }

        if (!facts.HumanoidCompatible || facts.FeralOrNonHumanoidSafeOnly)
        {
            diagnostics.Add(Error("visual_contract.adult.non_humanoid", slot.AssetSlot, "Adult metadata rejects non-humanoid or safe-only body plans."));
        }
    }

    private static void ValidateCandidate(
        VisualCandidateRecord candidate,
        IReadOnlySet<string> knownSlotIds,
        ICollection<VisualAssetContractDiagnostic> diagnostics)
    {
        var target = string.IsNullOrWhiteSpace(candidate.CandidateId) ? candidate.AssetSlot : candidate.CandidateId;
        ValidateId(candidate.CandidateId, "visual_contract.candidate.id.invalid", target, "Candidate records require stable ids.", diagnostics);
        ValidateId(candidate.AssetSlot, "visual_contract.candidate.slot_id.invalid", target, "Candidate records require a stable asset slot id.", diagnostics);

        if (!string.IsNullOrWhiteSpace(candidate.AssetSlot) && !knownSlotIds.Contains(candidate.AssetSlot))
        {
            diagnostics.Add(Error("visual_contract.candidate.slot_id.unknown", target, "Candidate records must reference a known asset slot."));
        }

        if (candidate.ProviderState == VisualProviderState.CandidateQuarantine && !candidate.CandidateQuarantine)
        {
            diagnostics.Add(Error("visual_contract.candidate.quarantine_flag_missing", target, "Provider candidates must remain in candidate quarantine."));
        }

        if (candidate.PromptTextIsSourceOfTruth || candidate.SourceOfTruthKind.Contains("prompt", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error("visual_contract.prompt.source_of_truth", target, "Prompt text must not be treated as source of truth."));
        }

        if (candidate.ProviderState == VisualProviderState.CandidateQuarantine && ApprovedReviewStatuses.Contains(candidate.ReviewStatus))
        {
            diagnostics.Add(Error("visual_contract.provider_candidate.treated_as_approved", target, "Quarantined provider candidates must not be treated as approved assets."));
        }

        if (candidate.ProviderState == VisualProviderState.ApprovedAsset
            && (candidate.CandidateQuarantine || !ApprovedReviewStatuses.Contains(candidate.ReviewStatus)))
        {
            diagnostics.Add(Error("visual_contract.provider_candidate.treated_as_approved", target, "Provider candidates cannot become approved refs without review/promotion state."));
        }

        if (candidate.PromotionRequested && !ApprovedReviewStatuses.Contains(candidate.ReviewStatus))
        {
            diagnostics.Add(Error("visual_contract.promotion.unreviewed_or_rejected", target, "Promotion requests require approved review status."));
        }

        if (candidate.ReviewStatus == VisualReviewStatus.Rejected && candidate.ProviderState == VisualProviderState.ApprovedAsset)
        {
            diagnostics.Add(Error("visual_contract.promotion.unreviewed_or_rejected", target, "Rejected candidate records must not be promoted."));
        }

        if (candidate.ProviderState != VisualProviderState.None)
        {
            ValidateRequiredText(candidate.ProvenanceRef, "visual_contract.candidate.provenance.missing", target, "Candidate records require provenance.", diagnostics);
            ValidateRequiredText(candidate.GeneratorVersion, "visual_contract.candidate.generator_version.missing", target, "Candidate records require generator version.", diagnostics);

            if (!IsSafeRelativePath(candidate.RelativePath))
            {
                diagnostics.Add(Error("visual_contract.candidate.path.invalid", target, "Candidate records require a safe relative path."));
            }

            if (!IsSha256(candidate.Sha256))
            {
                diagnostics.Add(Error("visual_contract.candidate.sha256.missing", target, "Candidate records require a 64-character sha256 hash."));
            }
        }
    }

    private static void ValidateId(
        string value,
        string code,
        string target,
        string message,
        ICollection<VisualAssetContractDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value) || !StableIdPattern().IsMatch(value))
        {
            diagnostics.Add(Error(code, string.IsNullOrWhiteSpace(target) ? "<empty>" : target, message));
        }
    }

    private static void ValidateRequiredText(
        string value,
        string code,
        string target,
        string message,
        ICollection<VisualAssetContractDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add(Error(code, string.IsNullOrWhiteSpace(target) ? "<empty>" : target, message));
        }
    }

    private static bool IsSafeRelativePath(string path) =>
        !string.IsNullOrWhiteSpace(path)
        && !Path.IsPathRooted(path)
        && !path.Contains(':', StringComparison.Ordinal)
        && !path.Contains("://", StringComparison.Ordinal)
        && !path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries).Contains("..", StringComparer.Ordinal);

    private static bool IsSha256(string value) =>
        Sha256Pattern().IsMatch(value);

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "error" => 0,
            "warning" => 1,
            "info" => 2,
            _ => 3
        };

    private static VisualAssetContractDiagnostic Error(string code, string target, string message) =>
        VisualAssetContractDiagnostic.Error(code, target, message);

    [GeneratedRegex("^[a-z0-9][a-z0-9_./-]*[a-z0-9]$")]
    private static partial Regex StableIdPattern();

    [GeneratedRegex("^[A-Fa-f0-9]{64}$")]
    private static partial Regex Sha256Pattern();
}
