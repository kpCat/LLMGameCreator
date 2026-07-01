namespace LLMGameCreator.Application.Design.GeneratorSpineQualityConsolidation;

public sealed class GeneratorSpineQualityRiskClassifier
{
    public IReadOnlyList<GeneratorSpineQualityFinding> Classify(GeneratorSpineQualityScanResult scan)
    {
        var findings = new List<GeneratorSpineQualityFinding>();
        AddSourceFormatFindings(scan, findings);
        AddLargeFileFindings(scan, findings);
        AddUnityBootstrapFindings(scan, findings);
        AddProofQualityFindings(scan, findings);
        AddArtifactFindings(scan, findings);
        AddStateFindings(scan, findings);
        AddRepeatedRoleFindings(scan, findings);

        return findings
            .OrderBy(item => SeverityOrder(item.Severity))
            .ThenBy(item => item.FindingId, StringComparer.Ordinal)
            .ToList();
    }

    private static void AddSourceFormatFindings(GeneratorSpineQualityScanResult scan, List<GeneratorSpineQualityFinding> findings)
    {
        var minified = scan.SourceFiles.Where(item => item.IsOneLineOrMinifiedCandidate).ToList();
        if (minified.Count > 0)
        {
            findings.Add(new GeneratorSpineQualityFinding
            {
                FindingId = "GQ-P0-SOURCE-MINIFIED",
                Severity = "P0",
                Area = "source-format",
                Evidence = "Minified or one-line C# candidates: " + JoinPaths(minified.Select(item => item.RelativePath).Take(8)),
                RecommendedFutureGoal = "Immediate bounded readability repair before more generator work.",
                FixedInGoal072 = false,
                WhyNotFixed = "No automatic source reformat is performed by the scanner; if this appears in real repo evidence, Goal 072 must be BLOCKED or repaired in allowed scope."
            });
        }

        var extreme = scan.SourceFiles.Where(item => item.HasExtremeLineLength).ToList();
        if (extreme.Count > 0)
        {
            findings.Add(new GeneratorSpineQualityFinding
            {
                FindingId = "GQ-P0-SOURCE-EXTREME-LINE-LENGTH",
                Severity = "P0",
                Area = "source-format",
                Evidence = "Extreme source line length candidates: " + JoinPaths(extreme.Select(item => item.RelativePath + " maxLineLength=" + item.MaxLineLength).Take(8)),
                RecommendedFutureGoal = "Immediate bounded readability repair before more generator work.",
                FixedInGoal072 = false,
                WhyNotFixed = "Extreme line length requires a local, semantics-preserving edit in the owning seam."
            });
        }
    }

    private static void AddLargeFileFindings(GeneratorSpineQualityScanResult scan, List<GeneratorSpineQualityFinding> findings)
    {
        var largeFiles = scan.SourceFiles
            .Where(item => item.IsLargeFileCandidate)
            .OrderByDescending(item => item.LineCount)
            .ToList();
        if (largeFiles.Count > 0)
        {
            findings.Add(new GeneratorSpineQualityFinding
            {
                FindingId = "GQ-P1-LARGE-SOURCE-FILES",
                Severity = "P1",
                Area = "large-files",
                Evidence = "Large source candidates: " + JoinPaths(largeFiles.Select(item => item.RelativePath + " lines=" + item.LineCount).Take(10)),
                RecommendedFutureGoal = "Dedicated generator spine decomposition goal for very large recent seams.",
                FixedInGoal072 = false,
                WhyNotFixed = "Broad decomposition across recent goals would exceed safe bounded Goal 072 scope."
            });
        }

        if (scan.LargeMethods.Count > 0)
        {
            findings.Add(new GeneratorSpineQualityFinding
            {
                FindingId = "GQ-P1-LARGE-METHODS",
                Severity = "P1",
                Area = "large-methods",
                Evidence = "Large method candidates: " + JoinPaths(scan.LargeMethods.Select(item => item.RelativePath + "#" + item.MethodName + " lines=" + item.LineCount).Take(10)),
                RecommendedFutureGoal = "Dedicated local extraction goal for the largest methods with tests held fixed.",
                FixedInGoal072 = false,
                WhyNotFixed = "Method extraction is not attempted without a concrete behavior defect in this audit goal."
            });
        }
    }

    private static void AddUnityBootstrapFindings(GeneratorSpineQualityScanResult scan, List<GeneratorSpineQualityFinding> findings)
    {
        if (scan.UnityAlphaBootstrap.MonolithicGrowthRisk)
        {
            findings.Add(new GeneratorSpineQualityFinding
            {
                FindingId = "GQ-P1-UNITY-BOOTSTRAP-GROWTH",
                Severity = "P1",
                Area = "unity-alpha-bootstrap",
                Evidence = scan.UnityAlphaBootstrap.RelativePath
                    + " lines=" + scan.UnityAlphaBootstrap.LineCount
                    + " markerRoutes=" + scan.UnityAlphaBootstrap.MarkerRouteCount
                    + " nestedTypes=" + scan.UnityAlphaBootstrap.PrivateNestedTypeCount,
                RecommendedFutureGoal = "Unity Alpha bootstrap decomposition into local private route loaders or data adapters without changing Unity architecture.",
                FixedInGoal072 = false,
                WhyNotFixed = "A broad Unity Alpha refactor is explicitly forbidden; current proof route remains validated and should be split in a dedicated follow-up."
            });
        }
    }

    private static void AddProofQualityFindings(GeneratorSpineQualityScanResult scan, List<GeneratorSpineQualityFinding> findings)
    {
        var shallow = scan.ProductSmokeRecords.Where(item => item.ReportOnlyShallowCandidate).ToList();
        if (shallow.Count > 0)
        {
            findings.Add(new GeneratorSpineQualityFinding
            {
                FindingId = "GQ-P1-SHALLOW-PRODUCT-SMOKE",
                Severity = "P1",
                Area = "product-smoke-tests",
                Evidence = "Product smoke shallow candidates: " + JoinPaths(shallow.Select(item => item.RelativePath + " asserts=" + item.AssertCount).Take(10)),
                RecommendedFutureGoal = "Strengthen affected product smokes with count/hash/delta/marker/staging assertions.",
                FixedInGoal072 = false,
                WhyNotFixed = "Goal 071 and nearby smokes already carry strong assertions; older shallow candidates are registered for follow-up rather than broad test churn."
            });
        }

        if (!scan.Goal071ProofIndicators.ProofQualityPassed)
        {
            findings.Add(new GeneratorSpineQualityFinding
            {
                FindingId = "GQ-P0-GOAL071-PROOF-QUALITY",
                Severity = "P0",
                Area = "goal-071-proof",
                Evidence = "Goal 071 proof indicators did not prove report, staged input, command plan, player proof, markers, actions and transitions.",
                RecommendedFutureGoal = "Immediate Goal 071 proof repair before accepting Goal 072.",
                FixedInGoal072 = false,
                WhyNotFixed = "Goal 072 must block if Goal 071 proof quality is incomplete."
            });
        }
    }

    private static void AddArtifactFindings(GeneratorSpineQualityScanResult scan, List<GeneratorSpineQualityFinding> findings)
    {
        if (scan.AbsolutePathLikeArtifacts.Count > 0)
        {
            findings.Add(new GeneratorSpineQualityFinding
            {
                FindingId = "GQ-P0-ARTIFACT-ABSOLUTE-PATHS",
                Severity = "P0",
                Area = "artifact-reproducibility",
                Evidence = "Absolute-path-like artifact values found in "
                    + scan.AbsolutePathLikeArtifacts.Select(item => item.RelativePath).Distinct(StringComparer.Ordinal).Count()
                    + " compact artifact files.",
                RecommendedFutureGoal = "Immediate evidence regeneration or writer repair to remove local-machine paths.",
                FixedInGoal072 = false,
                WhyNotFixed = "The scanner redacts matched values; any real compact evidence leak must be repaired before GREEN."
            });
        }

        if (scan.TimestampLikeArtifacts.Count > 0)
        {
            findings.Add(new GeneratorSpineQualityFinding
            {
                FindingId = "GQ-P2-ARTIFACT-TIMESTAMP-LIKE-VALUES",
                Severity = "P2",
                Area = "artifact-reproducibility",
                Evidence = "Timestamp-like artifact values found in "
                    + scan.TimestampLikeArtifacts.Select(item => item.RelativePath).Distinct(StringComparer.Ordinal).Count()
                    + " compact artifact files.",
                RecommendedFutureGoal = "Future reproducibility hardening to remove or normalize volatile timestamp-like values.",
                FixedInGoal072 = false,
                WhyNotFixed = "Timestamp-like values are registered as reproducibility debt unless they are current-goal P0 path leaks or break deterministic tests."
            });
        }
    }

    private static void AddStateFindings(GeneratorSpineQualityScanResult scan, List<GeneratorSpineQualityFinding> findings)
    {
        if (scan.CurrentStateConsistency.Diagnostics.Count > 0)
        {
            findings.Add(new GeneratorSpineQualityFinding
            {
                FindingId = "GQ-P0-CURRENT-STATE-INCONSISTENT",
                Severity = "P0",
                Area = "state-docs",
                Evidence = "State-doc consistency diagnostics: " + string.Join("; ", scan.CurrentStateConsistency.Diagnostics),
                RecommendedFutureGoal = "Immediate docs quartet repair before Goal 072 review.",
                FixedInGoal072 = false,
                WhyNotFixed = "State-doc consistency is required for this goal and should be repaired in Goal 072 scope."
            });
        }
    }

    private static void AddRepeatedRoleFindings(GeneratorSpineQualityScanResult scan, List<GeneratorSpineQualityFinding> findings)
    {
        var grouped = scan.RepeatedSeamRoles
            .GroupBy(item => item.RoleName, StringComparer.Ordinal)
            .Where(group => group.Count() >= 8)
            .Select(group => group.Key + "=" + group.Count())
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        if (grouped.Count > 0)
        {
            findings.Add(new GeneratorSpineQualityFinding
            {
                FindingId = "GQ-P2-REPEATED-SEAM-ROLES",
                Severity = "P2",
                Area = "seam-patterns",
                Evidence = "Repeated role folders: " + string.Join(", ", grouped),
                RecommendedFutureGoal = "Future shared extraction or template goal after proving the risk with focused tests.",
                FixedInGoal072 = false,
                WhyNotFixed = "Broad shared loader/evidence/hash/proof-runner extraction is explicitly P2 and out of bounded Goal 072 implementation scope."
            });
        }
    }

    private static string JoinPaths(IEnumerable<string> values) =>
        string.Join("; ", values);

    private static int SeverityOrder(string severity) =>
        severity switch
        {
            "P0" => 0,
            "P1" => 1,
            "P2" => 2,
            _ => 3
        };
}
