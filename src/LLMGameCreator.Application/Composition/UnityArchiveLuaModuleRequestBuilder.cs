using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Composition;

internal sealed class UnityArchiveLuaModuleRequestBuilder
{
    private readonly UnityArchiveRequestBuildContext _context;
    private readonly List<UnityArchiveLuaModuleRequest> _luaModuleRequests;
    private readonly List<UnityArchiveRequestDiagnostic> _diagnostics;

    public UnityArchiveLuaModuleRequestBuilder(UnityArchiveRequestBuildContext context)
    {
        _context = context;
        _luaModuleRequests = new List<UnityArchiveLuaModuleRequest>();
        _diagnostics = new List<UnityArchiveRequestDiagnostic>();
    }

    public (List<UnityArchiveLuaModuleRequest> Requests, List<UnityArchiveRequestDiagnostic> Diagnostics) Build()
    {
        BuildInventoryModule();
        BuildQuestJournalModule();
        BuildDialogueModule();
        BuildCombatModule();
        BuildCraftingModule();
        BuildStatsModule();
        BuildWorldMapModule();
        BuildFactionsModule();
        BuildFutureTransportModule();
        BuildFuturePoliceModule();
        BuildFutureArmyBattleModule();

        return (_luaModuleRequests, _diagnostics);
    }

    private void BuildInventoryModule()
    {
        if (_context.HasLuaModule("unity.gameplay.inventory") || _context.HasItems)
        {
            AddLua(UnityArchiveLuaModuleKind.inventory, "unity.gameplay.inventory", "design_brief", "Inventory system Lua/data module.", UnityArchiveRequestProviderKind.none);
        }
    }

    private void BuildQuestJournalModule()
    {
        if (_context.HasLuaModule("unity.gameplay.quest_journal") || _context.HasQuests)
        {
            AddLua(UnityArchiveLuaModuleKind.quest_journal, "unity.gameplay.quest_journal", "design_brief", "Quest journal Lua/data module.", UnityArchiveRequestProviderKind.none);
        }
    }

    private void BuildDialogueModule()
    {
        if (_context.HasLuaModule("unity.gameplay.dialogue") || _context.HasDialogues)
        {
            AddLua(UnityArchiveLuaModuleKind.dialogue, "unity.gameplay.dialogue", "design_brief", "Dialogue Lua/data module.", UnityArchiveRequestProviderKind.none);
        }
    }

    private void BuildCombatModule()
    {
        bool hasCombatModule = _context.RuntimeModuleIds.Contains("unity.gameplay.personal_combat");
        if (_context.HasLuaModule("unity.gameplay.personal_combat") || hasCombatModule)
        {
            AddLua(UnityArchiveLuaModuleKind.combat, "unity.gameplay.personal_combat", "design_brief", "Personal combat Lua/data module.", UnityArchiveRequestProviderKind.none);
        }
    }

    private void BuildCraftingModule()
    {
        if (_context.HasLuaModule("unity.gameplay.crafting") || _context.HasCrafting)
        {
            AddLua(UnityArchiveLuaModuleKind.crafting, "unity.gameplay.crafting", "design_brief", "Crafting Lua/data module.", UnityArchiveRequestProviderKind.none);
        }
    }

    private void BuildStatsModule()
    {
        if (_context.HasItems || _context.HasAbilities || _context.HasMechanics)
        {
            AddLua(UnityArchiveLuaModuleKind.stats, "unity.gameplay.stats", "design_brief", "Stats Lua/data module.", UnityArchiveRequestProviderKind.none);
        }
    }

    private void BuildWorldMapModule()
    {
        if (_context.HasScenes)
        {
            AddLua(UnityArchiveLuaModuleKind.world_map, "unity.world.topdown_map", "design_brief", "World map Lua/data module.", UnityArchiveRequestProviderKind.none);
        }
    }

    private void BuildFactionsModule()
    {
        if (_context.HasFactions)
        {
            AddLua(UnityArchiveLuaModuleKind.factions, "unity.gameplay.factions", "design_brief", "Factions Lua/data module.", UnityArchiveRequestProviderKind.none);
        }
    }

    private void BuildFutureTransportModule()
    {
        if (_context.HasLuaModule("unity.transport.vehicle_future"))
        {
            AddLua(UnityArchiveLuaModuleKind.transport_future, "unity.transport.vehicle_future", "design_brief", "Vehicle transport Lua/data module (future).", UnityArchiveRequestProviderKind.procedural_future);
            _diagnostics.Add(UnityArchiveRequestDiagnosticsBuilder.Warning(
                "request.diagnostic.future_lua_module",
                "Planned future Lua module 'unity.transport.vehicle_future' is metadata-only.",
                "unity.transport.vehicle_future"));
        }
    }

    private void BuildFuturePoliceModule()
    {
        if (_context.HasLuaModule("unity.crime.police_future"))
        {
            AddLua(UnityArchiveLuaModuleKind.police_future, "unity.crime.police_future", "design_brief", "Police/crime Lua/data module (future).", UnityArchiveRequestProviderKind.procedural_future);
            _diagnostics.Add(UnityArchiveRequestDiagnosticsBuilder.Warning(
                "request.diagnostic.future_lua_module",
                "Planned future Lua module 'unity.crime.police_future' is metadata-only.",
                "unity.crime.police_future"));
        }
    }

    private void BuildFutureArmyBattleModule()
    {
        if (_context.HasLuaModule("unity.combat.army_battle_future"))
        {
            AddLua(UnityArchiveLuaModuleKind.army_battle_future, "unity.combat.army_battle_future", "design_brief", "Army battle Lua/data module (future).", UnityArchiveRequestProviderKind.procedural_future);
            _diagnostics.Add(UnityArchiveRequestDiagnosticsBuilder.Warning(
                "request.diagnostic.future_lua_module",
                "Planned future Lua module 'unity.combat.army_battle_future' is metadata-only.",
                "unity.combat.army_battle_future"));
        }
    }

    private void AddLua(UnityArchiveLuaModuleKind kind, string moduleId, string sourceKind, string prompt, UnityArchiveRequestProviderKind provider)
    {
        _luaModuleRequests.Add(new UnityArchiveLuaModuleRequest
        {
            ModuleId = $"lua-request.{kind.ToString().ToLowerInvariant()}",
            ModuleKind = kind,
            ProviderKind = provider,
            SourceRef = new UnityArchiveRequestSourceRef { SourceId = _context.DesignBrief.BriefId, SourceKind = sourceKind },
            PromptOrInstruction = prompt,
            Metadata = new Dictionary<string, string>
            {
                ["design_brief_id"] = _context.DesignBrief.BriefId,
                ["target_profile_id"] = _context.TargetProfile.TargetProfileId,
                ["required_runtime_module_id"] = moduleId
            }
        });
    }
}