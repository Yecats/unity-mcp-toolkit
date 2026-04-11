#if MCP_TOOLKIT_RECORDER
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Recorder;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that starts a recording session using the current Recorder configuration.
    /// This will enter Play mode if not already active and ask the caller to retry.
    /// The Recorder only works in Play mode.
    /// Only available when com.unity.recorder is installed.
    /// </summary>
    public static class StartRecordingTool
    {
        const string ToolName = "McpToolkit.StartRecording";

        const string Description =
            "Starts a recording session using the current Unity Recorder configuration. " +
            "If the Editor is not in Play mode, it will enter Play mode and ask you to call " +
            "StartRecording again once Play mode is fully entered. " +
            "Use GetRecorderSettings to verify the configuration before starting. " +
            "Use StopRecording to end the session. Requires the com.unity.recorder package.";

        const string Title = "Start Recording";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "MCP Toolkit - Recorder" })]
        public static object HandleCommand()
        {
            try
            {
                var controllerSettings = RecorderControllerSettings.GetGlobalSettings();
                if (controllerSettings == null)
                    return Response.Error(
                        "RECORDER_NOT_CONFIGURED: No Recorder settings found. " +
                        "Use SetRecorderSettings to configure recorders first, or open the Recorder window.");

                var recorders = controllerSettings.RecorderSettings.ToList();
                if (recorders.Count == 0)
                    return Response.Error(
                        "NO_RECORDERS: No recorders configured. " +
                        "Use SetRecorderSettings with Action 'AddRecorder' to add a recorder.");

                var enabledRecorders = recorders.Where(r => r != null && r.Enabled).ToList();
                if (enabledRecorders.Count == 0)
                    return Response.Error(
                        "NO_ENABLED_RECORDERS: All recorders are disabled. " +
                        "Use SetRecorderSettings with Action 'EnableRecorder' to enable at least one recorder.");

                var controller = new RecorderController(controllerSettings);

                // The Recorder requires Play mode — auto-enter if not already playing.
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.EnterPlaymode();

                    // We can't call PrepareRecording synchronously after entering play mode
                    // because the domain reload hasn't happened yet. Return a message telling
                    // the caller to retry once Play mode is active.
                    return Response.Success(
                        "Entering Play mode. The Recorder requires Play mode to record. " +
                        "Please call StartRecording again once Play mode is active.",
                        new
                        {
                            recording = false,
                            enteringPlayMode = true,
                            hint = "Call StartRecording again after Play mode is fully entered."
                        });
                }

                controller.PrepareRecording();
                bool started = controller.StartRecording();

                if (!started)
                    return Response.Error(
                        "START_RECORDING_FAILED: The recording could not be started. " +
                        "Check the Unity Console for detailed error messages.");

                var recorderNames = enabledRecorders
                    .Select(r => $"{r.name} ({r.GetType().Name.Replace("Settings", "")})")
                    .ToList();

                return Response.Success(
                    $"Recording started with {enabledRecorders.Count} active recorder(s).",
                    new
                    {
                        recording = true,
                        activeRecorderCount = enabledRecorders.Count,
                        activeRecorders = recorderNames
                    });
            }
            catch (Exception ex)
            {
                return Response.Error($"START_RECORDING_FAILED: {ex.Message}");
            }
        }
    }
}
#endif
