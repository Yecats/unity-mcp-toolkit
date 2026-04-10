using System;
using UnityEngine;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that reads Unity Time and Audio settings.
    /// Covers fixed timestep, time scale, and audio system configuration.
    /// </summary>
    public static class GetTimeAndAudioSettingsTool
    {
        const string ToolName = "McpToolkit.GetTimeAndAudioSettings";

        const string Description =
            "Reads Unity Time settings (fixed timestep, maximum delta time, time scale, " +
            "maximum particle timestep) and Audio settings (sample rate, speaker mode, " +
            "DSP buffer size, real and virtual voice counts).";

        const string Title = "Read Time and Audio Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true,
            Groups = new[] { "MCP Toolkit - Project Settings" })]
        public static object HandleCommand()
        {
            try
            {
                var audioConfig = AudioSettings.GetConfiguration();

                var data = new
                {
                    time = new
                    {
                        fixedDeltaTime = Time.fixedDeltaTime,
                        maximumDeltaTime = Time.maximumDeltaTime,
                        timeScale = Time.timeScale,
                        maximumParticleDeltaTime = Time.maximumParticleDeltaTime,
                        captureDeltaTime = Time.captureDeltaTime
                    },
                    audio = new
                    {
                        sampleRate = AudioSettings.outputSampleRate,
                        speakerMode = AudioSettings.speakerMode.ToString(),
                        dspBufferSize = audioConfig.dspBufferSize,
                        numRealVoices = audioConfig.numRealVoices,
                        numVirtualVoices = audioConfig.numVirtualVoices
                    }
                };

                return Response.Success("Time and Audio settings retrieved.", data);
            }
            catch (Exception ex)
            {
                return Response.Error($"GET_TIME_AUDIO_SETTINGS_FAILED: {ex.Message}");
            }
        }
    }
}
