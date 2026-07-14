using System.Globalization;
using System.Text.RegularExpressions;
using LLMGameCreator.Application.Design.ProjectStandaloneBuild;

namespace LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;

public sealed class GameProjectAcceptedMechanicsSummaryService
{
    public GameProjectAcceptedMechanicsSummary Project(GameProjectBuildResult build)
    {
        ArgumentNullException.ThrowIfNull(build);

        var equipmentPresent = !string.IsNullOrWhiteSpace(build.EquipmentSlotSummary);
        var attributesPresent = !string.IsNullOrWhiteSpace(build.AttributesSummary);
        var progressionPresent = !string.IsNullOrWhiteSpace(build.ProgressionSummary);
        var abilityPresent = !string.IsNullOrWhiteSpace(build.AbilitySummary);
        var manaPresent = !string.IsNullOrWhiteSpace(build.ManaSummary);
        var statusPresent = !string.IsNullOrWhiteSpace(build.StatusSummary);
        var socialPresent = build.Social is { Present: true, Passed: true };
        var present = equipmentPresent || attributesPresent || progressionPresent || abilityPresent
                      || manaPresent || statusPresent || build.Social is { Present: true };

        var missing = new List<string>();
        AddMissing(equipmentPresent, "equipment", missing);
        AddMissing(attributesPresent, "attributes", missing);
        AddMissing(progressionPresent, "progression", missing);
        AddMissing(abilityPresent, "ability", missing);
        AddMissing(manaPresent, "mana", missing);
        AddMissing(statusPresent, "turn_status", missing);
        AddMissing(socialPresent, "social", missing);
        AddMissing(build.CheckpointReloadPassed, "checkpoint_reload", missing);
        AddMissing(build.FullReplayEquivalent, "full_replay", missing);
        AddMissing(build.ActionBindingPassed, "action_binding", missing);
        AddMissing(!string.IsNullOrWhiteSpace(build.QualifiedAuthoringFingerprint),
            "qualified_authoring_fingerprint", missing);

        var facts = new List<GameProjectSocialHumanFact>
        {
            Fact("Механики", build.SelectedMechanicCount.ToString(CultureInfo.InvariantCulture)),
            Fact("Настроенные параметры", build.ConfiguredParameterCount.ToString(CultureInfo.InvariantCulture))
        };
        if (equipmentPresent) facts.Add(Fact("Бонус оружия", Signed(build.WeaponDamageBonus)));
        if (attributesPresent) facts.Add(Fact("Бонус характеристик", Signed(build.StatDamageBonus)));
        if (equipmentPresent || attributesPresent)
            facts.Add(Fact("Общий дополнительный урон", Signed(build.TotalAdditionalDamage)));
        if (progressionPresent) facts.Add(Fact("Прогрессия", "пройдена"));
        if (abilityPresent)
            facts.Add(Fact("Прямой урон способности", Number(build.AbilityDirectDamage)));
        if (manaPresent)
            facts.Add(Fact("Мана", Number(build.ManaBefore) + " → " + Number(build.ManaRemaining)));
        if (statusPresent)
            facts.Add(Fact("Эффект по ходам", "урон " + Number(build.StatusTickDamage) + "; "
                                              + (build.StatusExpired ? "завершён" : "не завершён")));
        if (socialPresent)
        {
            var social = build.Social!;
            facts.Add(Fact("Репутация", Number(social.ReputationBefore) + " → " + Number(social.ReputationAfter)));
            facts.Add(Fact("Золото", Number(social.GoldBefore) + " → " + Number(social.GoldAfterQuest)
                                      + " → " + Number(social.GoldAfterClaim)));
            foreach (var socialFact in social.HumanFacts)
            {
                if (facts.Any(item => string.Equals(item.Label, socialFact.Label, StringComparison.Ordinal))) continue;
                facts.Add(socialFact);
            }
        }
        facts.Add(Fact("Сохранение и повтор",
            build.CheckpointReloadPassed && build.FullReplayEquivalent && build.ActionBindingPassed
                ? "пройдено" : "не пройдено"));

        var diagnostics = missing.Select(kind => "accepted_mechanics.missing:" + kind).ToList();
        if (!build.Passed) diagnostics.Insert(0, "accepted_mechanics.build_not_green");
        return new GameProjectAcceptedMechanicsSummary
        {
            Present = present,
            Passed = build.Passed && missing.Count == 0,
            SelectedMechanicCount = build.SelectedMechanicCount,
            ConfiguredParameterCount = build.ConfiguredParameterCount,
            QualifiedAuthoringFingerprint = build.QualifiedAuthoringFingerprint,
            EquipmentDamageBonus = build.WeaponDamageBonus,
            StatDamageBonus = build.StatDamageBonus,
            TotalAdditionalDamage = build.TotalAdditionalDamage,
            AbilityDirectDamage = build.AbilityDirectDamage,
            ManaBefore = build.ManaBefore,
            ManaSpent = build.ManaSpent,
            ManaRemaining = build.ManaRemaining,
            StatusTickDamage = build.StatusTickDamage,
            StatusExpired = build.StatusExpired,
            Social = build.Social,
            CheckpointReloadPassed = build.CheckpointReloadPassed,
            FullReplayEquivalent = build.FullReplayEquivalent,
            ActionBindingPassed = build.ActionBindingPassed,
            HumanFacts = facts,
            MissingFactKinds = missing,
            Diagnostics = diagnostics
        };
    }

    public IReadOnlyList<StandaloneHumanReviewFact> StandaloneHumanFacts(
        GameProjectBuildResult build,
        bool includeReleaseCandidateReady)
    {
        ArgumentNullException.ThrowIfNull(build);
        var summary = build.AcceptedMechanics ?? Project(build);
        var facts = summary.HumanFacts.Select(ToStandaloneFact).ToList();

        AddDistinct(facts, "Бонус от характеристик", Signed(build.StatDamageBonus));
        AddSummaryFact(build.AttributesSummary, "stat/strength=", "Сила", facts);
        var progression = Regex.Match(build.ProgressionSummary ?? string.Empty, @"=(\d+):[^/]+/(\d+)");
        if (progression.Success)
        {
            AddDistinct(facts, "Уровень", progression.Groups[2].Value);
            AddDistinct(facts, "Опыт", progression.Groups[1].Value);
        }
        if (!string.IsNullOrWhiteSpace(build.AbilitySummary))
        {
            AddDistinct(facts, "Способность", build.AbilitySummary);
            AddDistinct(facts, "Прямой урон", Number(build.AbilityDirectDamage));
        }
        if (!string.IsNullOrWhiteSpace(build.ManaSummary))
        {
            AddDistinct(facts, "Начальная мана", Number(build.ManaBefore));
            AddDistinct(facts, "Потрачено маны", Number(build.ManaSpent));
            AddDistinct(facts, "Осталось маны", Number(build.ManaRemaining));
        }
        if (!string.IsNullOrWhiteSpace(build.StatusSummary))
        {
            AddDistinct(facts, "Эффект", build.StatusSummary.Split(',')[0]);
            AddDistinct(facts, "Длительность", build.StatusSummary.Contains(',')
                ? build.StatusSummary[(build.StatusSummary.IndexOf(',') + 1)..].Trim()
                : build.StatusSummary);
            AddDistinct(facts, "Урон за ход", Number(build.StatusTickDamage));
            AddDistinct(facts, "Эффект завершён", build.StatusExpired ? "да" : "нет");
        }
        if (includeReleaseCandidateReady && summary.Passed)
            AddDistinct(facts, "Release Candidate", "готов");
        return facts;
    }

    private static StandaloneHumanReviewFact ToStandaloneFact(GameProjectSocialHumanFact fact) =>
        new() { Label = fact.Label, Value = fact.Value };

    private static GameProjectSocialHumanFact Fact(string label, string value) =>
        new() { Label = label, Value = value };

    private static void AddMissing(bool present, string kind, ICollection<string> missing)
    {
        if (!present) missing.Add(kind);
    }

    private static void AddDistinct(ICollection<StandaloneHumanReviewFact> facts, string label, string value)
    {
        if (facts.Any(item => string.Equals(item.Label, label, StringComparison.Ordinal))) return;
        facts.Add(new StandaloneHumanReviewFact { Label = label, Value = value });
    }

    private static void AddSummaryFact(
        string summary,
        string marker,
        string label,
        ICollection<StandaloneHumanReviewFact> facts)
    {
        summary ??= string.Empty;
        var index = summary.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0) return;
        var value = summary[(index + marker.Length)..].TakeWhile(char.IsDigit).ToArray();
        if (value.Length > 0) AddDistinct(facts, label, new string(value));
    }

    private static string Signed(decimal value) => (value >= 0 ? "+" : string.Empty) + Number(value);
    private static string Number(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture);
}
