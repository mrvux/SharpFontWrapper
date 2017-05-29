﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D11;
using SharpDX;

namespace SharpFontWrapper
{
    /// <summary>The IFW1FontWrapper interface is the main interface used to draw text.
    /// It holds references to all objects needed to format and convert text to vertices, as well as the D3D11 states and buffers needed to draw them.</summary>
    /// <remarks>Create a font-wrapper using IFW1Factory::CreateFontWrapper</remarks>
    public unsafe partial class FontWrapper
    {
        /// <summary>
        /// Draws a string
        /// </summary>
        /// <param name="deviceContext">A valid dirct3d11 device context</param>
        /// <param name="s">String to draw</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="origin">Origin (top left)</param>
        /// <param name="color">Color</param>
        /// <param name="flags">Draw flags</param>
        public void DrawString(DeviceContext deviceContext, string s, float fontSize, Vector2 origin, Color4 color, TextFlags flags)
        {
            DrawString(deviceContext, s, fontSize, origin.X, origin.Y, color.ToBgra(), flags);
        }

        /// <summary>
        /// Draws a string using a transformation matrix
        /// </summary>
        /// <param name="deviceContext">A valid dirct3d11 device context</param>
        /// <param name="s">String to draw</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="color">Color</param>
        /// <param name="flags">Draw flags</param>
        /// <param name="fontFamily">Font family</param>
        /// <param name="transform">A 3d transformation</param>
        public void DrawString(DeviceContext deviceContext, string s, string fontFamily, float fontSize, Matrix transform, Color4 color, TextFlags flags)
        {
            DrawString(deviceContext, s, fontFamily, fontSize, SharpDX.RectangleF.Empty, color.ToBgra(), IntPtr.Zero, new IntPtr(&transform), flags);
        }
    }
}