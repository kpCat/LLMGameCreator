#if UNITY_EDITOR
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldAlphaSlicePackageWindow : EditorWindow
    {
        private const string MenuPath = "LLMGameCreator/Offline Geoworld Alpha Slice Package";
        private const string RelativeRoot = "LLMGameCreator/OfflineGeoworldGoal109";
        private const string AcceptanceGateFileName = "offline-geoworld-alpha-export-acceptance-gate.json";
        private const string RunbookFileName = "offline-geoworld-alpha-export-runbook.md";

        private readonly OfflineGeoworldAlphaSlicePackageVerifier verifier =
            new OfflineGeoworldAlphaSlicePackageVerifier();

        private string packageRoot = string.Empty;
        private string statusLine = "Not verified";
        private string runbookSummary = string.Empty;
        private string acceptanceSummary = string.Empty;
        private bool packageReady;
        private int packageFileCount;
        private int indexedFileCount;
        private int verifiedChecksumCount;
        private Vector2 scrollPosition;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<OfflineGeoworldAlphaSlicePackageWindow>("Offline Geoworld Package");
        }

        private void OnEnable()
        {
            RefreshPackageStatus();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField("Package Root", packageRoot);
            EditorGUILayout.LabelField("Status", statusLine);
            EditorGUILayout.LabelField("PackageReady", packageReady.ToString());
            EditorGUILayout.LabelField("Package Files", packageFileCount.ToString());
            EditorGUILayout.LabelField("Indexed Files", indexedFileCount.ToString());
            EditorGUILayout.LabelField("Verified Checksums", verifiedChecksumCount.ToString());
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runbook");
            EditorGUILayout.TextArea(runbookSummary, GUILayout.MinHeight(130));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Acceptance");
            EditorGUILayout.TextArea(acceptanceSummary, GUILayout.MinHeight(120));
            EditorGUILayout.Space();
            if (GUILayout.Button("Verify Package"))
            {
                RefreshPackageStatus();
            }

            EditorGUILayout.EndScrollView();
        }

        public void RefreshPackageStatus()
        {
            packageRoot = Path.Combine(Application.streamingAssetsPath, RelativeRoot);
            packageReady = verifier.VerifyPackageRoot(packageRoot);
            packageFileCount = verifier.PackageFileCount;
            indexedFileCount = verifier.IndexedFileCount;
            verifiedChecksumCount = verifier.VerifiedChecksumCount;
            statusLine = verifier.StatusLine;
            runbookSummary = MarkdownSummary(Path.Combine(packageRoot, RunbookFileName));
            acceptanceSummary = AcceptanceSummary(Path.Combine(packageRoot, AcceptanceGateFileName));
        }

        private static string MarkdownSummary(string path)
        {
            if (!File.Exists(path))
            {
                return "runbook missing";
            }

            var text = File.ReadAllText(path, Encoding.UTF8);
            var lines = text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            return string.Join("\n", lines);
        }

        private static string AcceptanceSummary(string path)
        {
            if (!File.Exists(path))
            {
                return "acceptance gate missing";
            }

            var json = File.ReadAllText(path, Encoding.UTF8);
            return "accepted=" + BoolField(json, "accepted")
                   + "\nmanualGate=" + StringField(json, "manualGate")
                   + "\nstatus=" + StringField(json, "status");
        }

        private static string StringField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*\"([^\"]*)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static string BoolField(string json, string field)
        {
            var match = Regex.Match(json ?? string.Empty, "\"" + Regex.Escape(field) + "\"\\s*:\\s*(true|false)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
#endif
