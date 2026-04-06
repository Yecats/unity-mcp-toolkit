using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that modifies Unity Time and Audio settings.
    /// Only supplied (non-null) parameters are applied — omitted values are left unchanged.
    /// </summary>
    public static class SetTimeAndAudioSettingsTool
    {
        const string ToolName = "McpToolkit.SetTimeAndAudioSettings";

        const string Description =
            "Modifies Unity Time and Audio settings. Only supplied parameters are changed. " +
            "Time: fixed timestep, maximum delta time, time scale, maximum particle delta time, " +
            "capture delta time. Audio: sample rate, speaker mode, DSP buffer size, real and " +
            "virtual voice counts.";

        const string Title = "Modify Time and Audio Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "MCP Toolkit - Project Settings" })]
        public static object HandleCommand(SetTimeAndAudioSettingsParams parameters)
        {
            if (parameters == null)
                return Response.Error("INVALID_PARAMETER: No parameters provided.");

            try
            {
                var changes = new List<string>();

                // --- Time ---
                if (parameters.FixedDeltaTime.HasValue)
                {
                    if (parameters.FixedDeltaTime.Value <= 0)
                        return Response.Error("INVALID_PARAMETER: FixedDeltaTime must be positive.");
                    Time.fixedDeltaTime = parameters.FixedDeltaTime.Value;
                    changes.Add($"fixedDeltaTime = {parameters.FixedDeltaTime.Value}");
                }
                if (parameters.MaximumDeltaTime.HasValue)
                {
                    if (parameters.MaximumDeltaTime.Value <= 0)
                        return Response.Error("INVALID_PARAMETER: MaximumDeltaTime must be positive.");
                    Time.maximumDeltaTime = parameters.MaximumDeltaTime.Value;
                    changes.Add($"maximumDeltaTime = {parameters.MaximumDeltaTime.Value}");
                }
                if (parameters.TimeScale.HasValue)
                {
                    if (parameters.TimeScale.Value < 0)
                        return Response.Error("INVALID_PARAMETER: TimeScale cannot be negative.");
                    Time.timeScale = parameters.TimeScale.Value;
                    changes.Add($"timeScale = {parameters.TimeScale.Value}");
                }
                if (parameters.MaximumParticleDeltaTime.HasValue)
                {
                    if (parameters.MaximumParticleDeltaTime.Value <= 0)
                        return Response.Error("INVALID_PARAMETER: MaximumParticleDeltaTime must be positive.");
                    Time.maximumParticleDeltaTime = parameters.MaximumParticleDeltaTime.Value;
                    changes.Add($"maximumParticleDeltaTime = {parameters.MaximumParticleDeltaTime.Value}");
                }
                if (parameters.CaptureDeltaTime.HasValue)
                {
                    if (parameters.CaptureDeltaTime.Value < 0)
                        return Response.Error("INVALID_PARAMETER: CaptureDeltaTime cannot be negative.");
                    Time.captureDeltaTime = parameters.CaptureDeltaTime.Value;
                    changes.Add($"captureDeltaTime = {parameters.CaptureDeltaTime.Value}");
                }

                // --- Audio ---
                bool audioChanged = false;
                var audioConfig = AudioSettings.GetConfiguration();

                if (parameters.SpeakerMode != null)
                {
                    if (Enum.TryParse<AudioSpeakerMode>(parameters.SpeakerMode, true, out var sm))
                    {
                        audioConfig.speakerMode = sm;
                        audioChanged = true;
                        changes.Add($"speakerMode = {sm}");
                    }
                    else
                        return Response.Error($"INVALID_PARAMETER: Unknown SpeakerMode '{parameters.SpeakerMode}'.");
                }
                if (parameters.DspBufferSize.HasValue)
                {
                    audioConfig.dspBufferSize = parameters.DspBufferSize.Value;
                    audioChanged = true;
                    changes.Add($"dspBufferSize = {parameters.DspBufferSize.Value}");
                }
                if (parameters.NumRealVoices.HasValue)
                {
                    audioConfig.numRealVoices = parameters.NumRealVoices.Value;
                    audioChanged = true;
                    changes.Add($"numRealVoices = {parameters.NumRealVoices.Value}");
                }
                if (parameters.NumVirtualVoices.HasValue)
                {
                    audioConfig.numVirtualVoices = parameters.NumVirtualVoices.Value;
                    audioChanged = true;
                    changes.Add($"numVirtualVoices = {parameters.NumVirtualVoices.Value}");
                }
                if (parameters.SampleRate.HasValue)
                {
                    audioConfig.sampleRate = parameters.SampleRate.Value;
                    audioChanged = true;
                    changes.Add($"sampleRate = {parameters.SampleRate.Value}");
                }

                // Apply audio config changes in a single call
                if (audioChanged)
                {
                    if (!AudioSettings.Reset(audioConfig))
                        return Response.Error("AUDIO_RESET_FAILED: AudioSettings.Reset returned false. Check that audio values are valid.");
                }

                if (changes.Count == 0)
                    return Response.Success("No changes applied — all parameters were null/omitted.", new { changesApplied = 0 });

                return Response.Success(
                    $"Time and Audio settings updated: {changes.Count} value(s) changed.",
                    new { changesApplied = changes.Count, changes });
            }
            catch (Exception ex)
            {
                return Response.Error($"SET_TIME_AUDIO_SETTINGS_FAILED: {ex.Message}");
            }
        }
    }
}
