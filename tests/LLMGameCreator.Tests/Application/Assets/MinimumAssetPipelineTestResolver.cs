using System.Security.Cryptography;
using System.Text;
using LLMGameCreator.Application.Design.Assets;

namespace LLMGameCreator.Tests.Application.Assets;

public static class MinimumAssetPipelineAcceptanceTestFactory
{
    public static MinimumAssetPipelineAcceptanceService CreateService(
        IMinimumAssetPipelineResolver? resolver = null) =>
        new(resolver ?? new RealMinimumAssetPipelineResolver());
}

public sealed class RealMinimumAssetPipelineResolver : IMinimumAssetPipelineResolver
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public string ResolverId => "real_minimum_asset_pipeline_fixture_resolver";
    public bool IsAvailable => true;

    public MinimumAssetResolveEvidence Resolve(MinimumAssetResolveRequest request)
    {
        var source = request.SourcePack.Sources.Single(item => item.SourceId == request.Request.SourceId);
        var relativePath = Path.Combine(
                MinimumAssetPipelineAcceptanceService.RelativeOutputDirectory,
                "assets",
                SafeSegment(request.SourcePack.PackId),
                SafeSegment(request.Request.SlotId) + ".fixture")
            .Replace('\\', '/');
        var outputPath = Path.GetFullPath(Path.Combine(request.ProjectRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        byte[] bytes;
        var diagnostics = new List<MinimumAssetPipelineDiagnostic>();
        if (source.Kind == "local_fixture")
        {
            var sourcePath = Path.GetFullPath(Path.Combine(request.SourcePackDirectoryPath, source.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
            bytes = File.ReadAllBytes(sourcePath);
            File.WriteAllBytes(outputPath, bytes);
        }
        else if (source.Kind == "deterministic_fallback")
        {
            bytes = Utf8WithoutBom.GetBytes(
                "LLMGC_FIXTURE_MEDIA:" + request.Request.MediaType + "\n" +
                "resolution:fallback\n" +
                "slot:" + request.Request.SlotId + "\n" +
                "content:" + request.Request.ContentId + "\n" +
                "source:" + request.Request.SourceId + "\n");
            File.WriteAllBytes(outputPath, bytes);
        }
        else
        {
            diagnostics.Add(new MinimumAssetPipelineDiagnostic
            {
                Severity = "error",
                Code = "asset_pipeline.resolver.unknown_source_kind",
                Target = request.Request.SlotId,
                Message = "The concrete resolver only supports local fixtures and deterministic fallback sources."
            });
            bytes = [];
        }

        if (diagnostics.Count > 0)
        {
            return new MinimumAssetResolveEvidence { Diagnostics = diagnostics };
        }

        return new MinimumAssetResolveEvidence
        {
            ResolvedAsset = new ResolvedMinimumAsset
            {
                SlotId = request.Request.SlotId,
                AssetId = request.Request.SlotId.Replace("asset-slot/", "asset/", StringComparison.Ordinal),
                Category = request.Request.Category,
                MediaType = request.Request.MediaType,
                ContentId = request.Request.ContentId,
                SourceId = request.Request.SourceId,
                SourceKind = request.Request.SourceKind,
                ResolutionKind = source.Kind == "local_fixture" ? "import" : "fallback",
                RelativePath = relativePath,
                Hash = ComputeHash(bytes),
                ByteCount = bytes.LongLength
            }
        };
    }

    private static string ComputeHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string SafeSegment(string value)
    {
        var builder = new StringBuilder();
        foreach (var ch in value.ToLowerInvariant())
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
            {
                builder.Append(ch);
            }
            else if (ch is '/' or '_' or '-' or '.')
            {
                builder.Append('-');
            }
        }

        var safe = builder.ToString().Trim('-');
        while (safe.Contains("--", StringComparison.Ordinal))
        {
            safe = safe.Replace("--", "-", StringComparison.Ordinal);
        }

        return safe.Length == 0 ? "id" : safe;
    }
}
