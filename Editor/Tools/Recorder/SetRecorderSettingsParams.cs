#if MCP_TOOLKIT_RECORDER
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Parameters for the McpToolkit.SetRecorderSettings MCP tool.
    /// All properties are optional — only supplied values are applied.
    /// </summary>
    public record SetRecorderSettingsParams
    {
        // ── Session-level settings ──────────────────────────────────────

        [McpDescription("Recording frame rate (e.g. 30, 60). Applied to the session.", Required = false)]
        public float? FrameRate { get; set; }

        [McpDescription(
            "Frame rate playback mode: 'Constant' or 'Variable'. Constant locks the " +
            "simulation to the target frame rate; Variable uses it as a maximum.",
            Required = false)]
        public string FrameRatePlayback { get; set; }

        [McpDescription("Cap the rendering frame rate to the target frame rate.", Required = false)]
        public bool? CapFrameRate { get; set; }

        [McpDescription("Exit Play mode automatically when the recording finishes.", Required = false)]
        public bool? ExitPlayMode { get; set; }

        // ── Record mode ─────────────────────────────────────────────────

        [McpDescription(
            "Record mode: 'Manual', 'SingleFrame', 'FrameInterval', or 'TimeInterval'. " +
            "Manual starts/stops via script. SingleFrame records one frame (set SingleFrameNumber). " +
            "FrameInterval records between StartFrame and EndFrame. " +
            "TimeInterval records between StartTime and EndTime.",
            Required = false)]
        public string RecordMode { get; set; }

        [McpDescription("Start frame for FrameInterval mode.", Required = false)]
        public int? StartFrame { get; set; }

        [McpDescription("End frame for FrameInterval mode.", Required = false)]
        public int? EndFrame { get; set; }

        [McpDescription("Frame number for SingleFrame mode.", Required = false)]
        public int? SingleFrameNumber { get; set; }

        [McpDescription("Start time in seconds for TimeInterval mode.", Required = false)]
        public float? StartTime { get; set; }

        [McpDescription("End time in seconds for TimeInterval mode.", Required = false)]
        public float? EndTime { get; set; }

        // ── Recorder list action ────────────────────────────────────────

        [McpDescription(
            "Action to perform on the recorder list: 'AddRecorder', 'RemoveRecorder', " +
            "'EnableRecorder', 'DisableRecorder', or 'ModifyRecorder'. " +
            "AddRecorder requires RecorderType. Remove/Enable/Disable/Modify require RecorderIndex.",
            Required = false)]
        public string Action { get; set; }

        [McpDescription(
            "Index of the target recorder (0-based) for Remove, Enable, Disable, or Modify actions.",
            Required = false)]
        public int? RecorderIndex { get; set; }

        [McpDescription(
            "Type of recorder to add: 'Movie', 'ImageSequence', 'Audio', or 'AnimationClip'.",
            Required = false)]
        public string RecorderType { get; set; }

        // ── Recorder-level properties (for Add / Modify) ───────────────

        [McpDescription(
            "Output file path pattern for the recorder. Supports wildcards like <Take>, " +
            "<Time>, <Frame>, <Scene>, <Resolution>, <Project>, <Product>. " +
            "Example: 'Recordings/movie_<Take>'.",
            Required = false)]
        public string OutputFile { get; set; }

        // Movie-specific
        [McpDescription("Movie recorder: capture audio signal in the output.", Required = false)]
        public bool? CaptureAudio { get; set; }

        [McpDescription("Movie/Image recorder: capture the alpha channel in the output.", Required = false)]
        public bool? CaptureAlpha { get; set; }

        // Image-specific
        [McpDescription(
            "Image recorder output format: 'PNG', 'JPEG', or 'EXR'.",
            Required = false)]
        public string ImageOutputFormat { get; set; }

        [McpDescription(
            "Image recorder JPEG encoding quality (1-100). Only applies when ImageOutputFormat is 'JPEG'. Default: 75.",
            Required = false)]
        public int? JpegQuality { get; set; }

        [McpDescription(
            "Image recorder color space: 'sRGB_sRGB' or 'Unclamped_linear_sRGB'.",
            Required = false)]
        public string ImageColorSpace { get; set; }
    }
}
#endif
