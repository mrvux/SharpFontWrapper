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


        static VertexShader vsGlyphView;
        static PixelShader psGlyphView;

        static ViewportF viewPort;

        static Stopwatch watch = Stopwatch.StartNew();


        [STAThread]
        private static void Main()
        {
            RenderForm renderForm = new RenderForm("SharpFontWrapper - View glyph sheets");
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

            //Vertex/Pixel shaders
            using (ShaderBytecode byteCode = ShaderBytecode.CompileFromFile("GlyphSheetView.hlsl", "VS", "vs_5_0", ShaderFlags.OptimizationLevel3))
            {
                vsGlyphView = new VertexShader(device, byteCode);
            }
            using (ShaderBytecode byteCode = ShaderBytecode.CompileFromFile("GlyphSheetView.hlsl", "PS", "ps_5_0", ShaderFlags.OptimizationLevel3))
            {
                psGlyphView = new PixelShader(device, byteCode);
            }

            fontFactory = new SharpFontWrapper.Factory();
            fontWrapper = fontFactory.CreateFontWrapper(device, "Arial");

            renderStates = fontWrapper.RenderStates;

            bool showGlyphMode = false;
            renderForm.KeyDown += (sender, args) =>
            {
                if (args.KeyCode == System.Windows.Forms.Keys.Space)
                    showGlyphMode = !showGlyphMode;

            };

            RenderLoop.Run(renderForm, () =>
            {
                renderForm.Text = string.Format("SharpFontWrapper - Glyph Sheets view - {0} sheets built", fontWrapper.GlyphAtlas.SheetCount);

                float t = (float)watch.Elapsed.TotalSeconds;

                deviceContext.ClearRenderTargetView(renderView, Color.Black);
                deviceContext.OutputMerger.SetRenderTargets(renderView);
                deviceContext.Rasterizer.SetViewport(viewPort);

                SharpFontWrapper.TextFlags flags = SharpFontWrapper.TextFlags.NoWordWrapping
                    | SharpFontWrapper.TextFlags.VerticalCenter
                    | SharpFontWrapper.TextFlags.Center;


                if (showGlyphMode)
                {
                    deviceContext.VertexShader.Set(vsGlyphView);
                    deviceContext.GeometryShader.Set(null); //Text uses GS, so it might still be bound, ensure to remove it
                    deviceContext.PixelShader.Set(psGlyphView);

                    deviceContext.PixelShader.SetShaderResource(0, fontWrapper.GlyphAtlas.GetSheet(0).SheetTexture);
                    

                    deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
                    deviceContext.Draw(3, 0);
                }
                else
                {
                    fontWrapper.DrawString(deviceContext, "SharpFontWrapper, space to toggle glyph view", 32.0f, new Vector2(renderForm.Width * 0.5f, renderForm.Height * 0.5f), Color.White, flags);
                }
                swapChain.Present(1, SharpDX.DXGI.PresentFlags.None);

                
            });



            renderStates.Dispose();
            fontWrapper.Dispose();
            fontFactory.Dispose();

            vsGlyphView.Dispose();
            psGlyphView.Dispose();
            deviceContext.ClearState();
            deviceContext.Flush();

            renderView.Dispose();
            swapChain.Dispose();
            deviceContext.Dispose();
            device.Dispose();
        }

        private static void RenderForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

