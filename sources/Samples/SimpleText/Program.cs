using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SwapChain = SharpDX.DXGI.SwapChain;

namespace SimpleText
{
    public static class Program
    {
        static Device device;
        static DeviceContext deviceContext;

        static SwapChain swapChain;
        static RenderTargetView renderView;

        static SharpFontWrapper.Factory fontFactory;
        static SharpFontWrapper.FontWrapper fontWrapper;
        static ViewportF viewPort;

        static Stopwatch watch = Stopwatch.StartNew();

        //TextFormat and TextLayout
        static SharpDX.DirectWrite.TextFormat textFormat;
        static SharpDX.DirectWrite.TextLayout textLayout;

        [STAThread]
        private static void Main()
        {
            RenderForm renderForm = new RenderForm("SharpFontWrapper - Simple Text");
            renderForm.Width = 1024;
            renderForm.Height = 768;

            viewPort = new ViewportF(0, 0, renderForm.Width, renderForm.Height, 0.0f, 1.0f);

            device = new Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug);
            deviceContext = device.ImmediateContext;

            using (SharpDX.DXGI.Factory dxgiFactory = new SharpDX.DXGI.Factory1())
            {
                SharpDX.DXGI.SwapChainDescription swapChainDesc = new SharpDX.DXGI.SwapChainDescription()
                {
                    BufferCount = 2,
                    Flags = SharpDX.DXGI.SwapChainFlags.None,
                    IsWindowed = true,
                    ModeDescription = new SharpDX.DXGI.ModeDescription(0, 0, new SharpDX.DXGI.Rational(60, 1), SharpDX.DXGI.Format.R8G8B8A8_UNorm),
                    OutputHandle = renderForm.Handle,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(4, 0),
                    SwapEffect = SharpDX.DXGI.SwapEffect.Discard,
                    Usage = SharpDX.DXGI.Usage.BackBuffer | SharpDX.DXGI.Usage.RenderTargetOutput
                };

                swapChain = new SwapChain(dxgiFactory, device, swapChainDesc);

                using (Texture2D backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
                {
                    renderView = new RenderTargetView(device, backBuffer);
                }
            }


            fontFactory = new SharpFontWrapper.Factory();

            fontWrapper = fontFactory.CreateFontWrapper(device, "Arial");

            SharpFontWrapper.ColorRGBA colorStyle = fontFactory.CreateColor(SharpDX.Color.Green);

            textFormat = new SharpDX.DirectWrite.TextFormat(fontWrapper.DWriteFactory, "Consolas", 32.0f);
            textFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
            textFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Near;

            textLayout = new SharpDX.DirectWrite.TextLayout(fontWrapper.DWriteFactory, "DirectWrite Text Layout with Range formatting and color", textFormat, 1000.0f, 1000.0f);
            textLayout.SetFontStyle(SharpDX.DirectWrite.FontStyle.Italic, new SharpDX.DirectWrite.TextRange(0, 11));
            textLayout.SetFontWeight(SharpDX.DirectWrite.FontWeight.Bold, new SharpDX.DirectWrite.TextRange(13, 4));

            //Note : to pass color, make sure to use native pointer, as in that case we do not need sharpdx/.net to build com wrapper, colorStyle is already one
            textLayout.SetDrawingEffect(colorStyle.NativePointer, new SharpDX.DirectWrite.TextRange(17, 6));

            RenderLoop.Run(renderForm, () =>
            {
                float t = (float)watch.Elapsed.TotalMilliseconds;

                float x = ((float)Math.Sin(t * 0.002f) * 100.0f) + (renderForm.Width * 0.5f);

                deviceContext.ClearRenderTargetView(renderView, Color.White);
                deviceContext.OutputMerger.SetRenderTargets(renderView);
                deviceContext.Rasterizer.SetViewport(viewPort);

                SharpFontWrapper.TextFlags flags = SharpFontWrapper.TextFlags.NoWordWrapping
                    | SharpFontWrapper.TextFlags.VerticalCenter
                    | SharpFontWrapper.TextFlags.Center;

                fontWrapper.DrawString(deviceContext, "Hello SharpFontWrapper", 64.0f, new Vector2(x, renderForm.Height * 0.25f), Color.Red, flags);

                fontWrapper.DrawString(deviceContext, "This is another text", 64.0f, new Vector2(renderForm.Width * 0.5f, renderForm.Height * 0.25f + 100.0f), Color.Black, flags);

                fontWrapper.DrawTextLayout(deviceContext, textLayout, new Vector2(0, renderForm.Height * 0.25f + 200.0f), Color.Black, flags);

                swapChain.Present(1, SharpDX.DXGI.PresentFlags.None);
            });


            textLayout.Dispose();
            textFormat.Dispose();

            fontWrapper.Dispose();
            fontFactory.Dispose();

            deviceContext.ClearState();
            deviceContext.Flush();

            renderView.Dispose();
            swapChain.Dispose();
            deviceContext.Dispose();
            device.Dispose();
        }
    } 
}
