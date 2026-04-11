#if MCP_TOOLKIT_RECORDER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that reads the current Unity Recorder configuration including session settings,
    /// all configured recorders and their properties, and the current recording state.
    /// Only available when com.unity.recorder is installed.
    /// </summary>
    public static class GetRecorderSettingsTool
    {
        const string ToolName = "McpToolkit.GetRecorderSettings";

        const string Description =
            "Reads the Unity Recorder configuration. Returns session-level settings (frame rate, " +
            "record mode, frame/time interval), a list of all configured recorders with their " +
            "type-specific properties (Movie, Image Sequence, Audio, Animation Clip), and the " +
            "current recording state. Requires the com.unity.recorder package.";

        const string Title = "Read Recorder Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true,
            Groups = new[] { "MCP Toolkit - Recorder" })]
        public static object HandleCommand()
        {
            try
            {
                var controllerSettings = RecorderControllerSettings.GetGlobalSettings();
                if (controllerSettings == null)
                    return Response.Success("No Recorder settings found. The Recorder window may not have been opened yet.",
                        new { sessionConfigured = false });

                var controller = new RecorderController(controllerSettings);
                var isRecording = controller.IsRecording();

                // Build session-level settings
                var session = new
                {
                    frameRate = controllerSettings.FrameRate,
                    frameRatePlayback = controllerSettings.FrameRatePlayback.ToString(),
                    capFrameRate = controllerSettings.CapFrameRate,
                    exitPlayMode = controllerSettings.ExitPlayMode
                };

                // Build recorder list
                var recorderList = new List<object>();
                var recorderSettings = controllerSettings.RecorderSettings.ToList();
                for (int i = 0; i < recorderSettings.Count; i++)
                {
                    var rs = recorderSettings[i];
                    if (rs == null) continue;

                    recorderList.Add(BuildRecorderInfo(rs, i));
                }

                return Response.Success(
                    $"Recorder settings retrieved. {recorderList.Count} recorder(s) configured. Recording active: {isRecording}.",
                    new
                    {
                        sessionConfigured = true,
                        isRecording,
                        session,
                        recorderCount = recorderList.Count,
                        recorders = recorderList
                    });
            }
            catch (Exception ex)
            {
                return Response.Error($"GET_RECORDER_SETTINGS_FAILED: {ex.Message}");
            }
        }

        static object BuildRecorderInfo(RecorderSettings rs, int index)
        {
            var baseInfo = new Dictionary<string, object>
            {
                ["index"] = index,
                ["name"] = rs.name,
                ["enabled"] = rs.Enabled,
                ["recordMode"] = rs.RecordMode.ToString(),
                ["frameRate"] = rs.FrameRate,
                ["frameRatePlayback"] = rs.FrameRatePlayback.ToString(),
                ["startFrame"] = rs.StartFrame,
                ["endFrame"] = rs.EndFrame,
                ["startTime"] = rs.StartTime,
                ["endTime"] = rs.EndTime,
                ["outputFile"] = rs.OutputFile,
                ["take"] = rs.Take,
                ["capFrameRate"] = rs.CapFrameRate,
                ["platformSupported"] = rs.IsPlatformSupported
            };

            switch (rs)
            {
                case MovieRecorderSettings movie:
                    baseInfo["recorderType"] = "Movie";
                    baseInfo["captureAudio"] = movie.CaptureAudio;
                    baseInfo["captureAlpha"] = movie.CaptureAlpha;
                    if (movie.ImageInputSettings != null)
                    {
                        baseInfo["outputWidth"] = movie.ImageInputSettings.OutputWidth;
                        baseInfo["outputHeight"] = movie.ImageInputSettings.OutputHeight;
                        if (movie.ImageInputSettings is CameraInputSettings camInput)
                            baseInfo["imageSource"] = camInput.Source.ToString();
                    }
                    break;

                case ImageRecorderSettings image:
                    baseInfo["recorderType"] = "ImageSequence";
                    baseInfo["outputFormat"] = image.OutputFormat.ToString();
                    baseInfo["captureAlpha"] = image.CaptureAlpha;
                    baseInfo["outputColorSpace"] = image.OutputColorSpace.ToString();
                    if (image.OutputFormat == ImageRecorderSettings.ImageRecorderOutputFormat.JPEG)
                        baseInfo["jpegQuality"] = image.JpegQuality;
                    if (image.OutputFormat == ImageRecorderSettings.ImageRecorderOutputFormat.EXR)
                        baseInfo["exrCompression"] = image.EXRCompression.ToString();
                    if (image.imageInputSettings != null)
                    {
                        baseInfo["outputWidth"] = image.imageInputSettings.OutputWidth;
                        baseInfo["outputHeight"] = image.imageInputSettings.OutputHeight;
                        if (image.imageInputSettings is CameraInputSettings camInput)
                            baseInfo["imageSource"] = camInput.Source.ToString();
                    }
                    break;

                case AudioRecorderSettings _:
                    baseInfo["recorderType"] = "Audio";
                    break;

                case AnimationRecorderSettings animation:
                    baseInfo["recorderType"] = "AnimationClip";
                    if (animation.AnimationInputSettings != null)
                    {
                        baseInfo["gameObjectTarget"] = animation.AnimationInputSettings.gameObject != null
                            ? animation.AnimationInputSettings.gameObject.name
                            : "(none)";
                        baseInfo["recursive"] = animation.AnimationInputSettings.Recursive;
                    }
                    break;

                default:
                    baseInfo["recorderType"] = rs.GetType().Name;
                    break;
            }

            return baseInfo;
        }
    }
}
#endif
