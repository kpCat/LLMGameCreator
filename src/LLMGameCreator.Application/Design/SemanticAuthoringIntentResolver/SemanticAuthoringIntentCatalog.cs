using LLMGameCreator.Application.Design.DynamicSemanticFeatures;
using LLMGameCreator.Application.Design.SemanticArtifactContracts;
using LLMGameCreator.Application.Design.SemanticPackComposition;

namespace LLMGameCreator.Application.Design.SemanticAuthoringIntentResolver;

public static class SemanticAuthoringIntentCatalog
{
    public static IReadOnlyList<SemanticAuthoringWorkspace> BuildDefaultWorkspaces()
    {
        var definitions = DynamicSemanticFeatureCatalog.BuildDefaultFeatureDefinitions();
        var resolver = new DynamicSemanticFeatureResolver();
        return DynamicSemanticFeatureCatalog.BuildDefaultScenarios()
            .Select(scenario => BuildWorkspace(scenario, resolver.ResolveScenario(scenario, definitions), definitions))
            .OrderBy(item => item.WorkspaceId, StringComparer.Ordinal)
            .ToList();
    }

    public static LoreIntakeSkeleton BuildMetamoduleKingdomsLoreSkeleton()
    {
        var kingdomIds = new[] { "auric", "umbra", "verdant", "cinder", "tideglass", "ironroot", "moonveil" };
        var kingdoms = kingdomIds
            .Select((id, index) => new LoreKingdomSlot
            {
                KingdomId = $"kingdom/{id}",
                RegionFamily = index % 2 == 0 ? "mana_court_region" : "border_pressure_region",
                PressureAxis = index % 3 == 0 ? "forbidden_affinity" : index % 3 == 1 ? "kingdom_pressure" : "faction_relation"
            })
            .ToList();
        var regions = kingdoms
            .SelectMany(kingdom => Enumerable.Range(1, 3).Select(ordinal => new LoreNamedSlot
            {
                SlotId = $"{kingdom.KingdomId}/region_{ordinal:00}",
                Family = ordinal == 1 ? "capital" : ordinal == 2 ? "frontier" : "resource"
            }))
            .OrderBy(item => item.SlotId, StringComparer.Ordinal)
            .ToList();
        var slots = kingdoms
            .SelectMany((kingdom, kingdomIndex) => Enumerable.Range(1, 16).Select(ordinal => new LoreSpeciesArchetypeSlot
            {
                SlotId = $"{kingdom.KingdomId}/species_archetype_{ordinal:00}",
                KingdomId = kingdom.KingdomId,
                SpeciesFamily = ordinal % 4 == 0 ? "metamodule_bearer" : ordinal % 4 == 1 ? "mana_resonant" : ordinal % 4 == 2 ? "forbidden_affinity_lineage" : "pressure_adapted",
                ArchetypeFamily = (kingdomIndex + ordinal) % 5 == 0 ? "module_scout" : (kingdomIndex + ordinal) % 5 == 1 ? "resonance_warden" : (kingdomIndex + ordinal) % 5 == 2 ? "forbidden_crafter" : (kingdomIndex + ordinal) % 5 == 3 ? "faction_envoy" : "border_survivor",
                Ordinal = kingdomIndex * 16 + ordinal
            }))
            .OrderBy(item => item.SlotId, StringComparer.Ordinal)
            .ToList();

        return new LoreIntakeSkeleton
        {
            LoreBriefId = "lore/metamodule_kingdoms/brief_v1",
            ScenarioId = "metamodule_kingdoms",
            StyleProfileId = "metamodule_kingdoms",
            WorldThemes = ["metamodule", "kingdom_pressure", "mana_resonance", "forbidden_affinity"],
            KingdomSlots = kingdoms,
            RegionSlots = regions,
            SpeciesArchetypeSlots = slots,
            MagicSystemAxes = ["module_carrier", "mana_resonance", "forbidden_affinity", "resonance_grid"],
            ConflictAxes = ["kingdom_pressure", "faction_relation", "combat_pressure", "event_intent"],
            ManualFillSlots =
            [
                Slot("manual/kingdom_names", "kingdom", "manual", "user", "required"),
                Slot("manual/founding_myths", "world", "manual", "user", "optional"),
                Slot("manual/forbidden_affinity_policy", "archetype", "manual", "user", "required")
            ],
            ProgrammaticallyInferableSlots =
            [
                Slot("programmatic/species_slot_ids", "species", "programmatic", "programmatic", "ready"),
                Slot("programmatic/kingdom_pressure_defaults", "kingdom", "programmatic", "programmatic", "ready"),
                Slot("programmatic/dialogue_intent_hints", "dialogue", "programmatic", "programmatic", "ready")
            ],
            LlmCandidateSlots =
            [
                Slot("llm_candidate/kingdom_epithets", "kingdom", "candidate", "llm_candidate", "review_required"),
                Slot("llm_candidate/species_names", "species", "candidate", "llm_candidate", "review_required"),
                Slot("llm_candidate/conflict_hooks", "quest", "candidate", "llm_candidate", "review_required")
            ],
            FeatureFamilies =
            [
                "module_carriers",
                "mana_resonance",
                "forbidden_affinities",
                "kingdom_pressure",
                "faction_relation",
                "dialogue_intent",
                "quest_motive",
                "event_intent",
                "economy_pressure",
                "combat_pressure"
            ],
            EvidenceSummary = new LoreSkeletonEvidenceSummary
            {
                KingdomCount = kingdoms.Count,
                RegionSlotCount = regions.Count,
                SpeciesArchetypeSlotCount = slots.Count,
                RepresentativeSpeciesArchetypeSlots = slots.Take(8).Select(item => item.SlotId).ToList(),
                LlmCandidatesQuarantined = true
            }
        };
    }

    public static ManualVsAutoAuthoringMatrix BuildManualVsAutoAuthoringMatrix()
    {
        var rows = new List<ManualVsAutoAuthoringMatrixRow>
        {
            Row("explicit_user_set", "user", "accepted", true),
            Row("programmatic_default", "programmatic", "accepted", true),
            Row("inherited_value", "inherited", "accepted_with_trace", true),
            Row("semantic_pack_derived", "semantic_pack", "accepted_with_trace", true),
            Row("optional_absent", "unset", "optional_absent", true),
            Row("required_missing", "unset", "blocked", false, "semantic_authoring.required_field.missing"),
            Row("llm_candidate_review", "llm_candidate", "review_required", false, "semantic_authoring.candidate.not_accepted"),
            Row("imported_candidate_review", "imported_candidate", "review_required", false, "semantic_authoring.candidate.not_accepted"),
            Row("blocked_invalid_value", "blocked", "blocked", false, "semantic_authoring.field.blocked")
        };

        return new ManualVsAutoAuthoringMatrix
        {
            Rows = rows.OrderBy(item => item.CaseId, StringComparer.Ordinal).ToList(),
            Passed = rows.All(item => item.AcceptedAutomatically == (item.ExpectedStatus is "accepted" or "accepted_with_trace" or "optional_absent"))
        };
    }

    public static UpstreamSemanticSeamSummary BuildUpstreamSeamSummary()
    {
        var contracts = SemanticArtifactContractRegistry.BuildDefaultContracts();
        var packs = SemanticPackCompositionCatalog.BuildDefaultPacks();
        var definitions = DynamicSemanticFeatureCatalog.BuildDefaultFeatureDefinitions();
        return new UpstreamSemanticSeamSummary
        {
            Goal030ContractCount = contracts.Count,
            Goal031PackCount = packs.Count,
            Goal032FeatureCount = definitions.Count,
            Goal032InfluenceRuleCount = DynamicSemanticFeatureCatalog.BuildDefaultInfluenceRules().Count,
            Goal030ReadyContractIds = contracts
                .Where(item => item.LifecycleStatus == "ready")
                .Select(item => item.ContractId)
                .Order(StringComparer.Ordinal)
                .ToList(),
            Goal031PackIds = packs.Select(item => item.PackId).Order(StringComparer.Ordinal).ToList(),
            Goal032FeatureIds = definitions.Select(item => item.FeatureId).Order(StringComparer.Ordinal).ToList()
        };
    }

    private static SemanticAuthoringWorkspace BuildWorkspace(
        DynamicSemanticScenario scenario,
        DynamicSemanticResolvedScenarioState state,
        IReadOnlyList<DynamicSemanticFeatureDefinition> definitions)
    {
        var targetById = scenario.Targets.ToDictionary(item => item.TargetId, StringComparer.Ordinal);
        var resolvedByTarget = state.TargetStates.ToDictionary(item => item.TargetId, StringComparer.Ordinal);
        var groups = new List<SemanticAuthoringDomainGroup>();

        foreach (var domain in SemanticAuthoringIntentVocabulary.DomainGroups.Order(StringComparer.Ordinal))
        {
            var sections = scenario.Targets
                .Where(item => ToDomain(item.TargetScope) == domain)
                .OrderBy(item => item.TargetId, StringComparer.Ordinal)
                .Select(target => BuildSection(domain, target, resolvedByTarget.GetValueOrDefault(target.TargetId), definitions))
                .ToList();

            if (sections.Count == 0)
            {
                sections.Add(new SemanticAuthoringSection
                {
                    SectionId = $"{scenario.ScenarioId}/{domain}/legal_absence",
                    SourceTargetId = string.Empty,
                    CompletionStatus = "optional_absent",
                    Fields = [LegalAbsenceField(scenario.ScenarioId, domain)]
                });
            }

            groups.Add(new SemanticAuthoringDomainGroup { DomainId = domain, Sections = sections });
        }

        var fieldCount = groups.SelectMany(item => item.Sections).Sum(item => item.Fields.Count);
        var diagnostics = SemanticAuthoringIntentValidator.ValidateWorkspace(new SemanticAuthoringWorkspace
        {
            WorkspaceId = $"workspace/{scenario.ScenarioId}",
            ScenarioId = scenario.ScenarioId,
            ProfileId = scenario.ProfileId,
            DomainGroups = groups
        });
        return new SemanticAuthoringWorkspace
        {
            WorkspaceId = $"workspace/{scenario.ScenarioId}",
            ScenarioId = scenario.ScenarioId,
            ProfileId = scenario.ProfileId,
            DomainGroups = groups,
            Diagnostics = diagnostics,
            StableSummary = $"{scenario.ScenarioId}|domains={groups.Count}|fields={fieldCount}|diagnostics={diagnostics.Count(item => item.Severity == "error")}"
        };
    }

    private static SemanticAuthoringSection BuildSection(
        string domain,
        DynamicSemanticTargetNode target,
        DynamicSemanticResolvedTargetState? resolved,
        IReadOnlyList<DynamicSemanticFeatureDefinition> definitions)
    {
        var fields = new List<SemanticAuthoringField>();
        if (resolved == null)
        {
            return new SemanticAuthoringSection
            {
                SectionId = $"{target.TargetId}/context",
                SourceTargetId = target.TargetId,
                Fields = [LegalAbsenceField(target.TargetId, domain)],
                CompletionStatus = "optional_absent"
            };
        }

        if (resolved != null)
        {
            foreach (var feature in resolved.Features.OrderBy(item => item.FeatureId, StringComparer.Ordinal))
            {
                var definition = definitions.FirstOrDefault(item => item.FeatureId == feature.FeatureId);
                if (definition == null)
                {
                    continue;
                }

                if (ToDomain(definition.TargetScope) != domain && !feature.Inherited)
                {
                    continue;
                }

                fields.Add(new SemanticAuthoringField
                {
                    FieldId = $"{target.TargetId}:{feature.FeatureId}",
                    DomainId = domain,
                    FeatureId = feature.FeatureId,
                    ValueKind = feature.ValueKind,
                    RequirementStatus = definition?.RequiredMode ?? "optional",
                    Repeatable = definition?.Cardinality == "many",
                    ApplicabilityHint = "applicable",
                    InheritanceHint = feature.Inherited ? $"inherited_from:{feature.SourceTargetId}" : "local_or_programmatic",
                    ControlHint = ControlHint(feature.ValueKind),
                    CompletionStatus = feature.Blocked ? "blocked" : feature.Value == null ? "optional_absent" : "complete",
                    Provenance = ToProvenance(feature),
                    ResolvedValueSummary = feature.Value?.StableValueKey() ?? string.Empty
                });
            }
        }

        foreach (var definition in definitions.Where(item => ToDomain(item.TargetScope) == domain).OrderBy(item => item.FeatureId, StringComparer.Ordinal))
        {
            if (fields.Any(item => item.FeatureId == definition.FeatureId))
            {
                continue;
            }

            fields.Add(new SemanticAuthoringField
            {
                FieldId = $"{target.TargetId}:{definition.FeatureId}:absent",
                DomainId = domain,
                FeatureId = definition.FeatureId,
                ValueKind = definition.ValueKind,
                RequirementStatus = definition.RequiredMode,
                ApplicabilityHint = "legal_absence_if_optional_or_inapplicable",
                InheritanceHint = definition.InheritanceMode,
                ControlHint = ControlHint(definition.ValueKind),
                CompletionStatus = definition.RequiredMode == "required" ? "missing_required" : "optional_absent",
                Provenance = definition.RequiredMode == "required" ? "blocked" : "unset"
            });
        }

        return new SemanticAuthoringSection
        {
            SectionId = $"{target.TargetId}/authoring",
            SourceTargetId = target.TargetId,
            Fields = fields.OrderBy(item => item.FieldId, StringComparer.Ordinal).ToList(),
            CompletionStatus = fields.Any(item => item.CompletionStatus == "missing_required")
                ? "missing_required"
                : fields.Any(item => item.CompletionStatus == "optional_absent")
                    ? "partial"
                    : "complete"
        };
    }

    private static SemanticAuthoringField LegalAbsenceField(string scenarioId, string domain) =>
        new()
        {
            FieldId = $"{scenarioId}:{domain}:legal_absence",
            DomainId = domain,
            FeatureId = $"{domain}.optional_absence",
            ValueKind = "none",
            RequirementStatus = "optional",
            ApplicabilityHint = "domain_group_legally_absent_for_this_scenario",
            InheritanceHint = "none",
            ControlHint = "none",
            CompletionStatus = "optional_absent",
            Provenance = "unset"
        };

    private static string ToDomain(string scope) =>
        scope switch
        {
            "resource" or "item" => "economy",
            "magic" or "relationship" => "world",
            "biome" => "region",
            _ => SemanticAuthoringIntentVocabulary.DomainGroups.Contains(scope, StringComparer.Ordinal) ? scope : "world"
        };

    private static string ControlHint(string valueKind) =>
        valueKind switch
        {
            "flag" => "checkbox",
            "number" => "numeric",
            "enum" => "select",
            "weighted_tag" => "weighted_tags",
            "relation" => "relation_picker",
            "text_key" => "localization_key",
            "list" => "multi_select",
            _ => "none"
        };

    private static string ToProvenance(DynamicSemanticResolvedFeature feature)
    {
        if (feature.Blocked)
        {
            return "blocked";
        }

        if (feature.Inherited)
        {
            return "inherited";
        }

        if (feature.SourceLayer == "manual_override" || feature.Manual)
        {
            return "user";
        }

        if (feature.SourceLayer is "world" or "kingdom" or "region" or "faction" or "species" or "archetype")
        {
            return "semantic_pack";
        }

        return feature.Generated ? "programmatic" : "unset";
    }

    private static LoreAuthoringSlot Slot(string id, string domain, string fillMode, string provenance, string status) =>
        new()
        {
            SlotId = id,
            DomainId = domain,
            FillMode = fillMode,
            Provenance = provenance,
            ReviewStatus = status
        };

    private static ManualVsAutoAuthoringMatrixRow Row(
        string id,
        string provenance,
        string status,
        bool accepted,
        string diagnostic = "") =>
        new()
        {
            CaseId = id,
            Provenance = provenance,
            ExpectedStatus = status,
            AcceptedAutomatically = accepted,
            DiagnosticCode = diagnostic
        };
}
