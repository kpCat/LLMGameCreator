using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Composition;

internal sealed class UnityArchiveAssetRequestBuilder
{
private readonly UnityArchiveRequestBuildContext _context;
    private readonly List<UnityArchiveRequestDiagnostic> _diagnostics;
    private readonly List<UnityArchiveAssetRequest> _assetRequests;
    private readonly IReadOnlyList<string> _styleTags;

    public UnityArchiveAssetRequestBuilder(UnityArchiveRequestBuildContext context)
    {
        _context = context;
        _diagnostics = new List<UnityArchiveRequestDiagnostic>();
        _assetRequests = new List<UnityArchiveAssetRequest>();
        _styleTags = context.StyleTags;
    }

    public (List<UnityArchiveAssetRequest> Requests, List<UnityArchiveRequestDiagnostic> Diagnostics) Build()
    {
        BuildSceneIllustrations();
        BuildMapBackgrounds();
        BuildNpcPortraits();
        BuildItemIcons();
        BuildAbilityIcons();
        BuildMechanicIcons();
        BuildTileTextures();
        BuildUiTheme();
        BuildUiWidgets();

        return (_assetRequests, _diagnostics);
    }

    private void BuildSceneIllustrations()
    {
        if (!_context.HasScenes) return;

        foreach (var scene in _context.Package!.GeneratedContent.Scenes)
        {
            AddAsset(
                UnityArchiveAssetKind.scene_illustration,
                $"illustration.scene.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(scene.SourceId)}",
                scene.SourceId,
                "generated_scene",
                $"Scene illustration for '{scene.Title}' generated from source '{scene.SourceId}'.",
                UnityArchiveRequestProviderKind.manual_import);
        }
    }

    private void BuildMapBackgrounds()
    {
        if (!_context.HasScenes) return;

        foreach (var map in _context.Package!.Game.Maps)
        {
            AddAsset(
                UnityArchiveAssetKind.background,
                $"background.map.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(map.Id)}",
                map.Id,
                "package_map",
                $"Map background for '{map.Name}' from package map '{map.Id}'.",
                UnityArchiveRequestProviderKind.manual_import);
        }
    }

    private void BuildNpcPortraits()
    {
        if (!_context.HasNpcs) return;

        foreach (var npc in _context.Package!.GeneratedContent.Npcs)
        {
            AddAsset(
                UnityArchiveAssetKind.portrait,
                $"portrait.npc.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(npc.SourceId)}",
                npc.SourceId,
                "generated_npc",
                $"Portrait for NPC '{npc.Name}' generated from source '{npc.SourceId}'.",
                UnityArchiveRequestProviderKind.manual_import);
        }
    }

    private void BuildItemIcons()
    {
        if (!_context.HasItems) return;

        foreach (var item in _context.Package!.Game.Items)
        {
            AddAsset(
                UnityArchiveAssetKind.icon,
                $"icon.item.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(item.Id)}",
                item.Id,
                "package_item",
                $"Icon for item '{item.Name}' (kind={item.Kind}) from package.",
                UnityArchiveRequestProviderKind.manual_import);
        }

        foreach (var item in _context.Package!.GeneratedContent.Items)
        {
            AddAsset(
                UnityArchiveAssetKind.icon,
                $"icon.item.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(item.SourceId)}",
                item.SourceId,
                "generated_item",
                $"Icon for generated item '{item.Name}' from source '{item.SourceId}'.",
                UnityArchiveRequestProviderKind.manual_import);
        }
    }

    private void BuildAbilityIcons()
    {
        if (!_context.HasAbilities) return;

        foreach (var ability in _context.Package!.Game.Abilities)
        {
            AddAsset(
                UnityArchiveAssetKind.icon,
                $"icon.ability.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(ability.Id)}",
                ability.Id,
                "package_ability",
                $"Icon for ability '{ability.Name}' (kind={ability.Kind}) from package.",
                UnityArchiveRequestProviderKind.manual_import);
        }
    }

    private void BuildMechanicIcons()
    {
        if (!_context.HasMechanics) return;

        foreach (var mechanic in _context.Package!.GeneratedContent.Mechanics)
        {
            AddAsset(
                UnityArchiveAssetKind.icon,
                $"icon.mechanic.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(mechanic.SourceId)}",
                mechanic.SourceId,
                "generated_mechanic",
                $"Icon for mechanic '{mechanic.Name}' generated from source '{mechanic.SourceId}'.",
                UnityArchiveRequestProviderKind.manual_import);
        }
    }

    private void BuildTileTextures()
    {
        if (!_context.HasTilePrototypes) return;

        foreach (var tile in _context.Package!.Game.TilePrototypes)
        {
            AddAsset(
                UnityArchiveAssetKind.tile_texture,
                $"tile.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(tile.Id)}",
                tile.Id,
                "package_tile_prototype",
                $"Terrain texture for tile prototype '{tile.Name}' (id={tile.Id}).",
                UnityArchiveRequestProviderKind.manual_import);
        }
    }

    private void BuildUiTheme()
    {
        if (!_context.HasDynamicUi) return;

        AddAsset(
            UnityArchiveAssetKind.ui_theme,
            $"ui.theme.{_context.TargetProfile.TargetProfileId}",
            _context.TargetProfile.TargetProfileId,
            "unity_target_profile",
            $"UI theme for target profile '{_context.TargetProfile.TargetProfileId}'.",
            UnityArchiveRequestProviderKind.comfyui_future);
    }

    private void BuildUiWidgets()
    {
        if (!_context.HasDynamicUi) return;

        foreach (var layout in _context.ArchiveManifest.UiLayouts)
        {
            foreach (var panel in layout.Panels)
            {
                foreach (var widget in panel.Widgets)
                {
                    AddAsset(
                        UnityArchiveAssetKind.ui_widget,
                        $"ui.widget.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(widget.WidgetId)}",
                        widget.WidgetId,
                        "ui_layout_widget",
                        $"Widget asset for '{widget.WidgetId}' (kind={widget.WidgetKind}) in layout '{layout.LayoutId}'.",
                        UnityArchiveRequestProviderKind.comfyui_future);
                }
            }
        }
    }

    private void AddAsset(UnityArchiveAssetKind kind, string assetId, string sourceId, string sourceKind, string prompt, UnityArchiveRequestProviderKind provider)
    {
        var id = $"asset-request.{kind.ToString().ToLowerInvariant()}.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(sourceId)}";
        if (string.IsNullOrWhiteSpace(id))
        {
            _diagnostics.Add(UnityArchiveRequestDiagnosticsBuilder.Error(
                "request.diagnostic.blank_asset_request_id",
                $"Blank asset request id for {sourceKind}:{sourceId}.",
                sourceId));
            return;
        }

        _assetRequests.Add(new UnityArchiveAssetRequest
        {
            RequestId = id,
            AssetId = assetId,
            AssetKind = kind,
            ProviderKind = provider,
            SourceRef = new UnityArchiveRequestSourceRef { SourceId = sourceId, SourceKind = sourceKind },
            PromptOrInstruction = prompt,
            StyleTags = _styleTags,
            Metadata = new Dictionary<string, string>
            {
                ["design_brief_id"] = _context.DesignBrief.BriefId,
                ["target_profile_id"] = _context.TargetProfile.TargetProfileId
            }
        });
    }
}