using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Unity.AI.MCP.Editor.Helpers;
using Unity.AI.MCP.Editor.ToolRegistry;

namespace WhatUpGames.McpToolkit.Editor
{
    /// <summary>
    /// Custom MCP tool that captures a screenshot of the Game View and returns it
    /// as a base64-encoded PNG image.
    ///
    /// Uses the same GrabPixels approach as Unity's own built-in MCP camera capture
    /// tools (WindowUtils.CaptureEditorWindow). This reads the pixel buffer directly
    /// from the Game View window, capturing the full rendered output including all
    /// cameras, post-processing effects, and UI Toolkit overlays.
    ///
    /// Works in both Play and Edit mode (Edit mode shows whatever the Game View is
    /// currently displaying, which may be a static frame or nothing).
    /// </summary>
    public static class GameViewCaptureTool
    {
        const string ToolName = "McpToolkit.GameViewCapture";

        const string Description =
            "Captures a screenshot of the Game View (the final rendered frame the player sees, " +
            "including UI overlays and post-processing) and returns it as a base64-encoded PNG image. " +
            "By default, images larger than 1920px in either dimension are downscaled (aspect-ratio " +
            "preserved) to keep the payload size manageable. Set MaxDimension to 0 for full resolution. " +
            "The Game View window must be open in the Unity Editor. Works in both Play and Edit mode.";

        const string Title = "Capture Game View Screenshot";

        // Cached reflection handles for GrabPixels
        static FieldInfo s_mParentField;
        static MethodInfo s_grabPixelsMethod;

        /// <summary>
        /// Output schema so MCP clients know the shape of the response data.
        /// </summary>
        [McpOutputSchema(ToolName)]
        public static object GetOutputSchema()
        {
            return new
            {
                type = "object",
                properties = new
                {
                    success = new { type = "boolean", description = "Whether the capture succeeded" },
                    message = new { type = "string", description = "Human-readable result message" },
                    data = new
                    {
                        type = "object",
                        description = "Screenshot data",
                        properties = new
                        {
                            image_base64 = new { type = "string", description = "Base64-encoded PNG image data" },
                            width = new { type = "integer", description = "Image width in pixels (after any downscaling)" },
                            height = new { type = "integer", description = "Image height in pixels (after any downscaling)" },
                            original_width = new { type = "integer", description = "Original capture width before downscaling" },
                            original_height = new { type = "integer", description = "Original capture height before downscaling" },
                            format = new { type = "string", description = "Image format (always 'png')" },
                            size_bytes = new { type = "integer", description = "Size of the PNG data in bytes" }
                        }
                    }
                },
                required = new[] { "success", "message" }
            };
        }

        /// <summary>
        /// Main MCP tool handler. Captures the Game View via GrabPixels and returns base64 PNG.
        /// </summary>
        [McpTool(ToolName, Description, Title, EnabledByDefault = true, Groups = new[] { "MCP Toolkit" })]
        public static object HandleCommand(GameViewCaptureParams parameters)
        {
            int superSize = parameters?.SuperSize ?? 1;
            int maxDimension = parameters?.MaxDimension ?? 1920;

            // Validate superSize range
            if (superSize < 1 || superSize > 4)
            {
                return Response.Error(
                    "INVALID_PARAMETER: SuperSize must be between 1 and 4. " +
                    $"Received: {superSize}");
            }

            if (maxDimension < 0)
            {
                return Response.Error(
                    "INVALID_PARAMETER: MaxDimension must be 0 (disabled) or a positive integer. " +
                    $"Received: {maxDimension}");
            }

            try
            {
                // Find the Game View window
                var gameViewType = Type.GetType(
                    "UnityEditor.GameView, UnityEditor.CoreModule") ??
                    Type.GetType("UnityEditor.GameView, UnityEditor");

                if (gameViewType == null)
                {
                    return Response.Error(
                        "INTERNAL_ERROR: Could not resolve UnityEditor.GameView type.");
                }

                var gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
                if (gameView == null)
                {
                    return Response.Error(
                        "GAME_VIEW_NOT_FOUND: No Game View window is open. " +
                        "Open the Game View tab in the Unity Editor and try again.");
                }

                // EditorWindow.position gives the window rect in OS screen *points*
                // (logical pixels). On high-DPI displays we must multiply by the
                // DPI scale factor (EditorGUIUtility.pixelsPerPoint) to get actual
                // pixel dimensions. This matches Unity's own WindowUtils /
                // ScreenContextUtility which uses this exact formula.
                float scalingFactor = EditorGUIUtility.pixelsPerPoint;
                int width = Mathf.RoundToInt(gameView.position.width * scalingFactor) * superSize;
                int height = Mathf.RoundToInt(gameView.position.height * scalingFactor) * superSize;

                if (width <= 0 || height <= 0)
                {
                    return Response.Error(
                        $"INVALID_DIMENSIONS: Game View has invalid size ({width}x{height}). " +
                        "Ensure the Game View window is visible and not minimized.");
                }

                // Capture using GrabPixels
                Texture2D tex = CaptureViaGrabPixels(gameView, width, height);

                if (tex == null)
                {
                    return Response.Error(
                        "CAPTURE_FAILED: GrabPixels returned no image data. " +
                        "Ensure the Game View window is open and visible.");
                }

                try
                {
                    int originalWidth = tex.width;
                    int originalHeight = tex.height;

                    // Downscale if the image exceeds MaxDimension in either axis
                    if (maxDimension > 0 &&
                        (tex.width > maxDimension || tex.height > maxDimension))
                    {
                        var resized = DownscaleTexture(tex, maxDimension);
                        UnityEngine.Object.DestroyImmediate(tex);
                        tex = resized;
                    }

                    byte[] pngBytes = tex.EncodeToPNG();

                    if (pngBytes == null || pngBytes.Length == 0)
                    {
                        return Response.Error("ENCODE_FAILED: Failed to encode screenshot to PNG.");
                    }

                    string base64 = Convert.ToBase64String(pngBytes);

                    string sizeNote = (tex.width != originalWidth || tex.height != originalHeight)
                        ? $", downscaled from {originalWidth}x{originalHeight}"
                        : "";

                    return Response.Success(
                        $"Game View screenshot captured successfully ({tex.width}x{tex.height}, " +
                        $"{pngBytes.Length} bytes{sizeNote}).",
                        new
                        {
                            image_base64 = base64,
                            width = tex.width,
                            height = tex.height,
                            original_width = originalWidth,
                            original_height = originalHeight,
                            format = "png",
                            size_bytes = pngBytes.Length
                        });
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                }
            }
            catch (Exception ex)
            {
                return Response.Error($"CAPTURE_EXCEPTION: {ex.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Captures an EditorWindow's pixel buffer using the GrabPixels method on GUIView.
        /// Matches Unity's WindowUtils.CaptureEditorWindow implementation exactly.
        /// </summary>
        static Texture2D CaptureViaGrabPixels(EditorWindow window, int width, int height)
        {
            // Match Unity's WindowUtils exactly: use PlayerSettings.colorSpace (not
            // QualitySettings.activeColorSpace) and RenderTextureDescriptor with sRGB = false
            RenderTextureFormat rtformat = PlayerSettings.colorSpace == ColorSpace.Gamma
                ? RenderTextureFormat.ARGB32
                : RenderTextureFormat.ARGBHalf;

            var desc = new RenderTextureDescriptor
            {
                width = width,
                height = height,
                dimension = TextureDimension.Tex2D,
                depthBufferBits = 0,
                colorFormat = rtformat,
                mipCount = 0,
                volumeDepth = 1,
                msaaSamples = 1,
                sRGB = false
            };
            var rt = new RenderTexture(desc);
            var rect = new Rect(0, 0, width, height);

            if (!GrabPixelsFromWindow(window, rt, rect))
            {
                UnityEngine.Object.DestroyImmediate(rt);
                return null;
            }

            var oldRt = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.ReadPixels(rect, 0, 0, false);
            tex.Apply(false, false);
            RenderTexture.active = oldRt;
            UnityEngine.Object.DestroyImmediate(rt);

            return tex;
        }

        /// <summary>
        /// Calls GrabPixels on the EditorWindow's m_Parent (GUIView/DockArea) via reflection.
        /// Wrapped in GL.PushMatrix/LoadOrtho/PopMatrix to ensure clean GL state.
        /// Matches Unity's WindowUtils.Backport implementation.
        /// </summary>
        static bool GrabPixelsFromWindow(EditorWindow window, RenderTexture rt, Rect rect)
        {
            try
            {
                // Get the m_Parent field (GUIView / DockArea that hosts the window)
                s_mParentField ??= typeof(EditorWindow).GetField(
                    "m_Parent",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (s_mParentField == null)
                {
                    Debug.LogError("[GameViewCapture] Could not find EditorWindow.m_Parent field.");
                    return false;
                }

                var parent = s_mParentField.GetValue(window);
                if (parent == null)
                {
                    Debug.LogError("[GameViewCapture] EditorWindow.m_Parent is null.");
                    return false;
                }

                // Resolve GrabPixels from the field's declared type, not the runtime type.
                // This matches Unity's Backport: s_MParentField.FieldType.GetMethod(...)
                s_grabPixelsMethod ??= s_mParentField.FieldType.GetMethod(
                    "GrabPixels",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { typeof(RenderTexture), typeof(Rect) },
                    null);

                if (s_grabPixelsMethod == null)
                {
                    Debug.LogError(
                        $"[GameViewCapture] Could not find GrabPixels on {s_mParentField.FieldType.Name}.");
                    return false;
                }

                // Invoke GrabPixels with clean GL state
                GL.PushMatrix();
                GL.LoadOrtho();
                s_grabPixelsMethod.Invoke(parent, new object[] { rt, rect });
                GL.PopMatrix();

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameViewCapture] GrabPixels failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Downscales a Texture2D so that neither dimension exceeds maxDimension,
        /// preserving aspect ratio. Uses multi-step halving for quality (same
        /// strategy as Unity's TextureUtils.ResizeTexture).
        /// </summary>
        static Texture2D DownscaleTexture(Texture2D source, int maxDimension)
        {
            // Calculate target dimensions (aspect-preserving)
            int targetWidth, targetHeight;
            if (source.width >= source.height)
            {
                targetWidth = maxDimension;
                targetHeight = Mathf.Max(1, Mathf.RoundToInt((float)source.height * maxDimension / source.width));
            }
            else
            {
                targetHeight = maxDimension;
                targetWidth = Mathf.Max(1, Mathf.RoundToInt((float)source.width * maxDimension / source.height));
            }

            float scaleRatio = Mathf.Max(
                (float)source.width / targetWidth,
                (float)source.height / targetHeight);

            // For small ratios a single blit is fine
            if (scaleRatio <= 2f)
            {
                return BlitToTexture2D(source, targetWidth, targetHeight);
            }

            // Multi-step: repeatedly halve until within 2x of target, then final blit
            RenderTexture current = RenderTexture.GetTemporary(
                source.width, source.height, 0, RenderTextureFormat.Default);
            Graphics.Blit(source, current);

            int curW = source.width;
            int curH = source.height;

            while (Mathf.Max((float)curW / targetWidth, (float)curH / targetHeight) > 2f)
            {
                curW = Mathf.Max(curW / 2, targetWidth);
                curH = Mathf.Max(curH / 2, targetHeight);

                RenderTexture next = RenderTexture.GetTemporary(
                    curW, curH, 0, RenderTextureFormat.Default);
                Graphics.Blit(current, next);
                RenderTexture.ReleaseTemporary(current);
                current = next;
            }

            // Final blit to exact target size
            RenderTexture final_ = RenderTexture.GetTemporary(
                targetWidth, targetHeight, 0, RenderTextureFormat.Default);
            Graphics.Blit(current, final_);
            RenderTexture.ReleaseTemporary(current);

            RenderTexture oldRt = RenderTexture.active;
            RenderTexture.active = final_;
            var result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0, false);
            result.Apply(false, false);
            RenderTexture.active = oldRt;
            RenderTexture.ReleaseTemporary(final_);

            return result;
        }

        /// <summary>
        /// Single-pass blit resize for small scale ratios.
        /// </summary>
        static Texture2D BlitToTexture2D(Texture source, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(
                targetWidth, targetHeight, 0, RenderTextureFormat.Default);
            Graphics.Blit(source, rt);

            RenderTexture oldRt = RenderTexture.active;
            RenderTexture.active = rt;
            var result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0, false);
            result.Apply(false, false);
            RenderTexture.active = oldRt;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }
    }
}
