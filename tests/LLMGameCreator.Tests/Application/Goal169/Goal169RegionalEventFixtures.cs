using System.Reflection;
using System.Text.Json;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Generation.Procedural;
using LLMGameCreator.Application.Play.GeneratedCampaign;
using LLMGameCreator.Tests.Application.Goal164;
using LLMGameCreator.Tests.Application.Goal168;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.Application.Goal169;

internal sealed record Goal169RegionalEventFixture(
    LLMGameCreator.GamePackage.GamePackageDefinition Source,
    GeneratedCampaignRegionalEventBindingResult Binding,
    GeneratedCampaignRegionalEventBindingResult Rebound,
    GeneratedCampaignRegionalEventOverlayResult Overlay,
    GeneratedCampaignRegionalEventOverlayResult Rebuilt,
    GeneratedCampaignRegionalEventOverlayResult Reordered,
    GeneratedCampaignRegionalEventOverlayValidationResult ForbiddenDelta,
    string SourceCanonical,
    string ExistingCanonical,
    bool RelationshipTravelRecordsPreserved,
    bool EventReferencesResolve)
{
    private static readonly Lazy<Goal169RegionalEventFixture> Fixture =
        new(Create);

    internal static Goal169RegionalEventFixture Value => Fixture.Value;
    internal static GeneratedCampaignRegionalEventBindingResult
        InsufficientPlacement => InsufficientPlacementLazy.Value;
    internal static IReadOnlySet<
        GeneratedCampaignRegionalEventStatus> ProjectionStatuses =>
        Enum.GetValues<GeneratedCampaignRegionalEventStatus>()
            .ToHashSet();
    internal static bool HumanMarkerPassed =>
        Snapshot.Value.Map?.Cells.Any(item =>
            item.PrimarySymbol == "★"
            && !string.IsNullOrWhiteSpace(item.PrimaryTitle)) == true;
    internal static bool OtherRegionHuman =>
        Snapshot.Value.RegionalEvents.Any(item =>
            !item.OnCurrentMap
            && !string.IsNullOrWhiteSpace(item.MapTitle)
            && !string.IsNullOrWhiteSpace(item.RegionTitle));
    internal static bool NoRawIds => Snapshot.Value.RegionalEvents.All(row =>
        Value.Binding.Bindings.All(binding =>
            !RowText(row).Contains(binding.RegionalEventId,
                StringComparison.Ordinal)
            && !RowText(row).Contains(binding.RelationshipId,
                StringComparison.Ordinal)));
    internal static bool EventsTabPresent => UiCapture.Value.EventsTabPresent;
    internal static bool LayoutFits => UiCapture.Value.LayoutFits;

    private static readonly Lazy<
        GeneratedCampaignRegionalEventBindingResult>
        InsufficientPlacementLazy = new(CreateInsufficientPlacement);
    private static readonly Lazy<GeneratedCampaignSnapshot> Snapshot =
        new(CreateSnapshot);
    private static readonly Lazy<(bool EventsTabPresent, bool LayoutFits)>
        UiCapture = new(CreateUiCapture);

    private static Goal169RegionalEventFixture Create()
    {
        var source = Goal164TestKit.Clone(
            Goal168RelationshipFixture.Overlay
                .RelationshipOverlayPackage);
        var relationshipOverlay =
            Goal168RelationshipFixture.Overlay.Document;
        var binding = new GeneratedCampaignRegionalEventBindingService()
            .Bind(source, relationshipOverlay);
        Assert.True(binding.Passed,
            string.Join(Environment.NewLine, binding.Diagnostics));
        var rebound = new GeneratedCampaignRegionalEventBindingService()
            .Bind(Goal164TestKit.Clone(source), relationshipOverlay);
        var overlay = new GeneratedCampaignRegionalEventOverlayService()
            .Build(source, binding);
        Assert.True(overlay.Passed,
            string.Join(Environment.NewLine, overlay.Diagnostics));
        var rebuilt = new GeneratedCampaignRegionalEventOverlayService()
            .Build(Goal164TestKit.Clone(source), rebound);
        var reorderedBinding = binding with
        {
            Bindings = binding.Bindings.Reverse().ToList()
        };
        var reordered = new GeneratedCampaignRegionalEventOverlayService()
            .Build(Goal164TestKit.Clone(source), reorderedBinding);

        var tampered = Goal164TestKit.Clone(
            overlay.RegionalEventOverlayPackage);
        tampered.Game.Items[0].Name += " changed";
        var forbidden =
            new GeneratedCampaignRegionalEventOverlayService()
                .ValidateOverlayPackage(source, tampered,
                    overlay.Document);
        var validated =
            new GeneratedCampaignRegionalEventOverlayService()
                .ValidateOverlayPackage(source,
                    overlay.RegionalEventOverlayPackage,
                    overlay.Document);
        var stripped = Goal164TestKit.Clone(
            overlay.RegionalEventOverlayPackage);
        Strip(stripped, binding);
        var sourceCanonical = Goal164TestKit.Canonical(source);
        var existingCanonical = Goal164TestKit.Canonical(stripped);
        var references = binding.Bindings.All(item =>
            stripped.Game.EntityPrototypes.All(value =>
                value.Id != item.EntityPrototypeId)
            && overlay.RegionalEventOverlayPackage.Game.Dialogues.Count(
                value => value.Id == item.DialogueId) == 1
            && overlay.RegionalEventOverlayPackage.Game.Interactions.Count(
                value => value.Id == item.InteractionId) == 1
            && overlay.RegionalEventOverlayPackage.Game.Maps
                .Single(value => value.Id == item.MapId).Entities.Count(
                    value => value.Id == item.MapEntityId) == 1);
        return new Goal169RegionalEventFixture(
            source, binding, rebound, overlay, rebuilt, reordered,
            forbidden, sourceCanonical, existingCanonical,
            validated.Passed && overlay.Document.ControlledDeltaPassed,
            references);
    }

    private static void Strip(
        LLMGameCreator.GamePackage.GamePackageDefinition package,
        GeneratedCampaignRegionalEventBindingResult binding)
    {
        var prototypeIds = binding.Bindings
            .Select(item => item.EntityPrototypeId).ToHashSet();
        var dialogueIds = binding.Bindings
            .Select(item => item.DialogueId).ToHashSet();
        var interactionIds = binding.Bindings
            .Select(item => item.InteractionId).ToHashSet();
        var entityIds = binding.Bindings
            .Select(item => item.MapEntityId).ToHashSet();
        package.Game.EntityPrototypes.RemoveAll(item =>
            prototypeIds.Contains(item.Id));
        package.Game.Dialogues.RemoveAll(item =>
            dialogueIds.Contains(item.Id));
        package.Game.Interactions.RemoveAll(item =>
            interactionIds.Contains(item.Id));
        foreach (var map in package.Game.Maps)
            map.Entities.RemoveAll(item => entityIds.Contains(item.Id));
    }

    private static GeneratedCampaignRegionalEventBindingResult
        CreateInsufficientPlacement()
    {
        var package = Goal164TestKit.Clone(Value.Source);
        foreach (var map in package.Game.Maps)
        {
            map.Width = 1;
            map.Height = 1;
            map.StartPosition.X = 0;
            map.StartPosition.Y = 0;
        }
        return new GeneratedCampaignRegionalEventBindingService()
            .Bind(package, Goal168RelationshipFixture.Overlay.Document);
    }

    private static GeneratedCampaignSnapshot CreateSnapshot()
    {
        var build = Goal168TestKit.Real;
        var service = new GeneratedCampaignSessionService(build.Current,
            new GeneratedCampaignSessionTruthService(
                build.Current, build.Saves.Validator,
                build.Saves.Coordinator),
            build.Runtime, build.Saves.Save, build.Saves.Migration,
            new GeneratedCampaignActionPlanner(),
            new GeneratedCampaignProjectionService(),
            new GeneratedCampaignEventPresenter());
        var snapshot = service.StartNew();
        Assert.True(snapshot.Status ==
                    GeneratedCampaignSessionStatus.ACTIVE,
            string.Join(Environment.NewLine, snapshot.Diagnostics));
        Assert.NotEmpty(snapshot.RegionalEvents);
        return snapshot;
    }

    private static (bool EventsTabPresent, bool LayoutFits)
        CreateUiCapture()
    {
        var result = (false, false);
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var page = new GeneratedCampaignPageControl
                {
                    Size = new System.Drawing.Size(1100, 720)
                };
                typeof(GeneratedCampaignPageControl).GetMethod("Bind",
                        BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(page, [Snapshot.Value]);
                var text = (string)typeof(GeneratedCampaignPageControl)
                    .GetProperty("RegionalEventText",
                        BindingFlags.Instance
                        | BindingFlags.NonPublic)!
                    .GetValue(page)!;
                result = (!string.IsNullOrWhiteSpace(text),
                    page.Width >= 1100 && page.Height >= 720);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
            throw failure;
        return result;
    }

    private static string RowText(
        GeneratedCampaignRegionalEventRow row) =>
        string.Join("|", row.Title, row.KindTitle, row.RegionTitle,
            row.MapTitle, row.StatusTitle, row.NextAction);
}

internal static class Goal169HistoryCompatibilityFixture
{
    private static readonly Lazy<(bool Compatible, bool Rejected)> Result =
        new(Evaluate);

    internal static bool LegacyAllBranchCompatible =>
        Result.Value.Compatible;
    internal static bool LegacyPartialRejected => Result.Value.Rejected;

    private static (bool Compatible, bool Rejected) Evaluate()
    {
        var build = Goal168TestKit.Build;
        var relationships = Assert.IsType<
            GameProjectGeneratedCampaignRelationshipSummary>(
            build.GeneratedCampaignRelationships);
        var legacyRelationships = relationships with
        {
            SaveContinuationFactsPassed = true,
            SaveContinuationFactsEvaluationStatus = "EVALUATED",
            FinalStateHash = relationships.FinalStateHash
        };
        var entry = new GameProjectBuildHistoryEntry
        {
            SchemaVersion = GameProjectBuildHistoryReader.SchemaVersionV6,
            PackageSha256 = build.PackageSha256,
            FinalStateHash = relationships.FinalStateHash,
            GeneratedEncounterCombat = build.GeneratedEncounterCombat,
            GeneratedCampaignRelationships = legacyRelationships
        };
        var method = typeof(GameProjectBuildHistoryReader).GetMethod(
            "RelationshipEligible",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        var compatible = (bool)method.Invoke(null, [entry])!;
        var partial = entry with
        {
            GeneratedCampaignRelationships = legacyRelationships with
            {
                SaveContinuationFactsPassed = false
            }
        };
        var rejected = !(bool)method.Invoke(null, [partial])!;
        return (compatible, rejected);
    }
}
