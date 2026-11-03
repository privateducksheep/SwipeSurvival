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

    /// <summary>
    /// Browser build. The page fills the whole window at any size (template:
    /// Assets/WebGLTemplates/Responsive) and works on hosts that do not send
    /// compression headers, such as GitHub Pages. Needs the WebGL Build Support module.
    /// </summary>
    public static void BuildWebGL()
    {
        Build(
            BuildTarget.WebGL,
            "Builds/Web"
        );
    }

    /// <summary>
    /// Development build that screenshots every scene at many window sizes.
    /// It uses its own product name so it never touches the real game's saved
    /// window size, volume or high score. See Assets/Scripts/Dev/LayoutCapture.cs.
    /// </summary>
    public static void BuildMacOSLayoutCapture()
    {
        string productName = PlayerSettings.productName;
        PlayerSettings.productName = productName + " Layout Capture";
        try
        {
            Build(
                BuildTarget.StandaloneOSX,
                "Builds/LayoutCapture/Swipe Survival Layout Capture.app",
                BuildOptions.None,
                new[] { "SWIPE_CAPTURE" }
            );
        }
        finally
        {
            PlayerSettings.productName = productName;
        }
    }

    private static void Build(
        BuildTarget target,
        string outputPath,
        BuildOptions buildOptions = BuildOptions.CleanBuildCache,
        string[] extraDefines = null
    )
    {
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = EnabledScenes(),
            locationPathName = outputPath,
            target = target,
            options = buildOptions,
            extraScriptingDefines = extraDefines
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
