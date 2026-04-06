using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that reads Unity build configuration including target platform,
    /// development/debug flags, scenes in build, installed platforms, and active
    /// compilation defines. Complements the built-in ManageScene.GetBuildSettings
    /// which only returns the scene list.
    /// </summary>
    public static class GetBuildSettingsTool
    {
        const string ToolName = "McpToolkit.GetBuildSettings";

        const string Description =
            "Reads Unity build configuration including active build target, development/debug/profiler " +
            "flags, scenes in build (path, enabled, build index), installed platform modules, build " +
            "output location, and all active compilation defines. Returns structured data for the " +
            "current build configuration.";

        const string Title = "Read Build Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true,
            Groups = new[] { "MCP Toolkit - Build" })]
        public static object HandleCommand()
        {
            try
            {
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);

                // --- Build target context ---
                string subtarget = null;
                if (buildTargetGroup == BuildTargetGroup.Standalone)
                    subtarget = EditorUserBuildSettings.standaloneBuildSubtarget.ToString();

                // --- Development / debug flags ---
                var development = EditorUserBuildSettings.development;
                var allowDebugging = EditorUserBuildSettings.allowDebugging;
                var waitForManagedDebugger = EditorUserBuildSettings.waitForManagedDebugger;
                var connectProfiler = EditorUserBuildSettings.connectProfiler;
                var buildWithDeepProfilingSupport = EditorUserBuildSettings.buildWithDeepProfilingSupport;
                var compressFilesInPackage = EditorUserBuildSettings.compressFilesInPackage;

                // --- Scenes in build ---
                var editorScenes = EditorBuildSettings.scenes;
                var scenes = new List<object>();
                int runtimeIndex = 0;
                for (int i = 0; i < editorScenes.Length; i++)
                {
                    var s = editorScenes[i];
                    scenes.Add(new
                    {
                        path = s.path,
                        guid = s.guid.ToString(),
                        enabled = s.enabled,
                        listIndex = i,
                        runtimeBuildIndex = s.enabled ? runtimeIndex++ : -1
                    });
                }

                // --- Build output location ---
                string buildLocation = null;
                try
                {
                    buildLocation = EditorUserBuildSettings.GetBuildLocation(buildTarget);
                }
                catch
                {
                    // Some targets may not have a stored build location
                }

                // --- Is a build currently in progress ---
                var isBuildingPlayer = BuildPipeline.isBuildingPlayer;

                // --- Installed platform modules ---
                var installedPlatforms = new List<string>();
                var platformsToCheck = new[]
                {
                    (BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, "StandaloneWindows64"),
                    (BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, "StandaloneOSX"),
                    (BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64, "StandaloneLinux64"),
                    (BuildTargetGroup.Android, BuildTarget.Android, "Android"),
                    (BuildTargetGroup.iOS, BuildTarget.iOS, "iOS"),
                    (BuildTargetGroup.WebGL, BuildTarget.WebGL, "WebGL"),
                    (BuildTargetGroup.GameCoreXboxOne, BuildTarget.GameCoreXboxOne, "GameCoreXboxOne"),
                    (BuildTargetGroup.GameCoreXboxSeries, BuildTarget.GameCoreXboxSeries, "GameCoreXboxSeries"),
                    (BuildTargetGroup.PS4, BuildTarget.PS4, "PS4"),
                    (BuildTargetGroup.PS5, BuildTarget.PS5, "PS5"),
                    (BuildTargetGroup.EmbeddedLinux, BuildTarget.EmbeddedLinux, "EmbeddedLinux"),
                    (BuildTargetGroup.LinuxHeadlessSimulation, BuildTarget.LinuxHeadlessSimulation, "LinuxHeadlessSimulation"),
                    (BuildTargetGroup.VisionOS, BuildTarget.VisionOS, "VisionOS"),
                };

                foreach (var (group, target, name) in platformsToCheck)
                {
                    try
                    {
                        if (BuildPipeline.IsBuildTargetSupported(group, target))
                            installedPlatforms.Add(name);
                    }
                    catch
                    {
                        // Enum value may not exist in this Unity version
                    }
                }

                // --- Active compilation defines ---
                var compilationDefines = EditorUserBuildSettings.activeScriptCompilationDefines;

                var data = new
                {
                    // Build target
                    activeBuildTarget = buildTarget.ToString(),
                    buildTargetGroup = buildTargetGroup.ToString(),
                    standaloneBuildSubtarget = subtarget,

                    // Development / debug flags
                    developmentBuild = development,
                    allowDebugging,
                    waitForManagedDebugger,
                    connectProfiler,
                    buildWithDeepProfilingSupport,
                    compressFilesInPackage,

                    // Scenes in build
                    scenesInBuild = scenes,

                    // Build output
                    buildLocation,

                    // Build state
                    isBuildingPlayer,

                    // Installed platforms
                    installedPlatforms,

                    // Compilation defines
                    activeCompilationDefines = compilationDefines
                };

                return Response.Success(
                    $"Build settings retrieved for target: {buildTarget}.",
                    data);
            }
            catch (Exception ex)
            {
                return Response.Error($"GET_BUILD_SETTINGS_FAILED: {ex.Message}");
            }
        }
    }
}
