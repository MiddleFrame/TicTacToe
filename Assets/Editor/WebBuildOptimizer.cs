using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.WebGL;
using UnityEngine;

public sealed class WebBuildOptimizer : IPreprocessBuildWithReport
{
    private const string BuildLogPath = "Logs/web-size-optimized-build.log";

    public int callbackOrder => -1000;

    [MenuItem("Tools/Build/Apply Web Size Settings")]
    private static void ApplySettingsFromMenu()
    {
        ApplyRecommendedSettings();
        Debug.Log("Applied optimized release settings for Web builds.");
    }

    [MenuItem("Tools/Build/Build Optimized Web Release")]
    private static void BuildFromMenu()
    {
        ApplyRecommendedSettings();
        BuildOptimizedWebRelease();
    }

    public static void BuildOptimizedWebReleaseBatch()
    {
        EnsureWebBuildTargetIsActive();
        ApplyRecommendedSettings();
        BuildOptimizedWebRelease();

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(File.ReadAllText(BuildLogPath).StartsWith("Result: Succeeded", StringComparison.Ordinal) ? 0 : 1);
        }
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL &&
            (report.summary.options & BuildOptions.Development) == 0)
        {
            ApplyRecommendedSettings();
        }
    }

    private static void ApplyRecommendedSettings()
    {
        var webTarget = NamedBuildTarget.WebGL;

        PlayerSettings.SetIl2CppCodeGeneration(webTarget, Il2CppCodeGeneration.OptimizeSize);
        PlayerSettings.SetManagedStrippingLevel(webTarget, ManagedStrippingLevel.Medium);
        PlayerSettings.stripEngineCode = true;
        PlayerSettings.stripUnusedMeshComponents = true;

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.WebGL.wasm2023 = true;

        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.allowDebugging = false;
        EditorUserBuildSettings.connectProfiler = false;
        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;
        UserBuildSettings.codeOptimization = WasmCodeOptimization.DiskSizeLTO;

        AssetDatabase.SaveAssets();
    }

    private static void BuildOptimizedWebRelease()
    {
        EnsureWebBuildTargetIsActive();

        var configuredOutputPath = Environment.GetEnvironmentVariable("CODEX_WEB_BUILD_OUTPUT");
        var outputPath = string.IsNullOrWhiteSpace(configuredOutputPath)
            ? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "build-optimized"))
            : Path.GetFullPath(configuredOutputPath);
        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        Directory.CreateDirectory(Path.GetDirectoryName(BuildLogPath) ?? "Logs");

        try
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });

            var summary = report.summary;
            var message =
                $"Result: {summary.result}{Environment.NewLine}" +
                $"Output: {outputPath}{Environment.NewLine}" +
                $"Unity reported size: {summary.totalSize} bytes{Environment.NewLine}" +
                $"Duration: {summary.totalTime}{Environment.NewLine}" +
                $"Warnings: {summary.totalWarnings}{Environment.NewLine}" +
                $"Errors: {summary.totalErrors}{Environment.NewLine}";

            File.WriteAllText(BuildLogPath, message);
            Debug.Log(message);
        }
        catch (Exception exception)
        {
            File.WriteAllText(BuildLogPath, exception.ToString());
            Debug.LogException(exception);
        }
    }

    private static void EnsureWebBuildTargetIsActive()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL &&
            !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            throw new InvalidOperationException("Unity could not switch the active build target to WebGL.");
        }
    }
}
