using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Deenote.Editor
{
    [InitializeOnLoad]
    public static class ProjectBuildMenu
    {
        private const string ProductName = "Deenote";

        private const string RootKey = "Deenote.Build.Root";
        private const string VersionKey = "Deenote.Build.Version";
        private const string SwitchingKey = "Deenote.Build.Switching";
        private const string StepKey = "Deenote.Build.Step";

        private const int StepNone = -1;
        private const int StepBuildX64 = 0;
        private const int StepBuildX32 = 1;
        private const int StepPackage = 2;

        private static bool s_processing;

        static ProjectBuildMenu()
        {
            // The build may have been interrupted by a domain reload while switching
            // the active build target; resume it in that case.
            if (GetStep() != StepNone)
                EditorApplication.update += OnEditorUpdate;
        }

        [MenuItem("Deenote/Build")]
        private static void BuildWindows()
        {
            if (GetStep() != StepNone)
            {
                EditorUtility.DisplayDialog("Deenote Build", "A build is already in progress.", "OK");
                return;
            }

            string outputRoot = EditorUtility.OpenFolderPanel("Select Build Output Folder", string.Empty, string.Empty);
            if (string.IsNullOrEmpty(outputRoot))
                return;

            string version = PlayerSettings.bundleVersion;
            if (string.IsNullOrWhiteSpace(version))
                version = "0.0.0";

            SessionState.SetString(RootKey, outputRoot);
            SessionState.SetString(VersionKey, version);
            SessionState.SetInt(SwitchingKey, 0);
            SessionState.SetInt(StepKey, StepBuildX64);

            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (s_processing)
                return;

            int step = GetStep();
            if (step == StepNone)
            {
                EditorApplication.update -= OnEditorUpdate;
                return;
            }

            s_processing = true;
            try
            {
                switch (step)
                {
                    case StepBuildX64:
                        ContinueBuild(BuildTarget.StandaloneWindows64, "Windows x64", StepBuildX32);
                        break;
                    case StepBuildX32:
                        ContinueBuild(BuildTarget.StandaloneWindows, "Windows x32", StepPackage);
                        break;
                    case StepPackage:
                        PackageBuilds();
                        break;
                    default:
                        FinishBuild();
                        break;
                }
            }
            catch (Exception ex)
            {
                FailBuild(ex);
            }
            finally
            {
                s_processing = false;
            }
        }

        private static void ContinueBuild(BuildTarget target, string targetName, int nextStep)
        {
            if (EditorUserBuildSettings.activeBuildTarget != target)
            {
                if (SessionState.GetInt(SwitchingKey, 0) == 0)
                {
                    EditorUtility.DisplayProgressBar($"Building {targetName}", "Switching build target...", 0.05f);
                    SessionState.SetInt(SwitchingKey, 1);
                    if (!EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Standalone, target))
                        throw new InvalidOperationException($"Could not switch the active build target to {targetName}.");
                }

                // Wait until the target has switched and the editor has reloaded the
                // scripts compiled for it before starting the build.
                if (EditorUserBuildSettings.activeBuildTarget != target
                    || EditorApplication.isCompiling
                    || EditorApplication.isUpdating)
                {
                    return;
                }

                SessionState.SetInt(SwitchingKey, 0);
            }

            string outputRoot = SessionState.GetString(RootKey, string.Empty);
            Build(target, targetName, GetOutputDirectory(outputRoot, target));

            SessionState.SetInt(StepKey, nextStep);
        }

        private static void Build(BuildTarget target, string targetName, string outputDirectory)
        {
            string executablePath = Path.Combine(outputDirectory, ProductName + ".exe");

            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, true);
            Directory.CreateDirectory(outputDirectory);

            EditorUtility.DisplayProgressBar($"Building {targetName}", "Preparing...", 0.1f);

            BuildPlayerOptions options = new() {
                scenes = GetEnabledScenes(),
                locationPathName = executablePath,
                target = target,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Building {targetName} failed: {summary.result} ({summary.totalErrors} error(s)). " +
                    "See the Console for details.");
            }

            Debug.Log($"Building {targetName} succeeded: {executablePath}");
        }

        private static void PackageBuilds()
        {
            string outputRoot = SessionState.GetString(RootKey, string.Empty);
            string version = SessionState.GetString(VersionKey, "0.0.0");

            string x64ZipPath = Path.Combine(outputRoot, $"{ProductName}-{version}.zip");
            string x32ZipPath = Path.Combine(outputRoot, $"{ProductName}-{version}-32bit.zip");

            EditorUtility.DisplayProgressBar("Packaging", "Compressing the x64 build...", 0.5f);
            CreateZip(GetOutputDirectory(outputRoot, BuildTarget.StandaloneWindows64), x64ZipPath);

            EditorUtility.DisplayProgressBar("Packaging", "Compressing the x32 build...", 0.9f);
            CreateZip(GetOutputDirectory(outputRoot, BuildTarget.StandaloneWindows), x32ZipPath);

            FinishBuild();

            EditorUtility.RevealInFinder(outputRoot);
            EditorUtility.DisplayDialog("Build Completed",
                $"Output folder:\n{outputRoot}\n\n{Path.GetFileName(x64ZipPath)}\n{Path.GetFileName(x32ZipPath)}",
                "OK");
        }

        private static void FailBuild(Exception exception)
        {
            FinishBuild();
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("Build Failed", exception.Message, "OK");
        }

        private static void FinishBuild()
        {
            SessionState.EraseString(RootKey);
            SessionState.EraseString(VersionKey);
            SessionState.EraseInt(SwitchingKey);
            SessionState.EraseInt(StepKey);

            EditorApplication.update -= OnEditorUpdate;
            EditorUtility.ClearProgressBar();
        }

        private static int GetStep()
        {
            return SessionState.GetInt(StepKey, StepNone);
        }

        private static string GetOutputDirectory(string outputRoot, BuildTarget target)
        {
            string architectureName = target == BuildTarget.StandaloneWindows64 ? "x64" : "x32";
            return Path.Combine(outputRoot, architectureName, ProductName);
        }

        private static string[] GetEnabledScenes()
        {
            List<string> scenes = new();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }
            return scenes.ToArray();
        }

        private static void CreateZip(string sourceDirectory, string zipPath)
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            // Include the containing folder itself so the archive extracts to a "Deenote" folder.
            ZipFile.CreateFromDirectory(sourceDirectory, zipPath, System.IO.Compression.CompressionLevel.Optimal, true);
        }
    }
}