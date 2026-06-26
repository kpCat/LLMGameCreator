using System;
using System.IO;
using LLMGameCreatorAlpha;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LLMGameCreatorAlpha.Editor
{
    public static class AlphaBuildEntrypoint
    {
        private const string StreamingPayloadRelativePath = "Assets/StreamingAssets/LLMGameCreatorAlpha";
        private const string BootstrapScenePath = "Assets/AlphaBootstrap.generated.unity";
        private const string ExecutableName = "LLMGameCreatorAlpha.exe";

        public static void BuildWindows64()
        {
            var exitCode = 0;
            try
            {
                var projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
                var repoRoot = Directory.GetParent(Directory.GetParent(projectRoot)!.FullName)!.FullName;
                var stagingRoot = GetArgumentValue("-alphaStagingPath", Path.Combine(repoRoot, ".llmgc", "procedural", "alpha-runnable-build", "staging"));
                var outputRoot = GetArgumentValue("-alphaBuildOutputPath", Path.Combine(repoRoot, ".llmgc", "procedural", "alpha-runnable-build", "build", "windows"));
                var executablePath = Path.Combine(outputRoot, ExecutableName);

                if (!Directory.Exists(stagingRoot))
                {
                    throw new DirectoryNotFoundException("Alpha staging folder was not found: " + stagingRoot);
                }

                Directory.CreateDirectory(outputRoot);
                CopyStagingToStreamingAssets(projectRoot, stagingRoot);
                CreateBootstrapScene();

                var buildOptions = new BuildPlayerOptions
                {
                    scenes = new[] { BootstrapScenePath },
                    locationPathName = executablePath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development
                };
                var report = BuildPipeline.BuildPlayer(buildOptions);
                if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    throw new InvalidOperationException("Unity build failed with result " + report.summary.result);
                }
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Debug.LogError(ex.ToString());
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrap = new GameObject("AlphaRuntimeBootstrap");
            bootstrap.AddComponent<AlphaRuntimeBootstrap>();
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
            AssetDatabase.Refresh();
        }

        private static void CopyStagingToStreamingAssets(string projectRoot, string stagingRoot)
        {
            var targetRoot = Path.Combine(projectRoot, StreamingPayloadRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (Directory.Exists(targetRoot))
            {
                Directory.Delete(targetRoot, recursive: true);
            }

            Directory.CreateDirectory(targetRoot);
            foreach (var sourcePath in Directory.EnumerateFiles(stagingRoot, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(stagingRoot, sourcePath);
                var targetPath = Path.Combine(targetRoot, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.Copy(sourcePath, targetPath, overwrite: true);
            }

            AssetDatabase.Refresh();
        }

        private static string GetArgumentValue(string name, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            for (var index = 0; index < args.Length - 1; index++)
            {
                if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    return args[index + 1];
                }
            }

            return fallback;
        }
    }
}
