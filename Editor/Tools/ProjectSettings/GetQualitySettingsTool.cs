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
    /// MCP tool that reads Unity QualitySettings and returns all quality levels
    /// with their rendering properties.
    /// </summary>
    public static class GetQualitySettingsTool
    {
        const string ToolName = "McpToolkit.GetQualitySettings";

        const string Description =
            "Reads all Unity quality levels and their rendering properties including shadows, " +
            "lighting, anti-aliasing, VSync, LOD bias, anisotropic filtering, texture quality, " +
            "and render pipeline asset. Returns the active quality level and per-level settings.";

        const string Title = "Read Quality Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true,
            Groups = new[] { "MCP Toolkit - Project Settings" })]
        public static object HandleCommand()
        {
            try
            {
                int activeLevel = QualitySettings.GetQualityLevel();
                string[] levelNames = QualitySettings.names;
                var levels = new List<object>();

                // Read properties for each quality level
                for (int i = 0; i < levelNames.Length; i++)
                {
                    QualitySettings.SetQualityLevel(i, false);

                    var renderPipelineAsset = QualitySettings.renderPipeline;
                    string rpName = renderPipelineAsset != null ? renderPipelineAsset.name : "Built-in";

                    levels.Add(new
                    {
                        index = i,
                        name = levelNames[i],
                        isActive = i == activeLevel,

                        // Shadows
                        shadows = QualitySettings.shadows.ToString(),
                        shadowResolution = QualitySettings.shadowResolution.ToString(),
                        shadowDistance = QualitySettings.shadowDistance,
                        shadowCascades = QualitySettings.shadowCascades,

                        // Lighting & rendering
                        pixelLightCount = QualitySettings.pixelLightCount,
                        antiAliasing = QualitySettings.antiAliasing,
                        softParticles = QualitySettings.softParticles,
                        renderPipeline = rpName,

                        // Textures & detail
                        anisotropicFiltering = QualitySettings.anisotropicFiltering.ToString(),
                        lodBias = QualitySettings.lodBias,
                        maximumLODLevel = QualitySettings.maximumLODLevel,
                        skinWeights = QualitySettings.skinWeights.ToString(),

                        // Performance
                        vSyncCount = QualitySettings.vSyncCount,

                        // Streaming
                        streamingMipmapsActive = QualitySettings.streamingMipmapsActive,
                        streamingMipmapsMemoryBudget = QualitySettings.streamingMipmapsMemoryBudget
                    });
                }

                // Restore original quality level
                QualitySettings.SetQualityLevel(activeLevel, false);

                var data = new
                {
                    activeQualityLevel = activeLevel,
                    activeQualityLevelName = levelNames[activeLevel],
                    totalLevels = levelNames.Length,
                    levels
                };

                return Response.Success(
                    $"QualitySettings retrieved: {levelNames.Length} levels, active = \"{levelNames[activeLevel]}\".",
                    data);
            }
            catch (Exception ex)
            {
                return Response.Error($"GET_QUALITY_SETTINGS_FAILED: {ex.Message}");
            }
        }
    }
}
