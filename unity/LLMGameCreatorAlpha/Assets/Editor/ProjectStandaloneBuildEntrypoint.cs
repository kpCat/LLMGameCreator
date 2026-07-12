using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LLMGameCreator
{
    public static class ProjectStandaloneBuildEntrypoint
    {
        public static void BuildWindowsHost()
        {
            var output = ArgumentValue("-llmgcStandaloneHostOutput");
            if (string.IsNullOrEmpty(output)) throw new InvalidOperationException("-llmgcStandaloneHostOutput is required.");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var scenePath = "Assets/__LLMGC_ProjectStandaloneBuild__.unity";
            try
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                new GameObject("ProjectStandalonePlayerAdapterBootstrap").AddComponent<ProjectStandalonePlayerAdapterBootstrap>();
                EditorSceneManager.SaveScene(scene, scenePath);
                var options = new BuildPlayerOptions { scenes = new[] { scenePath }, locationPathName = output, target = BuildTarget.StandaloneWindows64, options = BuildOptions.None };
                var report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new InvalidOperationException("Unity Windows host build failed: " + report.summary.result);
            }
            finally
            {
                AssetDatabase.DeleteAsset(scenePath);
                AssetDatabase.Refresh();
            }
        }

        private static string ArgumentValue(string key)
        {
            var args = Environment.GetCommandLineArgs();
            for (var index = 0; index + 1 < args.Length; index++) if (string.Equals(args[index], key, StringComparison.Ordinal)) return args[index + 1];
            return string.Empty;
        }
    }
}
