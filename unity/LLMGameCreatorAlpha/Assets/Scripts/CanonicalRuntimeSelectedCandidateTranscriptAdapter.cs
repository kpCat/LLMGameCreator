using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace LLMGameCreatorAlpha
{
    public sealed class CanonicalRuntimeSelectedCandidateTranscriptSmokeResult
    {
        public bool Passed;
        public string TranscriptPath = string.Empty;
        public string StateSummaryPath = string.Empty;
        public string Diagnostics = string.Empty;
    }

    public static class CanonicalRuntimeSelectedCandidateTranscriptAdapter
    {
        public const string PassMarker = "GOAL134_CANONICAL_RUNTIME_TRANSCRIPT_PLAYER_PASS";
        public const string FailMarker = "GOAL134_CANONICAL_RUNTIME_TRANSCRIPT_PLAYER_FAIL";

        public static CanonicalRuntimeSelectedCandidateTranscriptSmokeResult RunFromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            var transcriptPath = ReadArgument(args, "-llmgcCanonicalRuntimeTranscriptPath");
            var stateSummaryPath = ReadArgument(args, "-llmgcCanonicalRuntimeStateSummaryPath");
            return Consume(transcriptPath, stateSummaryPath);
        }

#if UNITY_EDITOR
        public static void RunBatchmodeCanonicalRuntimeSelectedCandidateTranscriptSmoke()
        {
            var exitCode = 0;
            try
            {
                var result = RunFromCommandLine();
                if (result.Passed)
                {
                    Debug.Log(PassMarker + "\n" + result.Diagnostics);
                }
                else
                {
                    exitCode = 1;
                    Debug.LogError(FailMarker + "\n" + result.Diagnostics);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError(FailMarker + "\n" + ex);
            }
            finally
            {
                if (Application.isBatchMode) { EditorApplication.Exit(exitCode); }
            }
        }
#endif

        public static CanonicalRuntimeSelectedCandidateTranscriptSmokeResult Consume(
            string transcriptPath,
            string stateSummaryPath)
        {
            var result = new CanonicalRuntimeSelectedCandidateTranscriptSmokeResult
            {
                TranscriptPath = transcriptPath ?? string.Empty,
                StateSummaryPath = stateSummaryPath ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(transcriptPath) || !File.Exists(transcriptPath))
            {
                result.Diagnostics = "canonical runtime transcript path is missing";
                return result;
            }

            if (string.IsNullOrWhiteSpace(stateSummaryPath) || !File.Exists(stateSummaryPath))
            {
                result.Diagnostics = "canonical runtime state summary path is missing";
                return result;
            }

            var transcript = File.ReadAllText(transcriptPath);
            var stateSummary = File.ReadAllText(stateSummaryPath);
            var transcriptReady =
                ContainsJsonPair(transcript, "source", "gameplay-runtime")
                && ContainsJsonPair(transcript, "eventType", "RecipeCrafted")
                && ContainsJsonPair(transcript, "eventType", "EncounterStarted")
                && transcript.Contains("\"stateHashAfter\"");
            var stateReady =
                stateSummary.Contains("\"candidateId\"")
                && ContainsJsonPair(stateSummary, "packageId", "game/minimal-map-game")
                && stateSummary.Contains("\"finalStateHash\"")
                && stateSummary.Contains("\"stateHashChain\"");

            result.Passed = transcriptReady && stateReady;
            result.Diagnostics =
                "transcriptReady="
                + transcriptReady
                + "; stateSummaryReady="
                + stateReady
                + "; transcriptPath="
                + transcriptPath
                + "; stateSummaryPath="
                + stateSummaryPath;
            return result;
        }

        private static string ReadArgument(string[] args, string name)
        {
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return string.Empty;
        }

        private static bool ContainsJsonPair(string json, string name, string value)
        {
            return json.Contains("\"" + name + "\": \"" + value + "\"")
                   || json.Contains("\"" + name + "\":\"" + value + "\"");
        }
    }
}
