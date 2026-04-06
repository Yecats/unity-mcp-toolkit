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
    /// MCP tool that modifies Unity PlayerSettings.
    /// Only supplied (non-null) parameters are applied — omitted values are left unchanged.
    /// </summary>
    public static class SetPlayerSettingsTool
    {
        const string ToolName = "McpToolkit.SetPlayerSettings";

        const string Description =
            "Modifies Unity PlayerSettings. Only supplied parameters are changed — omitted values " +
            "are left as-is. Supports project identity (company, product, version), rendering " +
            "(color space, screen size, fullscreen mode), scripting configuration (backend, defines, " +
            "API compatibility, IL2CPP config, stripping level), and editor behavior (run in " +
            "background, unsafe code, incremental GC).";

        const string Title = "Modify Player Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "MCP Toolkit - Project Settings" })]
        public static object HandleCommand(SetPlayerSettingsParams parameters)
        {
            if (parameters == null)
                return Response.Error("INVALID_PARAMETER: No parameters provided.");

            try
            {
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                var namedTarget = NamedBuildTarget.FromBuildTargetGroup(
                    BuildPipeline.GetBuildTargetGroup(buildTarget));

                var changes = new List<string>();

                // Project identity
                if (parameters.CompanyName != null)
                {
                    PlayerSettings.companyName = parameters.CompanyName;
                    changes.Add($"companyName = \"{parameters.CompanyName}\"");
                }
                if (parameters.ProductName != null)
                {
                    PlayerSettings.productName = parameters.ProductName;
                    changes.Add($"productName = \"{parameters.ProductName}\"");
                }
                if (parameters.BundleVersion != null)
                {
                    PlayerSettings.bundleVersion = parameters.BundleVersion;
                    changes.Add($"bundleVersion = \"{parameters.BundleVersion}\"");
                }
                if (parameters.ApplicationIdentifier != null)
                {
                    PlayerSettings.applicationIdentifier = parameters.ApplicationIdentifier;
                    changes.Add($"applicationIdentifier = \"{parameters.ApplicationIdentifier}\"");
                }

                // Rendering
                if (parameters.ColorSpace != null)
                {
                    if (Enum.TryParse<ColorSpace>(parameters.ColorSpace, true, out var cs))
                    {
                        PlayerSettings.colorSpace = cs;
                        changes.Add($"colorSpace = {cs}");
                    }
                    else
                        return Response.Error($"INVALID_PARAMETER: Unknown ColorSpace '{parameters.ColorSpace}'. Use 'Gamma' or 'Linear'.");
                }
                if (parameters.DefaultScreenWidth.HasValue)
                {
                    PlayerSettings.defaultScreenWidth = parameters.DefaultScreenWidth.Value;
                    changes.Add($"defaultScreenWidth = {parameters.DefaultScreenWidth.Value}");
                }
                if (parameters.DefaultScreenHeight.HasValue)
                {
                    PlayerSettings.defaultScreenHeight = parameters.DefaultScreenHeight.Value;
                    changes.Add($"defaultScreenHeight = {parameters.DefaultScreenHeight.Value}");
                }
                if (parameters.FullScreenMode != null)
                {
                    if (Enum.TryParse<FullScreenMode>(parameters.FullScreenMode, true, out var fsm))
                    {
                        PlayerSettings.fullScreenMode = fsm;
                        changes.Add($"fullScreenMode = {fsm}");
                    }
                    else
                        return Response.Error($"INVALID_PARAMETER: Unknown FullScreenMode '{parameters.FullScreenMode}'.");
                }
                if (parameters.RunInBackground.HasValue)
                {
                    PlayerSettings.runInBackground = parameters.RunInBackground.Value;
                    changes.Add($"runInBackground = {parameters.RunInBackground.Value}");
                }

                // Scripting
                if (parameters.ScriptingBackend != null)
                {
                    if (Enum.TryParse<ScriptingImplementation>(parameters.ScriptingBackend, true, out var sb))
                    {
                        PlayerSettings.SetScriptingBackend(namedTarget, sb);
                        changes.Add($"scriptingBackend = {sb}");
                    }
                    else
                        return Response.Error($"INVALID_PARAMETER: Unknown ScriptingBackend '{parameters.ScriptingBackend}'. Use 'Mono2x' or 'IL2CPP'.");
                }
                if (parameters.ApiCompatibilityLevel != null)
                {
                    if (Enum.TryParse<ApiCompatibilityLevel>(parameters.ApiCompatibilityLevel, true, out var acl))
                    {
                        PlayerSettings.SetApiCompatibilityLevel(namedTarget, acl);
                        changes.Add($"apiCompatibilityLevel = {acl}");
                    }
                    else
                        return Response.Error($"INVALID_PARAMETER: Unknown ApiCompatibilityLevel '{parameters.ApiCompatibilityLevel}'.");
                }
                if (parameters.Il2CppCompilerConfiguration != null)
                {
                    if (Enum.TryParse<Il2CppCompilerConfiguration>(parameters.Il2CppCompilerConfiguration, true, out var il2cpp))
                    {
                        PlayerSettings.SetIl2CppCompilerConfiguration(namedTarget, il2cpp);
                        changes.Add($"il2CppCompilerConfiguration = {il2cpp}");
                    }
                    else
                        return Response.Error($"INVALID_PARAMETER: Unknown Il2CppCompilerConfiguration '{parameters.Il2CppCompilerConfiguration}'.");
                }
                if (parameters.ManagedStrippingLevel != null)
                {
                    if (Enum.TryParse<ManagedStrippingLevel>(parameters.ManagedStrippingLevel, true, out var msl))
                    {
                        PlayerSettings.SetManagedStrippingLevel(namedTarget, msl);
                        changes.Add($"managedStrippingLevel = {msl}");
                    }
                    else
                        return Response.Error($"INVALID_PARAMETER: Unknown ManagedStrippingLevel '{parameters.ManagedStrippingLevel}'.");
                }
                if (parameters.ScriptingDefineSymbols != null)
                {
                    PlayerSettings.SetScriptingDefineSymbols(namedTarget, parameters.ScriptingDefineSymbols);
                    changes.Add($"scriptingDefineSymbols = \"{parameters.ScriptingDefineSymbols}\"");
                }
                if (parameters.AllowUnsafeCode.HasValue)
                {
                    PlayerSettings.allowUnsafeCode = parameters.AllowUnsafeCode.Value;
                    changes.Add($"allowUnsafeCode = {parameters.AllowUnsafeCode.Value}");
                }
                if (parameters.GcIncremental.HasValue)
                {
                    PlayerSettings.gcIncremental = parameters.GcIncremental.Value;
                    changes.Add($"gcIncremental = {parameters.GcIncremental.Value}");
                }

                if (changes.Count == 0)
                    return Response.Success("No changes applied — all parameters were null/omitted.", new { changesApplied = 0 });

                return Response.Success(
                    $"PlayerSettings updated: {changes.Count} value(s) changed.",
                    new { changesApplied = changes.Count, changes });
            }
            catch (Exception ex)
            {
                return Response.Error($"SET_PLAYER_SETTINGS_FAILED: {ex.Message}");
            }
        }
    }
}
