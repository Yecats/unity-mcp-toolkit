using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that reads Unity PlayerSettings and returns them as structured data.
    /// Covers project identity, rendering, scripting backend, and platform configuration.
    /// </summary>
    public static class GetPlayerSettingsTool
    {
        const string ToolName = "McpToolkit.GetPlayerSettings";

        const string Description =
            "Reads Unity PlayerSettings including project identity (company, product, version), " +
            "rendering (color space, graphics APIs), scripting configuration (backend, defines, API " +
            "compatibility, IL2CPP config, stripping level), and editor behavior (run in background, " +
            "unsafe code, incremental GC). Returns structured data for the active build target.";

        const string Title = "Read Player Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true,
            Groups = new[] { "MCP Toolkit - Project Settings" })]
        public static object HandleCommand()
        {
            try
            {
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                var namedTarget = NamedBuildTarget.FromBuildTargetGroup(
                    BuildPipeline.GetBuildTargetGroup(buildTarget));

                // Graphics APIs for the active platform
                var graphicsApis = PlayerSettings.GetGraphicsAPIs(buildTarget);
                var graphicsApiNames = new string[graphicsApis.Length];
                for (int i = 0; i < graphicsApis.Length; i++)
                    graphicsApiNames[i] = graphicsApis[i].ToString();

                var data = new
                {
                    // Project identity
                    companyName = PlayerSettings.companyName,
                    productName = PlayerSettings.productName,
                    bundleVersion = PlayerSettings.bundleVersion,
                    applicationIdentifier = PlayerSettings.applicationIdentifier,

                    // Rendering
                    colorSpace = PlayerSettings.colorSpace.ToString(),
                    graphicsAPIs = graphicsApiNames,
                    autoGraphicsAPI = PlayerSettings.GetUseDefaultGraphicsAPIs(buildTarget),

                    // Resolution
                    defaultScreenWidth = PlayerSettings.defaultScreenWidth,
                    defaultScreenHeight = PlayerSettings.defaultScreenHeight,
                    fullScreenMode = PlayerSettings.fullScreenMode.ToString(),
                    runInBackground = PlayerSettings.runInBackground,

                    // Scripting
                    scriptingBackend = PlayerSettings.GetScriptingBackend(namedTarget).ToString(),
                    apiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(namedTarget).ToString(),
                    il2CppCompilerConfiguration = PlayerSettings.GetIl2CppCompilerConfiguration(namedTarget).ToString(),
                    managedStrippingLevel = PlayerSettings.GetManagedStrippingLevel(namedTarget).ToString(),
                    scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbols(namedTarget),
                    allowUnsafeCode = PlayerSettings.allowUnsafeCode,
                    gcIncremental = PlayerSettings.gcIncremental,

                    // Platform context
                    activeBuildTarget = buildTarget.ToString(),
                    buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget).ToString()
                };

                return Response.Success(
                    $"PlayerSettings retrieved for active build target: {buildTarget}.",
                    data);
            }
            catch (Exception ex)
            {
                return Response.Error($"GET_PLAYER_SETTINGS_FAILED: {ex.Message}");
            }
        }
    }
}
