using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// MCP tool that forces a domain reload in the Unity Editor,
    /// even when the editor is not in the foreground.
    ///
    /// This works by calling AssetDatabase.Refresh() programmatically
    /// (bypasses the focus-gated auto-refresh) and nudging the editor
    /// loop with QueuePlayerLoopUpdate().
    /// </summary>
    public static class ForceDomainRefreshTool
    {
        const string ToolName = "Custom.ForceDomainRefresh";

        const string Description =
            "Forces Unity to scan for changed assets, recompile scripts, and perform a domain reload. " +
            "Works even when the Unity Editor is not in the foreground. " +
            "Use after modifying scripts externally to trigger recompilation without switching to Unity.";

        const string Title = "Force Asset Refresh and Domain Reload";

        [McpTool(ToolName, Description, Title, EnabledByDefault = true)]
        public static object HandleCommand()
        {
            try
            {
                bool wasPlaying = EditorApplication.isPlaying;

                // Force-scan for changed assets and recompile scripts
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                // Explicitly request script compilation in case Refresh alone isn't enough
                CompilationPipeline.RequestScriptCompilation();

                // Nudge the editor loop so it processes the refresh even when unfocused
                EditorApplication.QueuePlayerLoopUpdate();

                return Response.Success(
                    "Asset refresh and script recompilation requested. " +
                    "Domain reload will occur if scripts changed. " +
                    $"Editor was {(wasPlaying ? "in Play mode" : "in Edit mode")}.",
                    new
                    {
                        was_playing = wasPlaying,
                        refresh_requested = true,
                        compilation_requested = true
                    });
            }
            catch (System.Exception ex)
            {
                return Response.Error($"REFRESH_FAILED: {ex.Message}");
            }
        }
    }
}
