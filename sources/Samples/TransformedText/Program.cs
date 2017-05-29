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

namespace TransformedText
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
            RenderForm renderForm = new RenderForm("SharpFontWrapper - Transformed Text");
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

            Matrix view = Matrix.Translation(0.0f, 0.0f, 2.0f);
            Matrix projection = Matrix.PerspectiveFovLH(1.57f, 1024.0f / 768.0f, 0.01f, 100.0f);
            

            int objectCount = 128;
            Matrix[] mats = new Matrix[objectCount];

            Random r = new Random();
            for (int i = 0; i < objectCount; i++)
            {
                mats[i] = Matrix.Translation(r.NextVector3(new Vector3(-5.0f), new Vector3(5.0f)));
            }

            RenderLoop.Run(renderForm, () =>
            {
                float t = (float)watch.Elapsed.TotalMilliseconds;

                deviceContext.ClearRenderTargetView(renderView, Color.Black);
                deviceContext.OutputMerger.SetRenderTargets(renderView);
                deviceContext.Rasterizer.SetViewport(viewPort);

                SharpFontWrapper.TextFlags flags = SharpFontWrapper.TextFlags.NoWordWrapping
                    | SharpFontWrapper.TextFlags.VerticalCenter
                    | SharpFontWrapper.TextFlags.Center;

                Matrix rotation = Matrix.RotationY(t * 0.0001f);

                for (int i = 0; i < objectCount; i++)
                {
                    //Please note that font wrapper defaults to inverted Y
                    Matrix world = Matrix.Scaling(0.002f, -0.002f, 0.002f);
                    world = world * mats[i] * rotation;

                    Matrix twvp = world * view * projection;

                    fontWrapper.DrawString(deviceContext, "SharpFontWrapper", "Arial", 64.0f, twvp, null, Color.White, flags);
                }



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
