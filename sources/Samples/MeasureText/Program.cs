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

namespace MeasureText
{
    public static class Program
    {
        static Device device;
        static DeviceContext2 deviceContext;

        static SwapChain swapChain;
        static RenderTargetView renderView;

        static SharpFontWrapper.Factory fontFactory;
        static SharpFontWrapper.FontWrapper fontWrapper;

        static ViewportF viewPort;

        static Stopwatch watch = Stopwatch.StartNew();


        [STAThread]
        private static void Main()
        {
            RenderForm renderForm = new RenderForm("SharpFontWrapper - Simple Text");
            renderForm.Width = 1024;
            renderForm.Height = 768;

            viewPort = new ViewportF(0, 0, renderForm.Width, renderForm.Height, 0.0f, 1.0f);

            device = new Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug);
            deviceContext = new DeviceContext2(device.ImmediateContext.NativePointer);

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

            RenderLoop.Run(renderForm, () =>
            {
                float t = (float)watch.Elapsed.TotalMilliseconds;

                deviceContext.ClearRenderTargetView(renderView, Color.Black);
                deviceContext.OutputMerger.SetRenderTargets(renderView);
                deviceContext.Rasterizer.SetViewport(viewPort);


                SharpFontWrapper.TextFlags flags = SharpFontWrapper.TextFlags.NoWordWrapping
                    | SharpFontWrapper.TextFlags.VerticalCenter
                    | SharpFontWrapper.TextFlags.Center;

                RectangleF layoutRect = new RectangleF(renderForm.Width * 0.5f, renderForm.Height * 0.5f, 0, 0);

                var rect = fontWrapper.MeasureString("Hello SharpFontWrapper", "Arial", 64.0f, layoutRect, flags);

                SharpDX.Rectangle r = new Rectangle((int)rect.Left, (int)rect.Top, (int)rect.Right - (int)rect.Left, (int)rect.Bottom - (int)rect.Top);

                deviceContext.ClearView(renderView, Color.Blue, new SharpDX.Mathematics.Interop.RawRectangle[] { r }, 1);


                fontWrapper.DrawString(deviceContext, "Hello SharpFontWrapper", 64.0f, new Vector2(renderForm.Width * 0.5f, renderForm.Height * 0.5f), Color.White, flags);

                swapChain.Present(1, SharpDX.DXGI.PresentFlags.None);
            });




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

