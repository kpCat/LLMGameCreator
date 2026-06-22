using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Composition;

internal sealed class UnityArchiveAudioRequestBuilder
{
    private readonly UnityArchiveRequestBuildContext _context;
    private readonly List<UnityArchiveAudioRequest> _audioRequests;

    public UnityArchiveAudioRequestBuilder(UnityArchiveRequestBuildContext context)
    {
        _context = context;
        _audioRequests = new List<UnityArchiveAudioRequest>();
    }

    public List<UnityArchiveAudioRequest> Build()
    {
        BuildUiSfx();
        BuildFootstepSounds();
        BuildAbilitySounds();
        BuildSceneAmbience();
        BuildMusicThemes();

        return _audioRequests;
    }

    private void BuildUiSfx()
    {
        if (!_context.HasDynamicUi) return;

        AddAudio(UnityArchiveAudioKind.ui_sfx, "sfx.ui.confirm", "ui.confirm", "ui_layout", "Short UI confirm sound.", false, UnityArchiveRequestProviderKind.local_audio_future);
        AddAudio(UnityArchiveAudioKind.ui_sfx, "sfx.ui.cancel", "ui.cancel", "ui_layout", "Short UI cancel sound.", false, UnityArchiveRequestProviderKind.local_audio_future);
        AddAudio(UnityArchiveAudioKind.ui_sfx, "sfx.ui.click", "ui.click", "ui_layout", "Short UI click/select sound.", false, UnityArchiveRequestProviderKind.local_audio_future);
    }

    private void BuildFootstepSounds()
    {
        if (!_context.HasTilePrototypes) return;

        foreach (var tile in _context.Package!.Game.TilePrototypes)
        {
            AddAudio(
                UnityArchiveAudioKind.footstep,
                $"sfx.footstep.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(tile.Id)}",
                tile.Id,
                "package_tile_prototype",
                $"Footstep sound for tile prototype '{tile.Name}' (id={tile.Id}).",
                true,
                UnityArchiveRequestProviderKind.local_audio_future);
        }
    }

    private void BuildAbilitySounds()
    {
        if (_context.HasAbilities)
        {
            foreach (var ability in _context.Package!.Game.Abilities)
            {
                AddAudio(
                    UnityArchiveAudioKind.ability,
                    $"sfx.ability.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(ability.Id)}",
                    ability.Id,
                    "package_ability",
                    $"Ability sound effect for '{ability.Name}' (id={ability.Id}).",
                    false,
                    UnityArchiveRequestProviderKind.local_audio_future);
            }
        }
        else if (_context.HasMechanics)
        {
            foreach (var mechanic in _context.Package!.GeneratedContent.Mechanics)
            {
                AddAudio(
                    UnityArchiveAudioKind.ability,
                    $"sfx.ability.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(mechanic.SourceId)}",
                    mechanic.SourceId,
                    "generated_mechanic",
                    $"Ability sound effect for mechanic '{mechanic.Name}' generated from source '{mechanic.SourceId}'.",
                    false,
                    UnityArchiveRequestProviderKind.local_audio_future);
            }
        }
    }

    private void BuildSceneAmbience()
    {
        if (!_context.HasScenes) return;

        if (_context.Package!.GeneratedContent.Scenes.Count > 0)
        {
            foreach (var scene in _context.Package!.GeneratedContent.Scenes)
            {
                AddAudio(
                    UnityArchiveAudioKind.scene_ambience,
                    $"ambience.scene.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(scene.SourceId)}",
                    scene.SourceId,
                    "generated_scene",
                    $"Scene ambience for '{scene.Title}' generated from source '{scene.SourceId}'.",
                    true,
                    UnityArchiveRequestProviderKind.local_audio_future);
            }
        }

        if (_context.Package!.Game.Maps.Count > 0)
        {
            foreach (var map in _context.Package!.Game.Maps)
            {
                AddAudio(
                    UnityArchiveAudioKind.scene_ambience,
                    $"ambience.map.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(map.Id)}",
                    map.Id,
                    "package_map",
                    $"Map ambience for '{map.Name}' from package map '{map.Id}'.",
                    true,
                    UnityArchiveRequestProviderKind.local_audio_future);
            }
        }
    }

    private void BuildMusicThemes()
    {
        foreach (var wish in _context.DesignBrief.AudioStyleWishes)
        {
            AddAudio(
                UnityArchiveAudioKind.music,
                $"music.theme.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(wish.WishId)}",
                wish.WishId,
                "design_brief_audio_wish",
                $"Music theme slot for audio style wish '{wish.Description}' (id={wish.WishId}).",
                true,
                UnityArchiveRequestProviderKind.suno_future);
        }
    }

    private void AddAudio(UnityArchiveAudioKind kind, string audioId, string sourceId, string sourceKind, string prompt, bool loop, UnityArchiveRequestProviderKind provider)
    {
        var id = $"audio-request.{kind.ToString().ToLowerInvariant()}.{UnityArchiveRequestDiagnosticsBuilder.NormalizeId(sourceId)}";
        if (string.IsNullOrWhiteSpace(id))
        {
            // Note: blank audio ids would be handled by the caller's diagnostics list
            return;
        }

        _audioRequests.Add(new UnityArchiveAudioRequest
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
                ["design_brief_id"] = _context.DesignBrief.BriefId,
                ["target_profile_id"] = _context.TargetProfile.TargetProfileId
            }
        });
    }
}