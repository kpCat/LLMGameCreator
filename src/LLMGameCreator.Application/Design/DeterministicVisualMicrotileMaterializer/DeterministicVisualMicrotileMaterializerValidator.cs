using System.Text.RegularExpressions;

namespace LLMGameCreator.Application.Design.DeterministicVisualMicrotileMaterializer;

public static class DeterministicVisualMicrotileMaterializerValidator
{
    private static readonly Regex StableIdRegex = new("^[a-z0-9][a-z0-9_.-]*$", RegexOptions.Compiled);
    private static readonly Regex HexColorRegex = new("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);

    public static VisualMicrotileValidationResult Validate(
        VisualMicrotileMaterializationRequest request,
        IReadOnlyDictionary<string, string>? svgByPreviewId = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        var diagnostics = new List<VisualMicrotileDiagnostic>();

        ValidateId(request.RequestId, "visual_microtile.request_id.invalid", "request", "Request id must be stable and lowercase.", diagnostics);
        ValidateRequiredText(request.GeneratorVersion, "visual_microtile.generator_version.missing", request.RequestId, "Generator version is required.", diagnostics);
        ValidateRelativePath(request.OutputRelativeDirectory, "visual_microtile.output_path.invalid", request.RequestId, "Output directory must be a safe relative path.", diagnostics);
        if (request.PromptTextIsSourceOfTruth
            || string.Equals(request.SourceOfTruthKind, "provider_prompt_text", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error("visual_microtile.prompt.source_of_truth", request.RequestId, "Prompt text must not be visual materialization source of truth."));
        }

        ValidateDuplicates(request.Previews, item => item.PreviewId, "visual_microtile.preview_id.duplicate", "Preview ids must be unique.", diagnostics);

        foreach (var preview in request.Previews.OrderBy(item => item.PreviewId, StringComparer.Ordinal))
        {
            ValidatePreview(preview, request.SourceGoal084And085LineageRequired, diagnostics);
        }

        if (svgByPreviewId != null)
        {
            foreach (var preview in request.Previews.OrderBy(item => item.PreviewId, StringComparer.Ordinal))
            {
                if (!svgByPreviewId.TryGetValue(preview.PreviewId, out var svg))
                {
                    diagnostics.Add(Error("visual_microtile.svg.missing", preview.PreviewId, "Every preview catalog entry must have a rendered SVG."));
                    continue;
                }

                ValidateSvg(preview.PreviewId, svg, diagnostics);
            }
        }

        return new VisualMicrotileValidationResult
        {
            Passed = diagnostics.All(item => item.Severity != "error"),
            DiagnosticCount = diagnostics.Count,
            Diagnostics = SortDiagnostics(diagnostics)
        };
    }

    public static IReadOnlyList<VisualMicrotileDiagnostic> SortDiagnostics(IEnumerable<VisualMicrotileDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(item => item.Severity, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.Target, StringComparer.Ordinal)
            .ToList();

    public static bool IsSvgSafe(string svg) =>
        !string.IsNullOrWhiteSpace(svg)
        && svg.Contains("<svg", StringComparison.Ordinal)
        && svg.Contains("viewBox=", StringComparison.Ordinal)
        && !svg.Contains("<script", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("http://", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("https://", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("xlink:href", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains(" href=", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("data:", StringComparison.OrdinalIgnoreCase)
        && !svg.Contains("base64", StringComparison.OrdinalIgnoreCase);

    public static int CountGeneratedShapes(string svg) =>
        Count(svg, "<rect ") + Count(svg, "<circle ") + Count(svg, "<ellipse ") + Count(svg, "<polygon ") + Count(svg, "<path ") + Count(svg, "<polyline ");

    private static void ValidatePreview(
        VisualMicrotilePreviewSpec preview,
        bool sourceLineageRequired,
        List<VisualMicrotileDiagnostic> diagnostics)
    {
        ValidateId(preview.PreviewId, "visual_microtile.preview_id.invalid", preview.PreviewId, "Preview id must be stable and lowercase.", diagnostics);
        ValidateRequiredText(preview.PartPackId, "visual_microtile.part_pack_id.missing", preview.PreviewId, "Part-pack id is required.", diagnostics);
        ValidateRequiredText(preview.AssetSlotId, "visual_microtile.asset_slot_id.missing", preview.PreviewId, "Goal084 asset slot id is required.", diagnostics);
        ValidateRequiredText(preview.PaletteProfileId, "visual_microtile.palette.missing", preview.PreviewId, "Palette profile id is required.", diagnostics);
        ValidateRelativePath(preview.PreviewRelativePath, "visual_microtile.preview_path.invalid", preview.PreviewId, "Preview path must be relative and safe.", diagnostics);

        if (!preview.PreviewRelativePath.StartsWith(DeterministicVisualMicrotileMaterializerVocabulary.PreviewRelativeDirectory + "/", StringComparison.Ordinal)
            || !preview.PreviewRelativePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(Error("visual_microtile.preview_path.not_svg_preview", preview.PreviewId, "Preview output must be previews/*.svg."));
        }

        if (preview.Seed <= 0)
        {
            diagnostics.Add(Error("visual_microtile.seed.missing_or_nondeterministic", preview.PreviewId, "A deterministic positive seed is required."));
        }

        if (preview.LayerStack.Count == 0)
        {
            diagnostics.Add(Error("visual_microtile.layer_stack.missing", preview.PreviewId, "A non-empty layer stack is required."));
        }

        if (preview.LayerStack.Select(item => item.Order).Distinct().Count() != preview.LayerStack.Count)
        {
            diagnostics.Add(Error("visual_microtile.layer_stack.order_duplicate", preview.PreviewId, "Layer ordering must be deterministic and unique."));
        }

        if (preview.Palette.Count == 0 || preview.Palette.Any(item => string.IsNullOrWhiteSpace(item.SlotId) || !HexColorRegex.IsMatch(item.HexColor)))
        {
            diagnostics.Add(Error("visual_microtile.palette.invalid", preview.PreviewId, "Palette swatches must include stable ids and #RRGGBB colors."));
        }

        if (preview.MaskIds.Count == 0 || preview.SocketIds.Count == 0 || preview.AnchorIds.Count == 0)
        {
            diagnostics.Add(Error("visual_microtile.bindings.missing", preview.PreviewId, "Mask, socket and anchor metadata are required."));
        }

        if (sourceLineageRequired
            && (string.IsNullOrWhiteSpace(preview.SourceGoal084SlotId) || string.IsNullOrWhiteSpace(preview.SourceGoal085PackId)))
        {
            diagnostics.Add(Error("visual_microtile.source_lineage.missing", preview.PreviewId, "Every preview must trace to Goal084 slot and Goal085 pack lineage."));
        }

        if (preview.Category == VisualMicrotileCategory.Water)
        {
            ValidateWaterPreview(preview, diagnostics);
        }

        if (preview.AdultMetadataOnly)
        {
            if (string.IsNullOrWhiteSpace(preview.SafeFallbackPreviewId))
            {
                diagnostics.Add(Error("visual_microtile.adult.safe_fallback_missing", preview.PreviewId, "Adult-capable metadata-only slots require a safe fallback preview."));
            }

            if (preview.ProviderState != VisualMicrotileProviderState.CandidateQuarantine)
            {
                diagnostics.Add(Error("visual_microtile.adult.boundary.invalid", preview.PreviewId, "Adult metadata proof must remain metadata-only and quarantined."));
            }
        }

        if (preview.ProviderState == VisualMicrotileProviderState.CandidateQuarantine
            && preview.TreatProviderCandidateAsApprovedOutput)
        {
            diagnostics.Add(Error("visual_microtile.provider_candidate.treated_as_approved", preview.PreviewId, "Provider candidates must not be treated as approved output."));
        }
    }

    private static void ValidateWaterPreview(
        VisualMicrotilePreviewSpec preview,
        List<VisualMicrotileDiagnostic> diagnostics)
    {
        var isCoast = preview.PreviewId.Contains("coast", StringComparison.OrdinalIgnoreCase)
            || preview.WaterRuleId.Contains("coast", StringComparison.OrdinalIgnoreCase);
        if (isCoast
            && (preview.WaterLandAdjacency == null
                || preview.WaterLandAdjacency.WaterEdges.Count == 0
                || preview.WaterLandAdjacency.LandEdges.Count == 0))
        {
            diagnostics.Add(Error("visual_microtile.water.coast_adjacency_missing", preview.PreviewId, "Coast previews require water and land adjacency metadata."));
        }

        var isRiver = preview.PreviewId.Contains("river", StringComparison.OrdinalIgnoreCase)
            || preview.WaterRuleId.Contains("river", StringComparison.OrdinalIgnoreCase);
        if (isRiver && preview.FlowConnectors.Count < 2)
        {
            diagnostics.Add(Error("visual_microtile.water.river_flow_missing", preview.PreviewId, "River previews require at least two deterministic flow connectors."));
        }
    }

    private static void ValidateSvg(string previewId, string svg, List<VisualMicrotileDiagnostic> diagnostics)
    {
        if (!IsSvgSafe(svg))
        {
            diagnostics.Add(Error("visual_microtile.svg.unsafe", previewId, "SVG must be script-free, external-resource-free and base64-free."));
        }

        var shapeCount = CountGeneratedShapes(svg) - 1;
        if (shapeCount < 2 || shapeCount > 5)
        {
            diagnostics.Add(Error("visual_microtile.svg.shape_count.invalid", previewId, "SVG previews must contain 2-5 generated shapes beyond the background."));
        }
    }

    private static void ValidateDuplicates<T>(
        IEnumerable<T> items,
        Func<T, string> idSelector,
        string code,
        string message,
        List<VisualMicrotileDiagnostic> diagnostics)
    {
        foreach (var duplicate in items
            .Where(item => !string.IsNullOrWhiteSpace(idSelector(item)))
            .GroupBy(idSelector, StringComparer.Ordinal)
            .Where(item => item.Count() > 1))
        {
            diagnostics.Add(Error(code, duplicate.Key, message));
        }
    }

    private static void ValidateId(
        string id,
        string code,
        string target,
        string message,
        List<VisualMicrotileDiagnostic> diagnostics)
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
        List<VisualMicrotileDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static void ValidateRelativePath(
        string relativePath,
        string code,
        string target,
        string message,
        List<VisualMicrotileDiagnostic> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(relativePath)
            || Path.IsPathRooted(relativePath)
            || relativePath.Contains(':', StringComparison.Ordinal)
            || relativePath.Contains('\\', StringComparison.Ordinal)
            || relativePath.Split('/').Any(segment => segment == ".." || string.IsNullOrWhiteSpace(segment)))
        {
            diagnostics.Add(Error(code, target, message));
        }
    }

    private static int Count(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            index += pattern.Length;
        }

        return count;
    }

    private static VisualMicrotileDiagnostic Error(string code, string target, string message) =>
        VisualMicrotileDiagnostic.Error(code, target, message);
}
