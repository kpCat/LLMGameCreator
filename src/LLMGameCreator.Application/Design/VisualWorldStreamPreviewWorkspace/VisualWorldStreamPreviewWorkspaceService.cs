using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMGameCreator.Application.Design.VisualWorldStreamPreviewWorkspace;

public sealed partial class VisualWorldStreamPreviewWorkspaceService
{
    public const string ReportMarkdownFileName = "unity-handoff-inspector-report.md";
    public const string CatalogJsonFileName = "unity-handoff-inspector-catalog.json";
    public const string ProofStatusJsonFileName = "unity-handoff-inspector-proof-status.json";
    public const string WinFormsBindingInventoryJsonFileName =
        "unity-handoff-inspector-winforms-binding-inventory.json";
    public const string QualityGateScanJsonFileName =
        "unity-handoff-inspector-quality-gate-scan.json";
    public const string SourceHealthScanJsonFileName =
        "unity-handoff-inspector-source-health-scan.json";

    private const int MaxPreviewCharacters = 32000;
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static VisualWorldStreamPreviewWorkspaceService()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public VisualWorldStreamPreviewWorkspaceResult Build(string projectRootPath)
    {
        var projectRoot = Path.GetFullPath(projectRootPath);
        var diagnostics = new List<VisualWorldPreviewDiagnostic>();
        var svgEntries = new List<VisualWorldPreviewSvgEntry>();
        var groups = new List<VisualWorldPreviewArtifactGroup>
        {
            BuildMicrotileGroup(projectRoot, diagnostics, svgEntries),
            BuildMapPatchGroup(projectRoot, diagnostics, svgEntries),
            BuildRegionGroup(projectRoot, diagnostics, svgEntries),
            BuildWorldProfileGroup(projectRoot, diagnostics, svgEntries),
            BuildChunkStreamGroup(projectRoot, diagnostics, svgEntries),
            BuildCacheExportGroup(projectRoot, diagnostics),
            BuildUnityHandoffGroup(projectRoot, diagnostics),
            BuildGeoworldGroup(projectRoot, diagnostics, svgEntries),
            BuildOfflineGeoworldHandoffGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldUnityPreviewGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldUnityEditorPreviewGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldPlayModeTravelGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldInteractiveTravelGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldInteractionGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldSessionReplayGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldObjectiveAcceptanceGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldAlphaSliceGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldAlphaExportPackageGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldAlphaManualAcceptanceGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldAlphaManualResultIntakeGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldAlphaAcceptanceOperatorPackGroup(projectRoot, diagnostics),
            BuildOfflineGeoworldAlphaManualResultWorkbenchGroup(projectRoot, diagnostics)
        };

        var proofStatus = BuildProofStatus(projectRoot, diagnostics);
        var bindingInventory = BuildWinFormsBindingInventory(projectRoot);
        var sourceHealth = VisualWorldStreamPreviewSourceHealthScanner.ScanGoal092Namespace(projectRoot);
        var qualityGate = BuildQualityGate(
            groups,
            svgEntries,
            proofStatus,
            bindingInventory,
            sourceHealth,
            diagnostics);
        var catalog = new VisualWorldStreamPreviewCatalog
        {
            Accepted = false,
            GroupCount = groups.Count,
            EntryCount = groups.Sum(group => group.EntryCount),
            SvgTextPreviewCount = svgEntries.Count,
            Groups = groups,
            SvgEntries = svgEntries.OrderBy(item => item.RelativePath, StringComparer.Ordinal).ToList()
        };

        var proofDocument = new VisualWorldStreamPreviewProofStatusDocument
        {
            Passed = proofStatus.All(item => item.Passed),
            ProofCount = proofStatus.Count,
            Proofs = proofStatus
        };

        var catalogJson = Serialize(catalog);
        var proofStatusJson = Serialize(proofDocument);
        var bindingJson = Serialize(bindingInventory);
        var qualityJson = Serialize(qualityGate);
        var sourceHealthJson = Serialize(sourceHealth);
        var reportWithoutHash = BuildReport(
            catalog,
            proofDocument,
            bindingInventory,
            qualityGate,
            catalogJson,
            proofStatusJson,
            bindingJson,
            qualityJson,
            sourceHealthJson);
        var reportMarkdownWithoutHash = RenderReport(
            reportWithoutHash,
            catalog,
            proofDocument,
            bindingInventory,
            qualityGate,
            deterministicReportHash: string.Empty);
        var report = reportWithoutHash with
        {
            DeterministicReportHash = Sha256Text(reportMarkdownWithoutHash)
        };
        var reportMarkdown = RenderReport(
            report,
            catalog,
            proofDocument,
            bindingInventory,
            qualityGate,
            report.DeterministicReportHash);

        return new VisualWorldStreamPreviewWorkspaceResult
        {
            Catalog = catalog,
            ProofStatus = proofDocument,
            WinFormsBindingInventory = bindingInventory,
            QualityGateScan = qualityGate,
            SourceHealthScan = sourceHealth,
            Report = report,
            CatalogJson = catalogJson,
            ProofStatusJson = proofStatusJson,
            WinFormsBindingInventoryJson = bindingJson,
            QualityGateScanJson = qualityJson,
            SourceHealthScanJson = sourceHealthJson,
            ReportMarkdown = reportMarkdown,
            Diagnostics = diagnostics
                .Concat(bindingInventory.Diagnostics)
                .Concat(sourceHealth.Diagnostics)
                .Concat(qualityGate.Diagnostics)
                .OrderBy(item => item.Code, StringComparer.Ordinal)
                .ThenBy(item => item.Target, StringComparer.Ordinal)
                .ToList()
        };
    }

    public async Task<VisualWorldStreamPreviewWorkspaceWriteResult> BuildAndWriteAsync(
        string projectRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(projectRootPath);
        return await WriteAsync(projectRootPath, result, cancellationToken).ConfigureAwait(false);
    }

    public async Task<VisualWorldStreamPreviewWorkspaceWriteResult> BuildAndWriteAsync(
        string sourceRootPath,
        string outputRootPath,
        CancellationToken cancellationToken = default)
    {
        var result = Build(sourceRootPath);
        return await WriteAsync(outputRootPath, result, cancellationToken).ConfigureAwait(false);
    }
}
