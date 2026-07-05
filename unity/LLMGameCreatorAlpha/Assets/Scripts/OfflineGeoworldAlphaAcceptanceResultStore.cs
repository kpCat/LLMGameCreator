using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldAlphaAcceptanceResultStore : MonoBehaviour
    {
        private const string ResultFileName = "offline-geoworld-alpha-acceptance-result.json";
        private OfflineGeoworldAlphaAcceptanceResult lastResult;
        private string lastStatus = "No local acceptance result loaded.";

        public OfflineGeoworldAlphaAcceptanceResult LastResult { get { return lastResult; } }
        public string LastStatus { get { return lastStatus; } }
        public string ResultPath { get { return BuildResultPath(); } }

        public OfflineGeoworldAlphaAcceptanceResult CreatePendingResult(
            IEnumerable<string> stepIds,
            string packagePath,
            string checklistHash,
            string resultTemplateHash)
        {
            lastResult = OfflineGeoworldAlphaAcceptanceResult.CreatePending(
                stepIds,
                packagePath,
                checklistHash,
                resultTemplateHash);
            lastStatus = "Pending Alpha acceptance result created.";
            return lastResult;
        }

        public bool SaveResult(OfflineGeoworldAlphaAcceptanceResult result)
        {
            if (result == null)
            {
                lastStatus = "Save rejected: result is missing.";
                return false;
            }

            var path = BuildResultPath();
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, result.ToJson(), Encoding.UTF8);
            lastResult = result;
            lastStatus = "Saved Alpha acceptance result: " + path;
            return true;
        }

        public OfflineGeoworldAlphaAcceptanceResult LoadResult()
        {
            var path = BuildResultPath();
            if (!File.Exists(path))
            {
                lastStatus = "Load skipped: result file missing.";
                return null;
            }

            var json = File.ReadAllText(path, Encoding.UTF8);
            lastResult = OfflineGeoworldAlphaAcceptanceResult.FromJson(json);
            lastStatus = lastResult == null
                ? "Load failed: result JSON was empty."
                : "Loaded Alpha acceptance result status="
                  + lastResult.resultStatus
                  + " steps="
                  + (lastResult.steps == null ? 0 : lastResult.steps.Count);
            return lastResult;
        }

        public void ClearResult()
        {
            var path = BuildResultPath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            lastResult = null;
            lastStatus = "Cleared Alpha acceptance result.";
        }

        private static string BuildResultPath()
        {
            var root = Application.persistentDataPath;
            if (string.IsNullOrEmpty(root))
            {
                root = Path.Combine(Directory.GetCurrentDirectory(), "LLMGameCreatorAlphaAcceptance");
            }

            return Path.Combine(root, ResultFileName);
        }
    }
}
