using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Parameters for the McpToolkit.SetTimeAndAudioSettings MCP tool.
    /// All properties are optional — only supplied values are applied.
    /// </summary>
    public record SetTimeAndAudioSettingsParams
    {
        // --- Time ---

        [McpDescription("Fixed timestep interval in seconds (default: 0.02).", Required = false)]
        public float? FixedDeltaTime { get; set; }

        [McpDescription("Maximum delta time per frame in seconds.", Required = false)]
        public float? MaximumDeltaTime { get; set; }

        [McpDescription("Time scale factor (1 = normal, 0 = paused, 2 = double speed).", Required = false)]
        public float? TimeScale { get; set; }

        [McpDescription("Maximum particle delta time in seconds.", Required = false)]
        public float? MaximumParticleDeltaTime { get; set; }

        [McpDescription("Capture delta time for recording (0 = disabled).", Required = false)]
        public float? CaptureDeltaTime { get; set; }

        // --- Audio ---

        [McpDescription("Audio output sample rate in Hz (e.g. 44100, 48000).", Required = false)]
        public int? SampleRate { get; set; }

        [McpDescription("Speaker mode: 'Mono', 'Stereo', 'Quad', 'Surround', 'Mode5point1', 'Mode7point1', or 'Prologic'.", Required = false)]
        public string SpeakerMode { get; set; }

        [McpDescription("DSP buffer size (256, 512, 1024, etc.). Smaller = lower latency, higher CPU.", Required = false)]
        public int? DspBufferSize { get; set; }

        [McpDescription("Number of real audio voices (simultaneously playing).", Required = false)]
        public int? NumRealVoices { get; set; }

        [McpDescription("Number of virtual audio voices (tracked but not rendered).", Required = false)]
        public int? NumVirtualVoices { get; set; }
    }
}
