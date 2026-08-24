using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildPlayer
{
    private static string[] EnabledScenes()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    public static void BuildMacOS()
    {
        Build(
            BuildTarget.StandaloneOSX,
            "Builds/macOS/Swipe Survival.app"
        );
    }

    public static void BuildWindows()
    {
        Build(
            BuildTarget.StandaloneWindows64,
            "Builds/Windows/Swipe Survival.exe"
        );
    }

    private static void Build(BuildTarget target, string outputPath)
    {
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = EnabledScenes(),
            locationPathName = outputPath,
            target = target,
            options = BuildOptions.CleanBuildCache
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"{target} build failed with {summary.totalErrors} errors."
            );
        }

        UnityEngine.Debug.Log(
            $"{target} build succeeded: {summary.outputPath} ({summary.totalSize} bytes)"
        );
    }
}
