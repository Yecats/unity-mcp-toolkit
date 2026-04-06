using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that modifies Unity build settings including development/debug/profiler
    /// flags and scene list operations (add, remove, enable, disable, reorder).
    /// Excludes SwitchActiveBuildTarget and BuildPlayer for safety.
    /// </summary>
    public static class SetBuildSettingsTool
    {
        const string ToolName = "McpToolkit.SetBuildSettings";

        const string Description =
            "Modifies Unity build settings. Toggle development build, script debugging, profiler, " +
            "deep profiling, wait for debugger, and file compression. Manage scenes in build: " +
            "add, remove, enable, disable, or reorder scenes. Excludes platform switching and " +
            "build execution for safety. Only supplied parameters are changed.";

        const string Title = "Modify Build Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "MCP Toolkit - Build" })]
        public static object HandleCommand(SetBuildSettingsParams parameters)
        {
            if (parameters == null)
                return Response.Error("INVALID_PARAMETER: No parameters provided.");

            try
            {
                var changes = new List<string>();

                // --- Development / debug flags ---
                if (parameters.DevelopmentBuild.HasValue)
                {
                    EditorUserBuildSettings.development = parameters.DevelopmentBuild.Value;
                    changes.Add($"developmentBuild = {parameters.DevelopmentBuild.Value}");
                }
                if (parameters.AllowDebugging.HasValue)
                {
                    EditorUserBuildSettings.allowDebugging = parameters.AllowDebugging.Value;
                    changes.Add($"allowDebugging = {parameters.AllowDebugging.Value}");
                }
                if (parameters.WaitForManagedDebugger.HasValue)
                {
                    EditorUserBuildSettings.waitForManagedDebugger = parameters.WaitForManagedDebugger.Value;
                    changes.Add($"waitForManagedDebugger = {parameters.WaitForManagedDebugger.Value}");
                }
                if (parameters.ConnectProfiler.HasValue)
                {
                    EditorUserBuildSettings.connectProfiler = parameters.ConnectProfiler.Value;
                    changes.Add($"connectProfiler = {parameters.ConnectProfiler.Value}");
                }
                if (parameters.BuildWithDeepProfilingSupport.HasValue)
                {
                    EditorUserBuildSettings.buildWithDeepProfilingSupport = parameters.BuildWithDeepProfilingSupport.Value;
                    changes.Add($"buildWithDeepProfilingSupport = {parameters.BuildWithDeepProfilingSupport.Value}");
                }
                if (parameters.CompressFilesInPackage.HasValue)
                {
                    EditorUserBuildSettings.compressFilesInPackage = parameters.CompressFilesInPackage.Value;
                    changes.Add($"compressFilesInPackage = {parameters.CompressFilesInPackage.Value}");
                }

                // --- Scene list operations ---
                if (!string.IsNullOrEmpty(parameters.SceneAction))
                {
                    var sceneChange = ApplySceneAction(parameters);
                    changes.Add(sceneChange);
                }

                if (changes.Count == 0)
                    return Response.Success("No changes applied — all parameters were null/omitted.",
                        new { changesApplied = 0 });

                return Response.Success(
                    $"Build settings updated: {changes.Count} change(s) applied.",
                    new { changesApplied = changes.Count, changes });
            }
            catch (Exception ex)
            {
                return Response.Error($"SET_BUILD_SETTINGS_FAILED: {ex.Message}");
            }
        }

        static string ApplySceneAction(SetBuildSettingsParams parameters)
        {
            var action = parameters.SceneAction.Trim();
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            switch (action.ToLowerInvariant())
            {
                case "add":
                    return AddScene(scenes, parameters.ScenePath);

                case "remove":
                    return RemoveScene(scenes, parameters.ScenePath);

                case "enable":
                    return SetSceneEnabled(scenes, parameters.ScenePath, true);

                case "disable":
                    return SetSceneEnabled(scenes, parameters.ScenePath, false);

                case "move":
                    return MoveScene(scenes, parameters.SceneFromIndex, parameters.SceneToIndex);

                default:
                    throw new ArgumentException(
                        $"Unknown SceneAction '{action}'. Valid actions: Add, Remove, Enable, Disable, Move.");
            }
        }

        static string AddScene(List<EditorBuildSettingsScene> scenes, string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
                throw new ArgumentException("ScenePath is required for the Add action.");

            // Validate the scene asset exists
            var sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(scenePath);
            if (sceneAsset == null)
                throw new ArgumentException($"No scene asset found at path: '{scenePath}'.");

            // Check if already in the list
            for (int i = 0; i < scenes.Count; i++)
            {
                if (string.Equals(scenes[i].path, scenePath, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException($"Scene '{scenePath}' is already in the build list at index {i}.");
            }

            var guid = AssetDatabase.GUIDFromAssetPath(scenePath);
            scenes.Add(new EditorBuildSettingsScene(guid, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            return $"sceneList: added '{scenePath}' at index {scenes.Count - 1}";
        }

        static string RemoveScene(List<EditorBuildSettingsScene> scenes, string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
                throw new ArgumentException("ScenePath is required for the Remove action.");

            int idx = FindSceneIndex(scenes, scenePath);
            scenes.RemoveAt(idx);
            EditorBuildSettings.scenes = scenes.ToArray();
            return $"sceneList: removed '{scenePath}' (was at index {idx})";
        }

        static string SetSceneEnabled(List<EditorBuildSettingsScene> scenes, string scenePath, bool enabled)
        {
            if (string.IsNullOrEmpty(scenePath))
                throw new ArgumentException($"ScenePath is required for the {(enabled ? "Enable" : "Disable")} action.");

            int idx = FindSceneIndex(scenes, scenePath);
            var scene = scenes[idx];
            scene.enabled = enabled;
            scenes[idx] = scene;
            EditorBuildSettings.scenes = scenes.ToArray();
            return $"sceneList: {(enabled ? "enabled" : "disabled")} '{scenePath}' at index {idx}";
        }

        static string MoveScene(List<EditorBuildSettingsScene> scenes, int? fromIndex, int? toIndex)
        {
            if (!fromIndex.HasValue || !toIndex.HasValue)
                throw new ArgumentException("SceneFromIndex and SceneToIndex are required for the Move action.");

            int from = fromIndex.Value;
            int to = toIndex.Value;

            if (from < 0 || from >= scenes.Count)
                throw new ArgumentException($"SceneFromIndex {from} is out of range (0-{scenes.Count - 1}).");
            if (to < 0 || to >= scenes.Count)
                throw new ArgumentException($"SceneToIndex {to} is out of range (0-{scenes.Count - 1}).");
            if (from == to)
                return $"sceneList: no move needed — source and destination are both index {from}";

            var scene = scenes[from];
            scenes.RemoveAt(from);
            scenes.Insert(to, scene);
            EditorBuildSettings.scenes = scenes.ToArray();
            return $"sceneList: moved '{scene.path}' from index {from} to index {to}";
        }

        static int FindSceneIndex(List<EditorBuildSettingsScene> scenes, string scenePath)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                if (string.Equals(scenes[i].path, scenePath, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            throw new ArgumentException($"Scene '{scenePath}' not found in the build list.");
        }
    }
}
