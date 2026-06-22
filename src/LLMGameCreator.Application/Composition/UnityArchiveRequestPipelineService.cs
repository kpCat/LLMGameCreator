namespace LLMGameCreator.Application.Composition;

public sealed class UnityArchiveAssetAudioLuaRequestService
{
    public UnityArchiveRequestPipelineResult BuildRequests(UnityArchiveRequestPipelineRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.DesignBrief);
        ArgumentNullException.ThrowIfNull(request.TargetProfile);
        ArgumentNullException.ThrowIfNull(request.ArchiveManifest);
        if (string.IsNullOrWhiteSpace(request.ProjectRootPath))
        {
            throw new ArgumentException("Project root path is required.", nameof(request));
        }

        var diagnostics = new List<UnityArchiveRequestDiagnostic>();
        var assetRequests = new List<UnityArchiveAssetRequest>();
        var audioRequests = new List<UnityArchiveAudioRequest>();
        var luaModuleRequests = new List<UnityArchiveLuaModuleRequest>();

        var package = request.Package;
        var brief = request.DesignBrief;
        var profile = request.TargetProfile;
        var moduleIds = new HashSet<string>(request.ArchiveManifest.RuntimeModuleIds, StringComparer.OrdinalIgnoreCase);

        bool hasItems = package is not null && (package.Game.Items.Count > 0 || package.GeneratedContent.Items.Count > 0);
        bool hasNpcs = package is not null && package.GeneratedContent.Npcs.Count > 0;
        bool hasScenes = package is not null && (package.Game.Maps.Count > 0 || package.GeneratedContent.Scenes.Count > 0);
        bool hasAbilities = package is not null && package.Game.Abilities.Count > 0;
        bool hasMechanics = package is not null && package.GeneratedContent.Mechanics.Count > 0;
        bool hasTilePrototypes = package is not null && package.Game.TilePrototypes.Count > 0;
        bool hasQuests = package is not null && (package.Game.Quests.Count > 0 || package.GeneratedContent.Quests.Count > 0);
        bool hasDialogues = package is not null && (package.Game.Dialogues.Count > 0 || package.GeneratedContent.Dialogues.Count > 0);
        bool hasCrafting = package is not null && package.Game.Recipes.Count > 0;
        bool hasFactions = package is not null && package.Game.Factions.Count > 0;
        bool hasCombatModule = moduleIds.Contains("unity.gameplay.personal_combat");
        bool hasUiLayouts = request.ArchiveManifest.UiLayouts.Count > 0;
        bool hasDynamicUi = brief.ViewModeWishes.Any(w => w.Required) || hasUiLayouts;

        var styleTags = brief.AssetStyleWishes
            .Select(w => w.WishId.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        void AddAsset(UnityArchiveAssetKind kind, string assetId, string sourceId, string sourceKind, string prompt, UnityArchiveRequestProviderKind provider)
        {
            var id = $"asset-request.{kind.ToString().ToLowerInvariant()}.{NormalizeId(sourceId)}";
            if (string.IsNullOrWhiteSpace(id))
            {
                diagnostics.Add(Error("request.diagnostic.blank_asset_request_id", $"Blank asset request id for {sourceKind}:{sourceId}.", sourceId));
                return;
            }

            assetRequests.Add(new UnityArchiveAssetRequest
            {
                RequestId = id,
                AssetId = assetId,
                AssetKind = kind,
                ProviderKind = provider,
                SourceRef = new UnityArchiveRequestSourceRef { SourceId = sourceId, SourceKind = sourceKind },
                PromptOrInstruction = prompt,
                StyleTags = styleTags,
                Metadata = new Dictionary<string, string>
                {
                    ["design_brief_id"] = brief.BriefId,
                    ["target_profile_id"] = profile.TargetProfileId
                }
            });
        }

        void ValidateAssetIds()
        {
            foreach (var duplicateId in assetRequests
                         .GroupBy(r => r.RequestId, StringComparer.OrdinalIgnoreCase)
                         .Where(g => g.Count() > 1)
                         .Select(g => g.Key)
                         .OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
            {
                diagnostics.Add(Error("request.diagnostic.duplicate_asset_request_id", $"Duplicate asset request id '{duplicateId}'.", duplicateId));
            }
        }

        void AddAudio(UnityArchiveAudioKind kind, string audioId, string sourceId, string sourceKind, string prompt, bool loop, UnityArchiveRequestProviderKind provider)
        {
            var id = $"audio-request.{kind.ToString().ToLowerInvariant()}.{NormalizeId(sourceId)}";
            if (string.IsNullOrWhiteSpace(id))
            {
                diagnostics.Add(Error("request.diagnostic.blank_audio_request_id", $"Blank audio request id for {sourceKind}:{sourceId}.", sourceId));
                return;
            }

            audioRequests.Add(new UnityArchiveAudioRequest
            {
                RequestId = id,
                AudioId = audioId,
                AudioKind = kind,
                ProviderKind = provider,
                SourceRef = new UnityArchiveRequestSourceRef { SourceId = sourceId, SourceKind = sourceKind },
                PromptOrInstruction = prompt,
                Loop = loop,
                Metadata = new Dictionary<string, string>
                {
                    ["design_brief_id"] = brief.BriefId,
                    ["target_profile_id"] = profile.TargetProfileId
                }
            });
        }

        void ValidateAudioIds()
        {
            foreach (var duplicateId in audioRequests
                         .GroupBy(r => r.RequestId, StringComparer.OrdinalIgnoreCase)
                         .Where(g => g.Count() > 1)
                         .Select(g => g.Key)
                         .OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
            {
                diagnostics.Add(Error("request.diagnostic.duplicate_audio_request_id", $"Duplicate audio request id '{duplicateId}'.", duplicateId));
            }
        }

        void AddLua(UnityArchiveLuaModuleKind kind, string moduleId, string sourceId, string sourceKind, string prompt, UnityArchiveRequestProviderKind provider)
        {
            luaModuleRequests.Add(new UnityArchiveLuaModuleRequest
            {
                ModuleId = $"lua-request.{kind.ToString().ToLowerInvariant()}",
                ModuleKind = kind,
                ProviderKind = provider,
                SourceRef = new UnityArchiveRequestSourceRef { SourceId = sourceId, SourceKind = sourceKind },
                PromptOrInstruction = prompt,
                Metadata = new Dictionary<string, string>
                {
                    ["design_brief_id"] = brief.BriefId,
                    ["target_profile_id"] = profile.TargetProfileId,
                    ["required_runtime_module_id"] = moduleId
                }
            });
        }

        void ValidateLuaIds()
        {
            foreach (var duplicateId in luaModuleRequests
                         .GroupBy(r => r.ModuleId, StringComparer.OrdinalIgnoreCase)
                         .Where(g => g.Count() > 1)
                         .Select(g => g.Key)
                         .OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
            {
                diagnostics.Add(Error("request.diagnostic.duplicate_lua_request_id", $"Duplicate Lua module request id '{duplicateId}'.", duplicateId));
            }
        }

        // Asset requests from existing package data

        if (hasScenes)
        {
            if (package!.GeneratedContent.Scenes.Count > 0)
            {
                foreach (var scene in package.GeneratedContent.Scenes)
                {
                    AddAsset(
                        UnityArchiveAssetKind.scene_illustration,
                        $"illustration.scene.{NormalizeId(scene.SourceId)}",
                        scene.SourceId,
                        "generated_scene",
                        $"Scene illustration for '{scene.Title}' generated from source '{scene.SourceId}'.",
                        UnityArchiveRequestProviderKind.manual_import);
                }
            }

            if (package.Game.Maps.Count > 0)
            {
                foreach (var map in package.Game.Maps)
                {
                    AddAsset(
                        UnityArchiveAssetKind.background,
                        $"background.map.{NormalizeId(map.Id)}",
                        map.Id,
                        "package_map",
                        $"Map background for '{map.Name}' from package map '{map.Id}'.",
                        UnityArchiveRequestProviderKind.manual_import);
                }
            }
        }

        if (hasNpcs)
        {
            foreach (var npc in package!.GeneratedContent.Npcs)
            {
                AddAsset(
                    UnityArchiveAssetKind.portrait,
                    $"portrait.npc.{NormalizeId(npc.SourceId)}",
                    npc.SourceId,
                    "generated_npc",
                    $"Portrait for NPC '{npc.Name}' generated from source '{npc.SourceId}'.",
                    UnityArchiveRequestProviderKind.manual_import);
            }
        }

        if (hasItems)
        {
            foreach (var item in package!.Game.Items)
            {
                AddAsset(
                    UnityArchiveAssetKind.icon,
                    $"icon.item.{NormalizeId(item.Id)}",
                    item.Id,
                    "package_item",
                    $"Icon for item '{item.Name}' (kind={item.Kind}) from package.",
                    UnityArchiveRequestProviderKind.manual_import);
            }

            foreach (var item in package!.GeneratedContent.Items)
            {
                AddAsset(
                    UnityArchiveAssetKind.icon,
                    $"icon.item.{NormalizeId(item.SourceId)}",
                    item.SourceId,
                    "generated_item",
                    $"Icon for generated item '{item.Name}' from source '{item.SourceId}'.",
                    UnityArchiveRequestProviderKind.manual_import);
            }
        }

        if (hasAbilities)
        {
            foreach (var ability in package!.Game.Abilities)
            {
                AddAsset(
                    UnityArchiveAssetKind.icon,
                    $"icon.ability.{NormalizeId(ability.Id)}",
                    ability.Id,
                    "package_ability",
                    $"Icon for ability '{ability.Name}' (kind={ability.Kind}) from package.",
                    UnityArchiveRequestProviderKind.manual_import);
            }
        }

        if (hasMechanics)
        {
            foreach (var mechanic in package!.GeneratedContent.Mechanics)
            {
                AddAsset(
                    UnityArchiveAssetKind.icon,
                    $"icon.mechanic.{NormalizeId(mechanic.SourceId)}",
                    mechanic.SourceId,
                    "generated_mechanic",
                    $"Icon for mechanic '{mechanic.Name}' generated from source '{mechanic.SourceId}'.",
                    UnityArchiveRequestProviderKind.manual_import);
            }
        }

        if (hasTilePrototypes)
        {
            foreach (var tile in package!.Game.TilePrototypes)
            {
                AddAsset(
                    UnityArchiveAssetKind.tile_texture,
                    $"tile.{NormalizeId(tile.Id)}",
                    tile.Id,
                    "package_tile_prototype",
                    $"Terrain texture for tile prototype '{tile.Name}' (id={tile.Id}).",
                    UnityArchiveRequestProviderKind.manual_import);
            }
        }

        if (hasUiLayouts)
        {
            AddAsset(
                UnityArchiveAssetKind.ui_theme,
                $"ui.theme.{profile.TargetProfileId}",
                profile.TargetProfileId,
                "unity_target_profile",
                $"UI theme for target profile '{profile.TargetProfileId}'.",
                UnityArchiveRequestProviderKind.comfyui_future);

            foreach (var layout in request.ArchiveManifest.UiLayouts)
            {
                foreach (var panel in layout.Panels)
                {
                    foreach (var widget in panel.Widgets)
                    {
                        AddAsset(
                            UnityArchiveAssetKind.ui_widget,
                            $"ui.widget.{NormalizeId(widget.WidgetId)}",
                            widget.WidgetId,
                            "ui_layout_widget",
                            $"Widget asset for '{widget.WidgetId}' (kind={widget.WidgetKind}) in layout '{layout.LayoutId}'.",
                            UnityArchiveRequestProviderKind.comfyui_future);
                    }
                }
            }
        }

        ValidateAssetIds();

        foreach (var futureAsset in assetRequests.Where(r => IsFutureProvider(r.ProviderKind)))
        {
            diagnostics.Add(Warning(
                "request.diagnostic.future_provider_kind",
                $"Asset request '{futureAsset.RequestId}' uses future provider kind '{futureAsset.ProviderKind}'.",
                futureAsset.RequestId));
        }

        // Audio requests from existing package data

        if (hasDynamicUi)
        {
            AddAudio(UnityArchiveAudioKind.ui_sfx, "sfx.ui.confirm", "ui.confirm", "ui_layout", "Short UI confirm sound.", false, UnityArchiveRequestProviderKind.local_audio_future);
            AddAudio(UnityArchiveAudioKind.ui_sfx, "sfx.ui.cancel", "ui.cancel", "ui_layout", "Short UI cancel sound.", false, UnityArchiveRequestProviderKind.local_audio_future);
            AddAudio(UnityArchiveAudioKind.ui_sfx, "sfx.ui.click", "ui.click", "ui_layout", "Short UI click/select sound.", false, UnityArchiveRequestProviderKind.local_audio_future);
        }

        if (hasTilePrototypes)
        {
            foreach (var tile in package!.Game.TilePrototypes)
            {
                AddAudio(
                    UnityArchiveAudioKind.footstep,
                    $"sfx.footstep.{NormalizeId(tile.Id)}",
                    tile.Id,
                    "package_tile_prototype",
                    $"Footstep sound for tile prototype '{tile.Name}' (id={tile.Id}).",
                    true,
                    UnityArchiveRequestProviderKind.local_audio_future);
            }
        }

        if (hasAbilities)
        {
            foreach (var ability in package!.Game.Abilities)
            {
                AddAudio(
                    UnityArchiveAudioKind.ability,
                    $"sfx.ability.{NormalizeId(ability.Id)}",
                    ability.Id,
                    "package_ability",
                    $"Ability sound effect for '{ability.Name}' (id={ability.Id}).",
                    false,
                    UnityArchiveRequestProviderKind.local_audio_future);
            }
        }
        else if (hasMechanics)
        {
            foreach (var mechanic in package!.GeneratedContent.Mechanics)
            {
                AddAudio(
                    UnityArchiveAudioKind.ability,
                    $"sfx.ability.{NormalizeId(mechanic.SourceId)}",
                    mechanic.SourceId,
                    "generated_mechanic",
                    $"Ability sound effect for mechanic '{mechanic.Name}' generated from source '{mechanic.SourceId}'.",
                    false,
                    UnityArchiveRequestProviderKind.local_audio_future);
            }
        }

        if (hasScenes)
        {
            if (package!.GeneratedContent.Scenes.Count > 0)
            {
                foreach (var scene in package.GeneratedContent.Scenes)
                {
                    AddAudio(
                        UnityArchiveAudioKind.scene_ambience,
                        $"ambience.scene.{NormalizeId(scene.SourceId)}",
                        scene.SourceId,
                        "generated_scene",
                        $"Scene ambience for '{scene.Title}' generated from source '{scene.SourceId}'.",
                        true,
                        UnityArchiveRequestProviderKind.local_audio_future);
                }
            }

            if (package.Game.Maps.Count > 0)
            {
                foreach (var map in package.Game.Maps)
                {
                    AddAudio(
                        UnityArchiveAudioKind.scene_ambience,
                        $"ambience.map.{NormalizeId(map.Id)}",
                        map.Id,
                        "package_map",
                        $"Map ambience for '{map.Name}' from package map '{map.Id}'.",
                        true,
                        UnityArchiveRequestProviderKind.local_audio_future);
                }
            }
        }

        if (brief.AudioStyleWishes.Count > 0)
        {
            foreach (var wish in brief.AudioStyleWishes)
            {
                AddAudio(
                    UnityArchiveAudioKind.music,
                    $"music.theme.{NormalizeId(wish.WishId)}",
                    wish.WishId,
                    "design_brief_audio_wish",
                    $"Music theme slot for audio style wish '{wish.Description}' (id={wish.WishId}).",
                    true,
                    UnityArchiveRequestProviderKind.suno_future);
            }
        }

        ValidateAudioIds();

        foreach (var futureAudio in audioRequests.Where(r => IsFutureProvider(r.ProviderKind)))
        {
            diagnostics.Add(Warning(
                "request.diagnostic.future_provider_kind",
                $"Audio request '{futureAudio.RequestId}' uses future provider kind '{futureAudio.ProviderKind}'.",
                futureAudio.RequestId));
        }

        // Lua module requests from target modules and package data

        if (HasLuaModule(moduleIds, "unity.gameplay.inventory") || hasItems)
        {
            AddLua(UnityArchiveLuaModuleKind.inventory, "unity.gameplay.inventory", brief.BriefId, "design_brief", "Inventory system Lua/data module.", UnityArchiveRequestProviderKind.none);
        }

        if (HasLuaModule(moduleIds, "unity.gameplay.quest_journal") || hasQuests)
        {
            AddLua(UnityArchiveLuaModuleKind.quest_journal, "unity.gameplay.quest_journal", brief.BriefId, "design_brief", "Quest journal Lua/data module.", UnityArchiveRequestProviderKind.none);
        }

        if (HasLuaModule(moduleIds, "unity.gameplay.dialogue") || hasDialogues)
        {
            AddLua(UnityArchiveLuaModuleKind.dialogue, "unity.gameplay.dialogue", brief.BriefId, "design_brief", "Dialogue Lua/data module.", UnityArchiveRequestProviderKind.none);
        }

        if (HasLuaModule(moduleIds, "unity.gameplay.personal_combat") || hasCombatModule)
        {
            AddLua(UnityArchiveLuaModuleKind.combat, "unity.gameplay.personal_combat", brief.BriefId, "design_brief", "Personal combat Lua/data module.", UnityArchiveRequestProviderKind.none);
        }

        if (HasLuaModule(moduleIds, "unity.gameplay.crafting") || hasCrafting)
        {
            AddLua(UnityArchiveLuaModuleKind.crafting, "unity.gameplay.crafting", brief.BriefId, "design_brief", "Crafting Lua/data module.", UnityArchiveRequestProviderKind.none);
        }

        if (hasItems || hasAbilities || hasMechanics)
        {
            AddLua(UnityArchiveLuaModuleKind.stats, "unity.gameplay.stats", brief.BriefId, "design_brief", "Stats Lua/data module.", UnityArchiveRequestProviderKind.none);
        }

        if (hasScenes)
        {
            AddLua(UnityArchiveLuaModuleKind.world_map, "unity.world.topdown_map", brief.BriefId, "design_brief", "World map Lua/data module.", UnityArchiveRequestProviderKind.none);
        }

        if (hasFactions)
        {
            AddLua(UnityArchiveLuaModuleKind.factions, "unity.gameplay.factions", brief.BriefId, "design_brief", "Factions Lua/data module.", UnityArchiveRequestProviderKind.none);
        }

        if (HasLuaModule(moduleIds, "unity.transport.vehicle_future"))
        {
            AddLua(UnityArchiveLuaModuleKind.transport_future, "unity.transport.vehicle_future", brief.BriefId, "design_brief", "Vehicle transport Lua/data module (future).", UnityArchiveRequestProviderKind.procedural_future);
            diagnostics.Add(Warning("request.diagnostic.future_lua_module", "Planned future Lua module 'unity.transport.vehicle_future' is metadata-only.", "unity.transport.vehicle_future"));
        }

        if (HasLuaModule(moduleIds, "unity.crime.police_future"))
        {
            AddLua(UnityArchiveLuaModuleKind.police_future, "unity.crime.police_future", brief.BriefId, "design_brief", "Police/crime Lua/data module (future).", UnityArchiveRequestProviderKind.procedural_future);
            diagnostics.Add(Warning("request.diagnostic.future_lua_module", "Planned future Lua module 'unity.crime.police_future' is metadata-only.", "unity.crime.police_future"));
        }

        if (HasLuaModule(moduleIds, "unity.combat.army_battle_future"))
        {
            AddLua(UnityArchiveLuaModuleKind.army_battle_future, "unity.combat.army_battle_future", brief.BriefId, "design_brief", "Army battle Lua/data module (future).", UnityArchiveRequestProviderKind.procedural_future);
            diagnostics.Add(Warning("request.diagnostic.future_lua_module", "Planned future Lua module 'unity.combat.army_battle_future' is metadata-only.", "unity.combat.army_battle_future"));
        }

        ValidateLuaIds();

        // Sort deterministically

        assetRequests.Sort((a, b) => CompareRequests(a.RequestId, b.RequestId));
        audioRequests.Sort((a, b) => CompareRequests(a.RequestId, b.RequestId));
        luaModuleRequests.Sort((a, b) => CompareRequests(a.ModuleId, b.ModuleId));

        var hasWarnings = diagnostics.Any(d => d.Severity == UnityArchiveExportDiagnosticSeverity.Warning);
        var hasErrors = diagnostics.Any(d => d.Severity == UnityArchiveExportDiagnosticSeverity.Error);
        var readiness = hasErrors
            ? UnityArchiveRequestReadiness.BlockedByFutureProviders
            : hasWarnings
                ? UnityArchiveRequestReadiness.ReadyWithWarnings
                : UnityArchiveRequestReadiness.Ready;

        return new UnityArchiveRequestPipelineResult
        {
            AssetRequests = assetRequests,
            AudioRequests = audioRequests,
            LuaModuleRequests = luaModuleRequests,
            Diagnostics = diagnostics.OrderBy(d => SeverityOrder(d.Severity))
                .ThenBy(d => d.Code, StringComparer.OrdinalIgnoreCase)
                .ThenBy(d => d.TargetId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(d => d.Message, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Readiness = readiness
        };
    }

    private static bool HasLuaModule(HashSet<string> moduleIds, string moduleId)
    {
        return moduleIds.Contains(moduleId);
    }

    private static bool IsFutureProvider(UnityArchiveRequestProviderKind kind)
    {
        return kind is UnityArchiveRequestProviderKind.comfyui_future
            or UnityArchiveRequestProviderKind.suno_future
            or UnityArchiveRequestProviderKind.local_audio_future
            or UnityArchiveRequestProviderKind.procedural_future;
    }

    private static int CompareRequests(string a, string b)
    {
        var kindComparison = string.Compare(GetRequestKind(a), GetRequestKind(b), StringComparison.OrdinalIgnoreCase);
        if (kindComparison != 0)
        {
            return kindComparison;
        }

        var idComparison = string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        if (idComparison != 0)
        {
            return idComparison;
        }

        return string.Compare(a, b, StringComparison.Ordinal);
    }

    private static string GetRequestKind(string requestId)
    {
        var segments = requestId.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 2 ? segments[0].ToLowerInvariant() : requestId.ToLowerInvariant();
    }

    private static string NormalizeId(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return "unknown";
        }

        var sb = new System.Text.StringBuilder(trimmed.Length);
        foreach (var ch in trimmed)
        {
            if (ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_')
            {
                sb.Append(ch);
            }
            else
            {
                sb.Append('-');
            }
        }

        return sb.ToString().ToLowerInvariant();
    }

    private static IReadOnlyList<string> Normalize(IEnumerable<UnityRuntimeModuleContract> modules)
    {
        return modules
            .Where(m => !string.IsNullOrWhiteSpace(m.ModuleId))
            .Select(m => m.ModuleId.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ThenBy(id => id, StringComparer.Ordinal)
            .ToList();
    }

    private static UnityArchiveRequestDiagnostic Error(string code, string message, string targetId)
    {
        return new UnityArchiveRequestDiagnostic
        {
            Severity = UnityArchiveExportDiagnosticSeverity.Error,
            Code = code,
            Message = message,
            TargetId = targetId
        };
    }

    private static UnityArchiveRequestDiagnostic Warning(string code, string message, string targetId)
    {
        return new UnityArchiveRequestDiagnostic
        {
            Severity = UnityArchiveExportDiagnosticSeverity.Warning,
            Code = code,
            Message = message,
            TargetId = targetId
        };
    }

    private static int SeverityOrder(UnityArchiveExportDiagnosticSeverity severity)
    {
        return severity switch
        {
            UnityArchiveExportDiagnosticSeverity.Error => 0,
            UnityArchiveExportDiagnosticSeverity.Warning => 1,
            _ => 2
        };
    }
}
