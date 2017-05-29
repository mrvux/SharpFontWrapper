using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using SwapChain = SharpDX.DXGI.SwapChain;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CustomShader
{
    public static class Program
    {
        static Device device;
        static DeviceContext2 deviceContext;

        static SwapChain swapChain;
        static RenderTargetView renderView;

        static SharpFontWrapper.Factory fontFactory;
        static SharpFontWrapper.FontWrapper fontWrapper;
        static SharpFontWrapper.GlyphRenderStates renderStates;


        static PixelShader customPixelShader;
        static Buffer cBuffer;

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

            //Pixel shader and custom cbuffer
            using (ShaderBytecode byteCode = ShaderBytecode.CompileFromFile("PixelShader.hlsl", "PS" ,"ps_5_0", ShaderFlags.OptimizationLevel3))
            {
                customPixelShader = new PixelShader(device, byteCode);
            }

            BufferDescription cbufferDesc = new BufferDescription()
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = 16, //Need to be 16 aligned
                StructureByteStride = 0,
                Usage = ResourceUsage.Dynamic
            };
            cBuffer = new Buffer(device, cbufferDesc);


            fontFactory = new SharpFontWrapper.Factory();
            fontWrapper = fontFactory.CreateFontWrapper(device, "Arial");

            renderStates = fontWrapper.RenderStates;

            RenderLoop.Run(renderForm, () =>
            {
                float t = (float)watch.Elapsed.TotalSeconds;

                deviceContext.ClearRenderTargetView(renderView, Color.Black);
                deviceContext.OutputMerger.SetRenderTargets(renderView);
                deviceContext.Rasterizer.SetViewport(viewPort);

                SharpFontWrapper.TextFlags flags = SharpFontWrapper.TextFlags.NoWordWrapping
                    | SharpFontWrapper.TextFlags.VerticalCenter
                    | SharpFontWrapper.TextFlags.Center
                    | SharpFontWrapper.TextFlags.StatePrepared; //Make sure this is on,otherwise pixelshader will be reverted to default

                //Custom cbuffer is not overriden, we can set it anywhere
                DataStream ds;
                deviceContext.MapSubresource(cBuffer, MapMode.WriteDiscard, MapFlags.None, out ds);
                ds.Write<float>(-t*5.0f);
                deviceContext.UnmapSubresource(cBuffer, 0);


                deviceContext.PixelShader.SetConstantBuffer(1, cBuffer);

                //Set default render states
                renderStates.SetStates(deviceContext, 0);

                deviceContext.PixelShader.Set(customPixelShader);

                fontWrapper.DrawString(deviceContext, "SharpFontWrapper", 96.0f, new Vector2(renderForm.Width * 0.5f, renderForm.Height * 0.5f), Color.White, flags);

                swapChain.Present(1, SharpDX.DXGI.PresentFlags.None);
            });


            
            renderStates.Dispose();
            fontWrapper.Dispose();
            fontFactory.Dispose();

            cBuffer.Dispose();
            customPixelShader.Dispose();
            deviceContext.ClearState();
            deviceContext.Flush();

            renderView.Dispose();
            swapChain.Dispose();
            deviceContext.Dispose();
            device.Dispose();
        }
    }
}

