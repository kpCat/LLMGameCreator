using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Runtime.Abstractions;
using RuntimeInteractiveSession = LLMGameCreator.Runtime.Abstractions.SelectedRuntimeVariantInteractiveSession;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public sealed record GameProjectPackageIdentityOverlayResult
{
    public string CompositionPackagePath { get; init; } = string.Empty;
    public string CompositionPackageSha256 { get; init; } = string.Empty;
    public string ActivatedProjectPackagePath { get; init; } = string.Empty;
    public string ActivatedProjectPackageSha256 { get; init; } = string.Empty;
}

public sealed class GameProjectIdentityRuntimeQualificationAdapter : ISelectedRuntimeVariantInteractiveSessionService
{
    private readonly ISelectedRuntimeVariantInteractiveSessionService _runtime;
    private readonly GameManifest _compositionManifest;

    public GameProjectIdentityRuntimeQualificationAdapter(
        ISelectedRuntimeVariantInteractiveSessionService runtime,
        GameManifest compositionManifest)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _compositionManifest = compositionManifest ?? throw new ArgumentNullException(nameof(compositionManifest));
    }

    public RuntimeInteractiveSession StartSession(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSessionStartRequest request) =>
        _runtime.StartSession(WithCompositionIdentity(package), request);

    public SelectedRuntimeVariantInteractiveActionResult ExecuteAction(
        GamePackageDefinition package,
        RuntimeInteractiveSession session,
        SelectedRuntimeVariantInteractiveActionRequest request) =>
        _runtime.ExecuteAction(WithCompositionIdentity(package), session, request);

    public SelectedRuntimeVariantInteractiveCheckpoint SaveCheckpoint(
        RuntimeInteractiveSession session,
        string checkpointId,
        string createdAtUtc) =>
        _runtime.SaveCheckpoint(session, checkpointId, createdAtUtc);

    public SelectedRuntimeVariantInteractiveReplayResult ReloadCheckpoint(
        GamePackageDefinition package,
        SelectedRuntimeVariantInteractiveSessionStartRequest request,
        SelectedRuntimeVariantInteractiveCheckpoint checkpoint) =>
        _runtime.ReloadCheckpoint(WithCompositionIdentity(package), request, checkpoint);

    private GamePackageDefinition WithCompositionIdentity(GamePackageDefinition package) => new()
    {
        Manifest = new GameManifest
        {
            PackageId = _compositionManifest.PackageId,
            Title = _compositionManifest.Title,
            Version = _compositionManifest.Version,
            FormatVersion = _compositionManifest.FormatVersion,
            StartMapId = package.Manifest.StartMapId,
            Description = _compositionManifest.Description
        },
        Game = package.Game,
        AssetCatalog = package.AssetCatalog,
        ScriptCatalog = package.ScriptCatalog,
        GeneratedContent = package.GeneratedContent
    };
}

public sealed class GameProjectPackageIdentityOverlayService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public GameProjectPackageIdentityOverlayResult Overlay(
        string compositionPackagePath,
        string activatedProjectPackagePath,
        GameProjectIdentityDocument identity)
    {
        GameProjectIdentityStore.Validate(identity);
        var root = JsonNode.Parse(File.ReadAllText(compositionPackagePath, Encoding.UTF8))?.AsObject()
                   ?? throw new InvalidOperationException("Composition package JSON root must be an object.");
        var manifest = root["manifest"] as JsonObject
                       ?? throw new InvalidOperationException("Composition package manifest must be an object.");
        manifest["packageId"] = identity.PackageId;
        manifest["title"] = identity.Title;
        manifest["version"] = identity.Version;
        manifest["formatVersion"] = identity.FormatVersion;
        manifest["description"] = identity.Description;

        var json = root.ToJsonString(JsonOptions) + Environment.NewLine;
        Directory.CreateDirectory(Path.GetDirectoryName(activatedProjectPackagePath)!);
        File.WriteAllText(activatedProjectPackagePath, json, new UTF8Encoding(false));
        return new GameProjectPackageIdentityOverlayResult
        {
            CompositionPackagePath = compositionPackagePath,
            CompositionPackageSha256 = HashFile(compositionPackagePath),
            ActivatedProjectPackagePath = activatedProjectPackagePath,
            ActivatedProjectPackageSha256 = HashFile(activatedProjectPackagePath)
        };
    }

    public static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
