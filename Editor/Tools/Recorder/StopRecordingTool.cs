#if MCP_TOOLKIT_RECORDER
using System;
using UnityEditor.Recorder;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that stops any active recording session.
    /// Only available when com.unity.recorder is installed.
    /// </summary>
    public static class StopRecordingTool
    {
        const string ToolName = "McpToolkit.StopRecording";

        const string Description =
            "Stops any active Unity Recorder recording session. Most recorders will write " +
            "their output files once stopped. If no recording is active, returns a success " +
            "message indicating nothing was stopped. Requires the com.unity.recorder package.";

        const string Title = "Stop Recording";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "MCP Toolkit - Recorder" })]
        public static object HandleCommand()
        {
            try
            {
                var controllerSettings = RecorderControllerSettings.GetGlobalSettings();
                if (controllerSettings == null)
                    return Response.Success("No Recorder settings found. Nothing to stop.",
                        new { wasStopped = false });

                var controller = new RecorderController(controllerSettings);
                bool wasRecording = controller.IsRecording();
                controller.StopRecording();

                if (wasRecording)
                    return Response.Success(
                        "Recording stopped. Output files have been written.",
                        new { wasStopped = true });

                return Response.Success(
                    "No active recording was found. Nothing was stopped.",
                    new { wasStopped = false });
            }
            catch (Exception ex)
            {
                return Response.Error($"STOP_RECORDING_FAILED: {ex.Message}");
            }
        }
    }
}
#endif
