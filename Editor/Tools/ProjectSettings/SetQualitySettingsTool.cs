using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that modifies Unity QualitySettings for a specific quality level.
    /// Only supplied (non-null) parameters are applied — omitted values are left unchanged.
    /// </summary>
    public static class SetQualitySettingsTool
    {
        const string ToolName = "McpToolkit.SetQualitySettings";

        const string Description =
            "Modifies Unity QualitySettings for a specific quality level. If no level index is " +
            "provided, modifies the active level. Only supplied parameters are changed. Supports " +
            "shadows, anti-aliasing, VSync, LOD bias, anisotropic filtering, streaming mipmaps, " +
            "and switching the active quality level.";

        const string Title = "Modify Quality Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "MCP Toolkit - Project Settings" })]
        public static object HandleCommand(SetQualitySettingsParams parameters)
        {
            if (parameters == null)
                return Response.Error("INVALID_PARAMETER: No parameters provided.");

            try
            {
                string[] levelNames = QualitySettings.names;
                int originalLevel = QualitySettings.GetQualityLevel();
                var changes = new List<string>();

                // Determine which level to modify
                int targetLevel = parameters.QualityLevelIndex ?? originalLevel;
                if (targetLevel < 0 || targetLevel >= levelNames.Length)
                    return Response.Error($"INVALID_PARAMETER: QualityLevelIndex {targetLevel} is out of range (0-{levelNames.Length - 1}).");

                // Switch to target level to modify it
                QualitySettings.SetQualityLevel(targetLevel, false);

                if (parameters.Shadows != null)
                {
                    if (Enum.TryParse<ShadowQuality>(parameters.Shadows, true, out var sq))
                    {
                        QualitySettings.shadows = sq;
                        changes.Add($"shadows = {sq}");
                    }
                    else
                        return RestoreAndError(originalLevel, $"Unknown Shadows '{parameters.Shadows}'. Use 'Disable', 'HardOnly', or 'All'.");
                }
                if (parameters.ShadowResolution != null)
                {
                    if (Enum.TryParse<ShadowResolution>(parameters.ShadowResolution, true, out var sr))
                    {
                        QualitySettings.shadowResolution = sr;
                        changes.Add($"shadowResolution = {sr}");
                    }
                    else
                        return RestoreAndError(originalLevel, $"Unknown ShadowResolution '{parameters.ShadowResolution}'.");
                }
                if (parameters.ShadowDistance.HasValue)
                {
                    QualitySettings.shadowDistance = parameters.ShadowDistance.Value;
                    changes.Add($"shadowDistance = {parameters.ShadowDistance.Value}");
                }
                if (parameters.ShadowCascades.HasValue)
                {
                    int sc = parameters.ShadowCascades.Value;
                    if (sc != 1 && sc != 2 && sc != 4)
                        return RestoreAndError(originalLevel, $"ShadowCascades must be 1, 2, or 4. Got {sc}.");
                    QualitySettings.shadowCascades = sc;
                    changes.Add($"shadowCascades = {sc}");
                }
                if (parameters.PixelLightCount.HasValue)
                {
                    QualitySettings.pixelLightCount = parameters.PixelLightCount.Value;
                    changes.Add($"pixelLightCount = {parameters.PixelLightCount.Value}");
                }
                if (parameters.AntiAliasing.HasValue)
                {
                    int aa = parameters.AntiAliasing.Value;
                    if (aa != 0 && aa != 2 && aa != 4 && aa != 8)
                        return RestoreAndError(originalLevel, $"AntiAliasing must be 0, 2, 4, or 8. Got {aa}.");
                    QualitySettings.antiAliasing = aa;
                    changes.Add($"antiAliasing = {aa}");
                }
                if (parameters.SoftParticles.HasValue)
                {
                    QualitySettings.softParticles = parameters.SoftParticles.Value;
                    changes.Add($"softParticles = {parameters.SoftParticles.Value}");
                }
                if (parameters.AnisotropicFiltering != null)
                {
                    if (Enum.TryParse<AnisotropicFiltering>(parameters.AnisotropicFiltering, true, out var af))
                    {
                        QualitySettings.anisotropicFiltering = af;
                        changes.Add($"anisotropicFiltering = {af}");
                    }
                    else
                        return RestoreAndError(originalLevel, $"Unknown AnisotropicFiltering '{parameters.AnisotropicFiltering}'.");
                }
                if (parameters.LodBias.HasValue)
                {
                    QualitySettings.lodBias = parameters.LodBias.Value;
                    changes.Add($"lodBias = {parameters.LodBias.Value}");
                }
                if (parameters.MaximumLODLevel.HasValue)
                {
                    QualitySettings.maximumLODLevel = parameters.MaximumLODLevel.Value;
                    changes.Add($"maximumLODLevel = {parameters.MaximumLODLevel.Value}");
                }
                if (parameters.VSyncCount.HasValue)
                {
                    int vs = parameters.VSyncCount.Value;
                    if (vs < 0 || vs > 4)
                        return RestoreAndError(originalLevel, $"VSyncCount must be 0-4. Got {vs}.");
                    QualitySettings.vSyncCount = vs;
                    changes.Add($"vSyncCount = {vs}");
                }
                if (parameters.StreamingMipmapsActive.HasValue)
                {
                    QualitySettings.streamingMipmapsActive = parameters.StreamingMipmapsActive.Value;
                    changes.Add($"streamingMipmapsActive = {parameters.StreamingMipmapsActive.Value}");
                }
                if (parameters.StreamingMipmapsMemoryBudget.HasValue)
                {
                    QualitySettings.streamingMipmapsMemoryBudget = parameters.StreamingMipmapsMemoryBudget.Value;
                    changes.Add($"streamingMipmapsMemoryBudget = {parameters.StreamingMipmapsMemoryBudget.Value}");
                }

                // Handle active level switch
                if (parameters.SetActiveLevel.HasValue)
                {
                    int newActive = parameters.SetActiveLevel.Value;
                    if (newActive < 0 || newActive >= levelNames.Length)
                        return RestoreAndError(originalLevel, $"SetActiveLevel {newActive} is out of range (0-{levelNames.Length - 1}).");
                    QualitySettings.SetQualityLevel(newActive, true);
                    changes.Add($"activeLevel = {newActive} (\"{levelNames[newActive]}\")");
                }
                else
                {
                    // Restore original active level if we only modified a non-active level
                    QualitySettings.SetQualityLevel(originalLevel, false);
                }

                if (changes.Count == 0)
                    return Response.Success("No changes applied — all parameters were null/omitted.", new { changesApplied = 0 });

                return Response.Success(
                    $"QualitySettings updated for level \"{levelNames[targetLevel]}\": {changes.Count} value(s) changed.",
                    new { changesApplied = changes.Count, targetLevel, targetLevelName = levelNames[targetLevel], changes });
            }
            catch (Exception ex)
            {
                return Response.Error($"SET_QUALITY_SETTINGS_FAILED: {ex.Message}");
            }
        }

        static object RestoreAndError(int originalLevel, string message)
        {
            QualitySettings.SetQualityLevel(originalLevel, false);
            return Response.Error($"INVALID_PARAMETER: {message}");
        }
    }
}
