using LLMGameCreator.Application.Validation;
using LLMGameCreator.Domain.Definitions;
using LLMGameCreator.Domain.Validation;
using LLMGameCreator.GamePackage;

namespace LLMGameCreator.Application.Design.EditDrivenGamePackageRuntimePreviewBridge;

internal sealed class EditDrivenGamePackageRuntimePreviewBridgeProjectionBuilder
{
    private readonly GamePackageValidator _validator = new();

    public GamePackageDefinition BuildProjectedPackage(Goal080SourceContext source)
    {
        const string mapId = "map/goal080/runtime-preview-bridge";
        const string tileId = "tile/goal080/review-floor";
        const string playerPrototypeId = "entity-prototype/goal080/player";
        const string reviewerPrototypeId = "entity-prototype/goal080/review-node";
        var package = new GamePackageDefinition
        {
            Manifest =
            {
                PackageId = "game/goal080/edit-driven-runtime-preview-bridge",
                Title = "Goal080 Edit Driven Runtime Preview Bridge",
                Version = "0.80.0",
                FormatVersion = "0.1",
                StartMapId = mapId,
                Description = "Disk-backed GamePackage projection of Goal077 edit targets and Goal078 playable-session actions."
            }
        };

        package.Game.TilePrototypes.Add(new TilePrototypeDefinition
        {
            Id = tileId,
            Name = "Review floor",
            Walkable = true,
            MovementCost = 1
        });
        package.Game.EntityPrototypes.Add(new EntityPrototypeDefinition { Id = playerPrototypeId, Name = "Reviewer" });
        package.Game.EntityPrototypes.Add(new EntityPrototypeDefinition { Id = reviewerPrototypeId, Name = "Review node" });
        package.Game.Maps.Add(new MapDefinition
        {
            Id = mapId,
            Name = "Goal080 runtime preview bridge",
            Width = 8,
            Height = 8,
            DefaultTileId = tileId,
            StartPosition = new Position2D { X = 1, Y = 1 },
            Entities = BuildMapEntities(source.Rows, playerPrototypeId, reviewerPrototypeId)
        });
        package.Game.Resources.Add(new ResourceDefinition
        {
            Id = "resource/goal080/preview-confidence",
            Name = "Preview confidence",
            Kind = "score",
            DefaultValue = 0,
            MaxValue = 100,
            Metadata = { ["goalId"] = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId }
        });
        package.Game.Factions.Add(new FactionDefinition
        {
            Id = "faction/goal080/reviewers",
            Name = "Reviewers",
            Description = "Review-facing bridge actors."
        });

        foreach (var target in source.Targets)
        {
            package.Game.Items.Add(BuildItem(target));
            package.Game.Interactions.Add(BuildInteraction(target));
            package.Game.Abilities.Add(BuildAbility(target));
        }

        foreach (var row in source.Rows)
        {
            package.Game.Quests.Add(BuildQuest(row));
            package.Game.Dialogues.Add(BuildDialogue(row));
            package.Game.Encounters.Add(BuildEncounter(row));
        }

        package.GeneratedContent = BuildGeneratedContent(source, mapId);
        return package;
    }

    public IReadOnlyDictionary<string, string> BuildProjectedPackageFiles(
        Goal080SourceContext source,
        GamePackageDefinition package)
    {
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.PackageJsonRelativePath] = Serialize(package)
        };
        var packageHash = EditDrivenGamePackageRuntimePreviewBridgeHash.Sha256Text(
            files[EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.PackageJsonRelativePath]);

        var projectedIndex = new
        {
            schemaVersion = "edit_driven_gamepackage_runtime_preview_bridge_projected_index_v1",
            goalId = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId,
            passed = true,
            packageFile = EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.PackageJsonRelativePath,
            packageHash,
            sourceGoal077ReportHash = source.Goal077ReportHash,
            sourceGoal078ReportHash = source.Goal078ReportHash,
            sourceGoal079ReportHash = source.Goal079ReportHash,
            sourceGoal079AReportHash = source.Goal079AReportHash,
            rowCount = source.Rows.Count,
            targetCount = source.Targets.Count,
            rows = source.Rows.Select(row => new
            {
                row.RowId,
                row.FamilyId,
                row.SeedId,
                row.ProfileId,
                questId = QuestId(row),
                dialogueId = DialogueId(row),
                encounterId = EncounterId(row),
                targetIds = row.Targets.Select(target => target.TargetId).ToList()
            }).ToList()
        };
        var playerIndex = new
        {
            schemaVersion = "edit_driven_gamepackage_runtime_preview_bridge_player_index_v1",
            goalId = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId,
            passed = true,
            scenarioCount = source.Rows.Count,
            commandCount = source.ActionLog.ActionCount,
            scenarios = source.Rows.Select(row => new
            {
                scenarioId = row.ProfileId,
                row.RowId,
                row.FamilyId,
                row.SeedId,
                playerFacingQuest = QuestId(row),
                playerFacingDialogue = DialogueId(row),
                projectedTargets = row.Targets.Select(target => new
                {
                    target.TargetId,
                    target.LogicalPackagePath,
                    projectedItem = ItemId(target),
                    projectedInteraction = InteractionId(target)
                }).ToList()
            }).ToList()
        };
        var sourceTargets = new
        {
            schemaVersion = "edit_driven_gamepackage_runtime_preview_bridge_source_targets_v1",
            goalId = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId,
            targetCount = source.Targets.Count,
            targets = source.Targets.Select(target => new
            {
                target.RowId,
                target.FamilyId,
                target.SeedId,
                target.TargetId,
                target.RelativePath,
                target.LogicalPackagePath,
                target.PayloadHash,
                target.FileHash,
                target.AfterHash
            }).ToList()
        };
        var validation = _validator.Validate(package);
        var validationReport = new
        {
            schemaVersion = "edit_driven_gamepackage_runtime_preview_bridge_validation_report_v1",
            goalId = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId,
            passed = validation.IsValid,
            issueCount = validation.Issues.Count,
            issues = validation.Issues.Select(ToValidationIssueProjection).ToList()
        };

        files[EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.ProjectedIndexRelativePath] = Serialize(projectedIndex);
        files[EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.PlayerIndexRelativePath] = Serialize(playerIndex);
        files[EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.SourceTargetsRelativePath] = Serialize(sourceTargets);
        files[EditDrivenGamePackageRuntimePreviewBridgeProjectedPackagePaths.ValidationReportRelativePath] = Serialize(validationReport);
        return files;
    }

    private static List<EntityInstanceDefinition> BuildMapEntities(
        IReadOnlyList<EditDrivenGamePackageRuntimePreviewBridgeRowRecord> rows,
        string playerPrototypeId,
        string reviewerPrototypeId)
    {
        var entities = new List<EntityInstanceDefinition>
        {
            new()
            {
                Id = "entity/goal080/player",
                PrototypeId = playerPrototypeId,
                Position = new Position2D { X = 1, Y = 1 }
            }
        };

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            entities.Add(new EntityInstanceDefinition
            {
                Id = EntityId(row),
                PrototypeId = reviewerPrototypeId,
                Position = new Position2D { X = 2 + index % 3, Y = 2 + index / 3 }
            });
        }

        return entities;
    }

    private static ItemDefinition BuildItem(EditDrivenGamePackageRuntimePreviewBridgeTargetRecord target) =>
        new()
        {
            Id = ItemId(target),
            Name = $"{target.TargetId} bridge target",
            Description = target.AfterValue,
            Kind = "review_target",
            QuestItem = true,
            Unique = true,
            Tags = { target.FamilyId, target.SeedId, target.DomainId },
            Metadata =
            {
                ["goalId"] = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId,
                ["rowId"] = target.RowId,
                ["targetId"] = target.TargetId,
                ["logicalPackagePath"] = target.LogicalPackagePath,
                ["targetPayloadHash"] = target.PayloadHash
            }
        };

    private static InteractionDefinition BuildInteraction(EditDrivenGamePackageRuntimePreviewBridgeTargetRecord target) =>
        new()
        {
            Id = InteractionId(target),
            Kind = "inspect",
            Metadata =
            {
                ["goalId"] = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId,
                ["rowId"] = target.RowId,
                ["targetId"] = target.TargetId,
                ["afterHash"] = target.AfterHash,
                ["validationRequirement"] = target.ValidationRequirement
            }
        };

    private static AbilityDefinition BuildAbility(EditDrivenGamePackageRuntimePreviewBridgeTargetRecord target) =>
        new()
        {
            Id = AbilityId(target),
            Name = $"{target.TargetId} preview marker",
            Kind = "review_marker",
            Targeting = "self",
            Power = 1,
            Tags = { target.DomainId, target.FieldId },
            Metadata =
            {
                ["goalId"] = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId,
                ["targetId"] = target.TargetId,
                ["payloadHash"] = target.PayloadHash
            }
        };

    private static QuestDefinition BuildQuest(EditDrivenGamePackageRuntimePreviewBridgeRowRecord row) =>
        new()
        {
            Id = QuestId(row),
            Title = $"{row.ProfileId} preview bridge",
            Description = $"Review {row.Targets.Count} projected package targets for {row.ProfileId}.",
            Kind = "review_bridge",
            AutoStart = true,
            Tags = { row.FamilyId, row.SeedId },
            Metadata =
            {
                ["goalId"] = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId,
                ["rowId"] = row.RowId,
                ["profileId"] = row.ProfileId
            },
            Objectives = row.Targets.Select(target => new QuestObjectiveDefinition
            {
                Id = $"objective/{target.TargetId}",
                Kind = "custom_counter",
                RequiredAmount = 1,
                Metadata =
                {
                    ["targetId"] = target.TargetId,
                    ["logicalPackagePath"] = target.LogicalPackagePath
                }
            }).ToList(),
            Stages =
            {
                new QuestStageDefinition
                {
                    Id = "stage/review",
                    Text = $"Inspect projected targets for {row.ProfileId}.",
                    Objectives = row.Targets.Select(target => new QuestObjectiveDefinition
                    {
                        Id = $"stage-objective/{target.TargetId}",
                        Kind = "custom_counter",
                        RequiredAmount = 1,
                        Metadata = { ["targetId"] = target.TargetId }
                    }).ToList()
                }
            }
        };

    private static DialogueDefinition BuildDialogue(EditDrivenGamePackageRuntimePreviewBridgeRowRecord row) =>
        new()
        {
            Id = DialogueId(row),
            Title = $"{row.ProfileId} review briefing",
            StartNodeId = "node/start",
            Tags = { row.FamilyId, row.SeedId },
            Metadata =
            {
                ["goalId"] = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId,
                ["rowId"] = row.RowId
            },
            Nodes =
            {
                new DialogueNodeDefinition
                {
                    Id = "node/start",
                    SpeakerId = EntityId(row),
                    Text = $"Projected targets: {string.Join(", ", row.Targets.Select(target => target.TargetId))}.",
                    Choices =
                    {
                        new DialogueChoiceDefinition
                        {
                            Id = "choice/start-quest",
                            Text = "Open review quest",
                            StartQuestId = QuestId(row),
                            CloseDialogue = true
                        }
                    }
                }
            }
        };

    private static EncounterDefinition BuildEncounter(EditDrivenGamePackageRuntimePreviewBridgeRowRecord row) =>
        new()
        {
            Id = EncounterId(row),
            Name = $"{row.ProfileId} preview validation",
            Kind = "review_validation",
            Participants =
            {
                new EncounterParticipantDefinition
                {
                    Id = "participant/reviewer",
                    Name = "Reviewer",
                    Kind = "player",
                    EntityPrototypeId = "entity-prototype/goal080/player",
                    Team = "review"
                }
            },
            Metadata =
            {
                ["goalId"] = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId,
                ["rowId"] = row.RowId
            }
        };

    private static GeneratedContentDefinition BuildGeneratedContent(Goal080SourceContext source, string mapId)
    {
        var generated = new GeneratedContentDefinition
        {
            Profile =
            {
                Title = "Goal080 runtime preview bridge",
                Description = "Projection of Goal077 edit targets into a public GamePackage preview path.",
                Genre = "review",
                Tone = "deterministic",
                PresentationMode = "runtime-preview",
                WorldTopology = "single-map-review-grid",
                ActorModel = "reviewer-plus-row-nodes",
                CombatModel = "validation-only",
                CoreLoop =
                {
                    "read projected package from disk",
                    "validate package",
                    "project generated content into runtime preview",
                    "cover playable-session action targets"
                },
                Pillars =
                {
                    "disk backed",
                    "edit driven",
                    "runtime preview compatible"
                },
                SourceContextJson = Serialize(new
                {
                    sourceGoal077ReportHash = source.Goal077ReportHash,
                    sourceGoal078ReportHash = source.Goal078ReportHash,
                    rowCount = source.Rows.Count,
                    targetCount = source.Targets.Count,
                    actionCount = source.ActionLog.Actions.Count
                }).Trim()
            }
        };

        generated.Scenes.Add(new GeneratedSceneDefinition
        {
            SourceId = "scene/goal080/runtime-preview-bridge",
            PackageMapId = mapId,
            Title = "Runtime preview bridge",
            Description = "A deterministic scene containing every Goal077 row and target projected for preview.",
            Purpose = "goal080_proof"
        });

        foreach (var family in source.Rows.GroupBy(row => row.FamilyId).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            generated.Regions.Add(new GeneratedRegionDefinition
            {
                SourceId = $"region/goal080/{family.Key}",
                Title = family.Key,
                Description = $"Review region for {family.Key}.",
                SceneIds = { "scene/goal080/runtime-preview-bridge" }
            });
        }

        foreach (var row in source.Rows)
        {
            generated.Npcs.Add(new GeneratedNpcDefinition
            {
                SourceId = EntityId(row),
                Name = $"{row.ProfileId} review node",
                Description = $"Runtime-preview bridge node for {row.Targets.Count} targets.",
                RegionId = $"region/goal080/{row.FamilyId}",
                SceneId = "scene/goal080/runtime-preview-bridge"
            });
            generated.Dialogues.Add(new GeneratedDialogueDefinition
            {
                SourceId = DialogueId(row),
                Title = $"{row.ProfileId} review dialogue",
                Description = "Player-facing briefing generated from the projected GamePackage.",
                NpcId = EntityId(row),
                SceneId = "scene/goal080/runtime-preview-bridge",
                Lines = row.Targets.Select(target => $"{target.TargetId}: {target.AfterValue}").ToList()
            });
            generated.Encounters.Add(new GeneratedEncounterDefinition
            {
                SourceId = EncounterId(row),
                Title = $"{row.ProfileId} validation encounter",
                Description = "Validation-only encounter for projected target coverage.",
                RegionId = $"region/goal080/{row.FamilyId}",
                SceneId = "scene/goal080/runtime-preview-bridge",
                NpcIds = { EntityId(row) }
            });
            generated.Quests.Add(new GeneratedQuestSeedDefinition
            {
                SourceId = QuestId(row),
                PackageQuestId = QuestId(row),
                Title = $"{row.ProfileId} bridge quest",
                Description = $"Review all projected targets for {row.ProfileId}.",
                Steps = row.Targets.Select(target => $"Inspect {target.TargetId} at {target.LogicalPackagePath}.").ToList(),
                Objectives = row.Targets.Select(target => $"{target.TargetId} coverage").ToList()
            });
        }

        foreach (var target in source.Targets)
        {
            generated.Items.Add(new GeneratedItemDefinition
            {
                SourceId = ItemId(target),
                Name = $"{target.TargetId} package target",
                Description = target.AfterValue
            });
            generated.Mechanics.Add(new GeneratedMechanicDefinition
            {
                SourceId = InteractionId(target),
                PackageAbilityId = AbilityId(target),
                Name = $"{target.TargetId} bridge mechanic",
                Description = $"{target.FieldId} from {target.DomainId}.",
                Tags = { target.FamilyId, target.SeedId, target.TargetId }
            });
            generated.AppliedArtifacts.Add(new GeneratedContentArtifactProvenance
            {
                ArtifactId = target.TargetId,
                ContractId = target.LogicalPackagePath,
                ArtifactKind = "edit_target_projection",
                CapabilitySelectionId = target.ProfileId,
                GeneratedAt = "deterministic-goal080",
                AuditId = EditDrivenGamePackageRuntimePreviewBridgeVocabulary.GoalId,
                AppliedAt = "projected-gamepackage",
                ContentHash = target.PayloadHash,
                MappingResult = "projected_to_public_gamepackage_generated_content"
            });
        }

        return generated;
    }

    private static object ToValidationIssueProjection(ValidationIssue issue) =>
        new
        {
            issue.Code,
            severity = issue.Severity.ToString(),
            issue.Message,
            issue.TargetId,
            issue.Category
        };

    private static string EntityId(EditDrivenGamePackageRuntimePreviewBridgeRowRecord row) =>
        $"entity/goal080/{row.ProfileId}/review-node";

    private static string QuestId(EditDrivenGamePackageRuntimePreviewBridgeRowRecord row) =>
        $"quest/goal080/{row.ProfileId}/bridge";

    private static string DialogueId(EditDrivenGamePackageRuntimePreviewBridgeRowRecord row) =>
        $"dialogue/goal080/{row.ProfileId}/briefing";

    private static string EncounterId(EditDrivenGamePackageRuntimePreviewBridgeRowRecord row) =>
        $"encounter/goal080/{row.ProfileId}/validation";

    private static string ItemId(EditDrivenGamePackageRuntimePreviewBridgeTargetRecord target) =>
        $"item/goal080/{target.TargetId}";

    private static string InteractionId(EditDrivenGamePackageRuntimePreviewBridgeTargetRecord target) =>
        $"interaction/goal080/{target.TargetId}";

    private static string AbilityId(EditDrivenGamePackageRuntimePreviewBridgeTargetRecord target) =>
        $"ability/goal080/{target.TargetId}";

    private static string Serialize<T>(T value) =>
        EditDrivenGamePackageRuntimePreviewBridgeJson.Serialize(value);
}
