using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LLMGameCreatorAlpha
{
    [Serializable]
    public sealed class OfflineGeoworldAlphaAcceptanceResult
    {
        public string goalId = "goal_110_offline_geoworld_alpha_manual_acceptance_gate";
        public string manualGate = "offline_geoworld_alpha_manual_acceptance_verification";
        public bool accepted;
        public bool manualAcceptancePending = true;
        public bool automatedGatePassed = true;
        public string resultStatus = "manual_result_required";
        public string checklistHash = string.Empty;
        public string resultTemplateHash = string.Empty;
        public string packagePath = string.Empty;
        public string diagnostics = string.Empty;
        public string resultHash = string.Empty;
        public List<OfflineGeoworldAlphaAcceptanceStepResult> steps =
            new List<OfflineGeoworldAlphaAcceptanceStepResult>();

        public static OfflineGeoworldAlphaAcceptanceResult CreatePending(
            IEnumerable<string> stepIds,
            string packagePathValue,
            string checklistHashValue,
            string resultTemplateHashValue)
        {
            var result = new OfflineGeoworldAlphaAcceptanceResult
            {
                accepted = false,
                manualAcceptancePending = true,
                automatedGatePassed = true,
                resultStatus = "manual_result_pending",
                checklistHash = checklistHashValue ?? string.Empty,
                resultTemplateHash = resultTemplateHashValue ?? string.Empty,
                packagePath = packagePathValue ?? string.Empty
            };
            foreach (var stepId in stepIds)
            {
                result.steps.Add(new OfflineGeoworldAlphaAcceptanceStepResult
                {
                    stepId = stepId,
                    status = "pending",
                    notes = string.Empty,
                    evidenceRef = stepId + "Evidence"
                });
            }

            result.resultHash = result.ComputeStableHash();
            return result;
        }

        public string ToJson()
        {
            resultHash = ComputeStableHash();
            var builder = new StringBuilder();
            builder.AppendLine("{");
            AppendJsonString(builder, "goalId", goalId, true);
            AppendJsonString(builder, "manualGate", manualGate, true);
            AppendJsonBool(builder, "accepted", accepted, true);
            AppendJsonBool(builder, "manualAcceptancePending", manualAcceptancePending, true);
            AppendJsonBool(builder, "automatedGatePassed", automatedGatePassed, true);
            AppendJsonString(builder, "resultStatus", resultStatus, true);
            AppendJsonString(builder, "checklistHash", checklistHash, true);
            AppendJsonString(builder, "resultTemplateHash", resultTemplateHash, true);
            AppendJsonString(builder, "packagePath", packagePath, true);
            AppendJsonString(builder, "diagnostics", diagnostics, true);
            AppendJsonString(builder, "resultHash", resultHash, true);
            builder.AppendLine("  \"steps\": [");
            var stepList = steps ?? new List<OfflineGeoworldAlphaAcceptanceStepResult>();
            for (var i = 0; i < stepList.Count; i++)
            {
                var step = stepList[i] ?? new OfflineGeoworldAlphaAcceptanceStepResult();
                builder.AppendLine("    {");
                AppendJsonString(builder, "stepId", step.stepId, true, 6);
                AppendJsonString(builder, "status", step.status, true, 6);
                AppendJsonString(builder, "notes", step.notes, true, 6);
                AppendJsonString(builder, "evidenceRef", step.evidenceRef, false, 6);
                builder.Append("    }");
                if (i + 1 < stepList.Count)
                {
                    builder.Append(',');
                }

                builder.AppendLine();
            }

            builder.AppendLine("  ]");
            builder.AppendLine("}");
            return builder.ToString();
        }

        public static OfflineGeoworldAlphaAcceptanceResult FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var result = new OfflineGeoworldAlphaAcceptanceResult
            {
                goalId = StringField(json, "goalId", "goal_110_offline_geoworld_alpha_manual_acceptance_gate"),
                manualGate = StringField(json, "manualGate", "offline_geoworld_alpha_manual_acceptance_verification"),
                accepted = BoolField(json, "accepted", false),
                manualAcceptancePending = BoolField(json, "manualAcceptancePending", true),
                automatedGatePassed = BoolField(json, "automatedGatePassed", false),
                resultStatus = StringField(json, "resultStatus", "manual_result_required"),
                checklistHash = StringField(json, "checklistHash", string.Empty),
                resultTemplateHash = StringField(json, "resultTemplateHash", string.Empty),
                packagePath = StringField(json, "packagePath", string.Empty),
                diagnostics = StringField(json, "diagnostics", string.Empty),
                resultHash = StringField(json, "resultHash", string.Empty)
            };
            result.steps.Clear();
            foreach (var block in Blocks(json, "stepId"))
            {
                result.steps.Add(new OfflineGeoworldAlphaAcceptanceStepResult
                {
                    stepId = StringField(block, "stepId", string.Empty),
                    status = StringField(block, "status", string.Empty),
                    notes = StringField(block, "notes", string.Empty),
                    evidenceRef = StringField(block, "evidenceRef", string.Empty)
                });
            }

            return result;
        }

        public string ComputeStableHash()
        {
            var count = steps == null ? 0 : steps.Count;
            return goalId + "|" + manualGate + "|" + accepted + "|" + manualAcceptancePending
                   + "|" + automatedGatePassed + "|" + resultStatus + "|" + checklistHash
                   + "|" + resultTemplateHash + "|" + count;
        }

        private static void AppendJsonString(
            StringBuilder builder,
            string name,
            string value,
            bool trailingComma,
            int indent = 2)
        {
            builder.Append(new string(' ', indent));
            builder.Append('"').Append(name).Append("\": \"").Append(EscapeJson(value)).Append('"');
            if (trailingComma)
            {
                builder.Append(',');
            }

            builder.AppendLine();
        }

        private static void AppendJsonBool(
            StringBuilder builder,
            string name,
            bool value,
            bool trailingComma)
        {
            builder.Append("  \"").Append(name).Append("\": ").Append(value ? "true" : "false");
            if (trailingComma)
            {
                builder.Append(',');
            }

            builder.AppendLine();
        }

        private static string EscapeJson(string value)
        {
            var builder = new StringBuilder();
            foreach (var ch in value ?? string.Empty)
            {
                switch (ch)
                {
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (ch < ' ')
                        {
                            builder.Append("\\u").Append(((int)ch).ToString("x4"));
                        }
                        else
                        {
                            builder.Append(ch);
                        }

                        break;
                }
            }

            return builder.ToString();
        }

        private static List<string> Blocks(string json, string anchorField)
        {
            var result = new List<string>();
            foreach (Match match in Regex.Matches(json ?? string.Empty, "\\{[^\\{\\}]*\""
                                                                   + Regex.Escape(anchorField)
                                                                   + "\"[\\s\\S]*?\\}"))
            {
                result.Add(match.Value);
            }

            return result;
        }

        private static string StringField(string json, string field, string fallback)
        {
            var match = Regex.Match(
                json ?? string.Empty,
                "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"((?:\\\\.|[^\"])*)\"");
            return match.Success ? UnescapeJson(match.Groups[1].Value) : fallback;
        }

        private static bool BoolField(string json, string field, bool fallback)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(true|false)");
            bool value;
            return match.Success && bool.TryParse(match.Groups[1].Value, out value) ? value : fallback;
        }

        private static string UnescapeJson(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOf('\\') < 0)
            {
                return value ?? string.Empty;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if (ch != '\\' || i + 1 >= value.Length)
                {
                    builder.Append(ch);
                    continue;
                }

                i++;
                switch (value[i])
                {
                    case '"':
                        builder.Append('"');
                        break;
                    case '\\':
                        builder.Append('\\');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    default:
                        builder.Append(value[i]);
                        break;
                }
            }

            return builder.ToString();
        }
    }

    [Serializable]
    public sealed class OfflineGeoworldAlphaAcceptanceStepResult
    {
        public string stepId = string.Empty;
        public string status = "pending";
        public string notes = string.Empty;
        public string evidenceRef = string.Empty;
    }
}
