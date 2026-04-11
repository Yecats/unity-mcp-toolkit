#if MCP_TOOLKIT_RECORDER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Recorder;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that modifies Unity Recorder configuration. Supports changing session-level
    /// settings (frame rate, record mode) and managing the recorder list (add, remove,
    /// enable, disable, modify recorders). Only available when com.unity.recorder is installed.
    /// </summary>
    public static class SetRecorderSettingsTool
    {
        const string ToolName = "McpToolkit.SetRecorderSettings";

        const string Description =
            "Modifies the Unity Recorder configuration. Only supplied parameters are changed. " +
            "Supports session-level settings: FrameRate, FrameRatePlayback ('Constant'/'Variable'), " +
            "CapFrameRate, ExitPlayMode, RecordMode ('Manual'/'SingleFrame'/'FrameInterval'/'TimeInterval'), " +
            "and frame/time intervals. Manage the recorder list with Action: 'AddRecorder' (requires " +
            "RecorderType: 'Movie'/'ImageSequence'/'Audio'/'AnimationClip'), 'RemoveRecorder', " +
            "'EnableRecorder', 'DisableRecorder', or 'ModifyRecorder' (requires RecorderIndex). " +
            "Set recorder-level properties like OutputFile, CaptureAudio, CaptureAlpha, " +
            "ImageOutputFormat ('PNG'/'JPEG'/'EXR'), JpegQuality, ImageColorSpace. " +
            "Requires the com.unity.recorder package.";

        const string Title = "Modify Recorder Settings";

        [McpTool(ToolName, Description, Title, EnabledByDefault = false,
            Groups = new[] { "MCP Toolkit - Recorder" })]
        public static object HandleCommand(SetRecorderSettingsParams parameters)
        {
            if (parameters == null)
                return Response.Error("INVALID_PARAMETER: No parameters provided.");

            try
            {
                var controllerSettings = RecorderControllerSettings.GetGlobalSettings();
                if (controllerSettings == null)
                    return Response.Error("RECORDER_NOT_CONFIGURED: No Recorder settings found. Open the Recorder window first.");

                var changes = new List<string>();

                // ── Session-level settings ──────────────────────────────

                if (parameters.FrameRate.HasValue)
                {
                    controllerSettings.FrameRate = parameters.FrameRate.Value;
                    changes.Add($"frameRate = {parameters.FrameRate.Value}");
                }

                if (parameters.FrameRatePlayback != null)
                {
                    if (Enum.TryParse<FrameRatePlayback>(parameters.FrameRatePlayback, true, out var frp))
                    {
                        controllerSettings.FrameRatePlayback = frp;
                        changes.Add($"frameRatePlayback = {frp}");
                    }
                    else
                        return Response.Error(
                            $"INVALID_PARAMETER: Unknown FrameRatePlayback '{parameters.FrameRatePlayback}'. " +
                            "Use 'Constant' or 'Variable'.");
                }

                if (parameters.CapFrameRate.HasValue)
                {
                    controllerSettings.CapFrameRate = parameters.CapFrameRate.Value;
                    changes.Add($"capFrameRate = {parameters.CapFrameRate.Value}");
                }

                if (parameters.ExitPlayMode.HasValue)
                {
                    controllerSettings.ExitPlayMode = parameters.ExitPlayMode.Value;
                    changes.Add($"exitPlayMode = {parameters.ExitPlayMode.Value}");
                }

                // ── Record mode ─────────────────────────────────────────

                if (parameters.RecordMode != null)
                {
                    switch (parameters.RecordMode.ToLowerInvariant())
                    {
                        case "manual":
                            controllerSettings.SetRecordModeToManual();
                            changes.Add("recordMode = Manual");
                            break;
                        case "singleframe":
                            int frameNum = parameters.SingleFrameNumber ?? 0;
                            controllerSettings.SetRecordModeToSingleFrame(frameNum);
                            changes.Add($"recordMode = SingleFrame (frame {frameNum})");
                            break;
                        case "frameinterval":
                            int startFrame = parameters.StartFrame ?? 0;
                            int endFrame = parameters.EndFrame ?? 100;
                            controllerSettings.SetRecordModeToFrameInterval(startFrame, endFrame);
                            changes.Add($"recordMode = FrameInterval ({startFrame}-{endFrame})");
                            break;
                        case "timeinterval":
                            float startTime = parameters.StartTime ?? 0f;
                            float endTime = parameters.EndTime ?? 10f;
                            controllerSettings.SetRecordModeToTimeInterval(startTime, endTime);
                            changes.Add($"recordMode = TimeInterval ({startTime}s-{endTime}s)");
                            break;
                        default:
                            return Response.Error(
                                $"INVALID_PARAMETER: Unknown RecordMode '{parameters.RecordMode}'. " +
                                "Use 'Manual', 'SingleFrame', 'FrameInterval', or 'TimeInterval'.");
                    }
                }

                // ── Recorder list action ────────────────────────────────

                if (parameters.Action != null)
                {
                    var recorderSettingsList = controllerSettings.RecorderSettings.ToList();

                    switch (parameters.Action.ToLowerInvariant())
                    {
                        case "addrecorder":
                            return HandleAddRecorder(controllerSettings, parameters, changes);

                        case "removerecorder":
                            if (!parameters.RecorderIndex.HasValue)
                                return Response.Error("INVALID_PARAMETER: 'RemoveRecorder' action requires RecorderIndex.");
                            return HandleRemoveRecorder(controllerSettings, recorderSettingsList, parameters.RecorderIndex.Value, changes);

                        case "enablerecorder":
                            if (!parameters.RecorderIndex.HasValue)
                                return Response.Error("INVALID_PARAMETER: 'EnableRecorder' action requires RecorderIndex.");
                            return HandleToggleRecorder(controllerSettings, recorderSettingsList, parameters.RecorderIndex.Value, true, changes);

                        case "disablerecorder":
                            if (!parameters.RecorderIndex.HasValue)
                                return Response.Error("INVALID_PARAMETER: 'DisableRecorder' action requires RecorderIndex.");
                            return HandleToggleRecorder(controllerSettings, recorderSettingsList, parameters.RecorderIndex.Value, false, changes);

                        case "modifyrecorder":
                            if (!parameters.RecorderIndex.HasValue)
                                return Response.Error("INVALID_PARAMETER: 'ModifyRecorder' action requires RecorderIndex.");
                            return HandleModifyRecorder(controllerSettings, recorderSettingsList, parameters.RecorderIndex.Value, parameters, changes);

                        default:
                            return Response.Error(
                                $"INVALID_PARAMETER: Unknown Action '{parameters.Action}'. " +
                                "Use 'AddRecorder', 'RemoveRecorder', 'EnableRecorder', 'DisableRecorder', or 'ModifyRecorder'.");
                    }
                }

                // ── No recorder action — just session changes ───────────

                if (changes.Count == 0)
                    return Response.Success("No changes applied — all parameters were null/omitted.",
                        new { changesApplied = 0 });

                controllerSettings.Save();
                return Response.Success(
                    $"Recorder settings updated: {changes.Count} value(s) changed.",
                    new { changesApplied = changes.Count, changes });
            }
            catch (Exception ex)
            {
                return Response.Error($"SET_RECORDER_SETTINGS_FAILED: {ex.Message}");
            }
        }

        static object HandleAddRecorder(
            RecorderControllerSettings controllerSettings,
            SetRecorderSettingsParams parameters,
            List<string> changes)
        {
            if (string.IsNullOrEmpty(parameters.RecorderType))
                return Response.Error("INVALID_PARAMETER: 'AddRecorder' action requires RecorderType.");

            RecorderSettings newRecorder;
            switch (parameters.RecorderType.ToLowerInvariant())
            {
                case "movie":
                    var movie = new MovieRecorderSettings { name = "Movie Recorder" };
                    if (parameters.CaptureAudio.HasValue)
                        movie.CaptureAudio = parameters.CaptureAudio.Value;
                    if (parameters.CaptureAlpha.HasValue)
                        movie.CaptureAlpha = parameters.CaptureAlpha.Value;
                    newRecorder = movie;
                    break;

                case "imagesequence":
                    var image = new ImageRecorderSettings { name = "Image Sequence Recorder" };
                    ApplyImageSettings(image, parameters);
                    newRecorder = image;
                    break;

                case "audio":
                    newRecorder = new AudioRecorderSettings { name = "Audio Recorder" };
                    break;

                case "animationclip":
                    newRecorder = new AnimationRecorderSettings { name = "Animation Clip Recorder" };
                    break;

                default:
                    return Response.Error(
                        $"INVALID_PARAMETER: Unknown RecorderType '{parameters.RecorderType}'. " +
                        "Use 'Movie', 'ImageSequence', 'Audio', or 'AnimationClip'.");
            }

            if (!string.IsNullOrEmpty(parameters.OutputFile))
                newRecorder.OutputFile = parameters.OutputFile;

            controllerSettings.AddRecorderSettings(newRecorder);
            changes.Add($"Added {parameters.RecorderType} recorder");

            controllerSettings.Save();
            return Response.Success(
                $"Recorder settings updated: {changes.Count} value(s) changed.",
                new { changesApplied = changes.Count, changes });
        }

        static object HandleRemoveRecorder(
            RecorderControllerSettings controllerSettings,
            List<RecorderSettings> recorderList,
            int index,
            List<string> changes)
        {
            if (index < 0 || index >= recorderList.Count)
                return Response.Error(
                    $"INVALID_PARAMETER: RecorderIndex {index} is out of range. " +
                    $"Valid range: 0-{recorderList.Count - 1}.");

            var target = recorderList[index];
            var targetName = target.name;
            controllerSettings.RemoveRecorder(target);
            changes.Add($"Removed recorder at index {index} ({targetName})");

            controllerSettings.Save();
            return Response.Success(
                $"Recorder settings updated: {changes.Count} value(s) changed.",
                new { changesApplied = changes.Count, changes });
        }

        static object HandleToggleRecorder(
            RecorderControllerSettings controllerSettings,
            List<RecorderSettings> recorderList,
            int index,
            bool enabled,
            List<string> changes)
        {
            if (index < 0 || index >= recorderList.Count)
                return Response.Error(
                    $"INVALID_PARAMETER: RecorderIndex {index} is out of range. " +
                    $"Valid range: 0-{recorderList.Count - 1}.");

            var target = recorderList[index];
            target.Enabled = enabled;
            changes.Add($"Recorder at index {index} ({target.name}) enabled = {enabled}");

            controllerSettings.Save();
            return Response.Success(
                $"Recorder settings updated: {changes.Count} value(s) changed.",
                new { changesApplied = changes.Count, changes });
        }

        static object HandleModifyRecorder(
            RecorderControllerSettings controllerSettings,
            List<RecorderSettings> recorderList,
            int index,
            SetRecorderSettingsParams parameters,
            List<string> changes)
        {
            if (index < 0 || index >= recorderList.Count)
                return Response.Error(
                    $"INVALID_PARAMETER: RecorderIndex {index} is out of range. " +
                    $"Valid range: 0-{recorderList.Count - 1}.");

            var target = recorderList[index];

            if (!string.IsNullOrEmpty(parameters.OutputFile))
            {
                target.OutputFile = parameters.OutputFile;
                changes.Add($"Recorder[{index}].outputFile = {parameters.OutputFile}");
            }

            switch (target)
            {
                case MovieRecorderSettings movie:
                    if (parameters.CaptureAudio.HasValue)
                    {
                        movie.CaptureAudio = parameters.CaptureAudio.Value;
                        changes.Add($"Recorder[{index}].captureAudio = {parameters.CaptureAudio.Value}");
                    }
                    if (parameters.CaptureAlpha.HasValue)
                    {
                        movie.CaptureAlpha = parameters.CaptureAlpha.Value;
                        changes.Add($"Recorder[{index}].captureAlpha = {parameters.CaptureAlpha.Value}");
                    }
                    break;

                case ImageRecorderSettings image:
                    ApplyImageSettings(image, parameters, changes, index);
                    break;
            }

            controllerSettings.Save();

            if (changes.Count == 0)
                return Response.Success("No changes applied — all parameters were null/omitted.",
                    new { changesApplied = 0 });

            return Response.Success(
                $"Recorder settings updated: {changes.Count} value(s) changed.",
                new { changesApplied = changes.Count, changes });
        }

        static void ApplyImageSettings(ImageRecorderSettings image, SetRecorderSettingsParams parameters,
            List<string> changes = null, int index = -1)
        {
            string prefix = index >= 0 ? $"Recorder[{index}]." : "";

            if (parameters.ImageOutputFormat != null)
            {
                if (Enum.TryParse<ImageRecorderSettings.ImageRecorderOutputFormat>(
                    parameters.ImageOutputFormat, true, out var fmt))
                {
                    image.OutputFormat = fmt;
                    changes?.Add($"{prefix}outputFormat = {fmt}");
                }
            }

            if (parameters.JpegQuality.HasValue)
            {
                image.JpegQuality = parameters.JpegQuality.Value;
                changes?.Add($"{prefix}jpegQuality = {parameters.JpegQuality.Value}");
            }

            if (parameters.CaptureAlpha.HasValue)
            {
                image.CaptureAlpha = parameters.CaptureAlpha.Value;
                changes?.Add($"{prefix}captureAlpha = {parameters.CaptureAlpha.Value}");
            }

            if (parameters.ImageColorSpace != null)
            {
                if (Enum.TryParse<ImageRecorderSettings.ColorSpaceType>(
                    parameters.ImageColorSpace, true, out var cs))
                {
                    image.OutputColorSpace = cs;
                    changes?.Add($"{prefix}outputColorSpace = {cs}");
                }
            }
        }
    }
}
#endif
