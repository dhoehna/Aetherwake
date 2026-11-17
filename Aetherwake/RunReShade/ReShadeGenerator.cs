using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using System.Linq;

namespace RunReShade;

public class ReShadeGenerator
{
    private readonly IntPtr _hwnd;

    private readonly ID3D11Device _device;

    private readonly ID3D11DeviceContext _deviceContext;

    public ReShadeGenerator(IntPtr hwnd)
    {
        _hwnd = hwnd;
        var result = D3D11.D3D11CreateDevice(
            null,
            DriverType.Hardware,
            DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
            new List<FeatureLevel> { FeatureLevel.Level_11_0 }.ToArray(),
            out _device,
            out _deviceContext);

        if (result.Failure)
        {
            throw new Exception($"Unable to make device and Device Context because {result.Code}");
        }
    }

    public void ApplyShaders(string baseImageLocation, string saveLocationWithName, string presetText, string depthImageLocation)
    {
        File.WriteAllText("GeneratedPreset.ini", File.ReadAllText(presetText));
        var swapChain = MakeSwapChain();

        // Get bitmaps for textures and depth stencils
        using var bitmap = SKBitmap.Decode(baseImageLocation);
        using var depthBitmap = SKBitmap.Decode(depthImageLocation);

        if (depthBitmap.Width != bitmap.Width || depthBitmap.Height != bitmap.Height)
        {
            throw new Exception(
                $"Depth image ({depthBitmap.Width}x{depthBitmap.Height}) must match color image ({bitmap.Width}x{bitmap.Height}).");
        }

        uint width = (uint)bitmap.Width;
        uint height = (uint)bitmap.Height;

        var sourceTexture = GetSourceTexture(bitmap, width, height);
        var depthTexture = MakeDepthTexture(depthBitmap, width, height);
        var depthStencilView = MakeDepthStencilView(depthTexture);
        var renderedImage = swapChain.GetBuffer<ID3D11Texture2D>(0);
        var renderTargetView = _device.CreateRenderTargetView(renderedImage);

        // The ReShade Addon Generic Depth keeps track of how many Draw calls are made on each Depth Stencil Buffer
        // that is bound to the app.  Generic Depth uses the Depth Stencil View that has the most draw calls.
        var simpleVertexShader = MakeSimpleShader();
        var depthStencilState = MakeDepthStencilState();

        PresentToReshade(
            renderedImage,
            sourceTexture,
            renderTargetView,
            depthStencilView,
            depthStencilState,
            simpleVertexShader,
            width,
            height,
            swapChain);

        // Present then wait for shader compilation to finish.
        WaitForShaderCompilation(TimeSpan.FromMilliseconds(20000));

        // 240 presents to allow Temporal Effects to converge.
        for (int i = 0; i < 240; i++)
        {
            PresentToReshade(
                renderedImage,
                sourceTexture,
                renderTargetView,
                depthStencilView,
                depthStencilState,
                simpleVertexShader,
                width,
                height,
                swapChain);
        }

        var stagingDesc = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Staging,
            BindFlags = BindFlags.None,
            CPUAccessFlags = CpuAccessFlags.Read,
        };
        var stagingTexture = _device.CreateTexture2D(stagingDesc);
        _deviceContext.CopyResource(stagingTexture, renderedImage);
        var mapped = _deviceContext.Map(stagingTexture, 0, MapMode.Read);
        try
        {
            SaveImage(saveLocationWithName, mapped, width, height);
        }
        finally { _deviceContext.Unmap(stagingTexture, 0); }

        renderedImage.Dispose();
        stagingTexture.Dispose();
        sourceTexture.Dispose();
        renderTargetView.Dispose();
        depthStencilView.Dispose();
        depthBitmap.Dispose();
        simpleVertexShader.Dispose();
        depthStencilState.Dispose();
        _deviceContext.ClearState();
        _deviceContext.Flush();
        swapChain.Dispose();
    }

    private void PresentToReshade(
        ID3D11Texture2D renderedImage,
        ID3D11Texture2D sourceTexture,
        ID3D11RenderTargetView renderTargetView,
        ID3D11DepthStencilView depthStencilView,
        ID3D11DepthStencilState depthStencilState,
        ID3D11VertexShader simpleVertexShader,
        uint width,
        uint height,
        IDXGISwapChain swapChain)
    {
        _deviceContext.CopyResource(renderedImage, sourceTexture);
        ResetGraphicPipelineState(renderTargetView, depthStencilView, depthStencilState, simpleVertexShader, width, height);
        IssueDepthCountingDraws();
        var presentResult = swapChain.Present(0, PresentFlags.None);
        if (presentResult.Failure)
        {
            throw new Exception($"Error when presenting {presentResult.Description}");
        }
    }

    private ID3D11DepthStencilState MakeDepthStencilState()
    {
        var depthStateDesc = DepthStencilDescription.Default;
        depthStateDesc.DepthWriteMask = DepthWriteMask.Zero;
        depthStateDesc.DepthFunc = ComparisonFunction.Always;
        return _device.CreateDepthStencilState(depthStateDesc);
    }

    private void IssueDepthCountingDraws()
    {
        // Minimum number of draw calls needed to be recognized by Generic Depth addon.
        for (int i = 0; i < 10; i++)
        {
            // Minimum number of Verticies to be recognized by ReShades Generic Depth addon. 
            _deviceContext.Draw(4, 0);
        }
    }


    // Caller is responsible for disposing the texture.
    private ID3D11Texture2D GetSourceTexture(SKBitmap bitmap, uint width, uint height)
    {
        var sourceInit = new SubresourceData
        {
            DataPointer = bitmap.GetPixels(),
            RowPitch = (uint)bitmap.RowBytes,
        };
        var sourceDesc = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.B8G8R8A8_UNorm,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.RenderTarget,
        };

        var sourceTex = _device.CreateTexture2D(sourceDesc, new[] { sourceInit }) ?? throw new Exception("Could not make Source texture");

        return sourceTex;
    }

    private ID3D11VertexShader MakeSimpleShader()
    {
        // This is the contents of a simple vertex shader that is used to draw on the base image.
        // Coordinates are outside of the view port.
        const string DummyVertexShaderSource = @"
            float4 VS(uint id : SV_VertexID) : SV_Position {
            return float4(2.0, 2.0, 0.5, 1.0);
            }";
        var dummyVsBytecode = Compiler.Compile(
        DummyVertexShaderSource,
        entryPoint: "VS",
        sourceName: "DummyDepthVS",
        profile: "vs_5_0");
        return _device.CreateVertexShader(dummyVsBytecode.Span);
    }

    // Caller is responsible for disposing the depth stencil view.
    private ID3D11DepthStencilView MakeDepthStencilView(ID3D11Texture2D depthTexture)
    {
        var dsvDesc = new DepthStencilViewDescription
        {
            Format = Format.D32_Float,
            ViewDimension = DepthStencilViewDimension.Texture2D,
        };
        dsvDesc.Texture2D.MipSlice = 0;
        return _device.CreateDepthStencilView(depthTexture, dsvDesc) ?? throw new Exception("Could not make a depth stencil view");
    }

    // Caller is responsible for releaseing the texture
    private ID3D11Texture2D MakeDepthTexture(SKBitmap depthBitmap, uint width, uint height)
    {
        var depthFloats = ConvertDepthBitmapToFloats(depthBitmap);
        return _device.CreateTexture2D<float>(
            depthFloats,
            Format.R32_Typeless,
            width,
            height,
            arraySize: 1,
            mipLevels: 1,
            bindFlags: BindFlags.DepthStencil | BindFlags.ShaderResource);
    }

    private void ResetGraphicPipelineState(
        ID3D11RenderTargetView rtv,
        ID3D11DepthStencilView dsv,
        ID3D11DepthStencilState dss,
        ID3D11VertexShader vs,
        uint width,
        uint height)
    {
        _deviceContext.OMSetRenderTargets(rtv, dsv);
        _deviceContext.OMSetDepthStencilState(dss);
        _deviceContext.RSSetViewport(0, 0, width, height);
        _deviceContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        _deviceContext.IASetInputLayout(null);
        _deviceContext.VSSetShader(vs);
        _deviceContext.PSSetShader(null);
    }

    // Polls ReShade.log at regular intervals and hashes the contents.
    // Compilation is done whn the current hash matches the previous hash.
    // a.k.a reshade stops writing to the log.
    private static void WaitForShaderCompilation(TimeSpan timeout)
    {
        bool stillCompiling = true;
        var compilationTimeout = new Stopwatch();
        compilationTimeout.Start();
        var previousHash = new List<byte>();
        using var hashFunction = SHA256.Create();
        using var fileStream = new FileStream("Reshade.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        while ((compilationTimeout.Elapsed < timeout) && stillCompiling)
        {
            Thread.Sleep((int)TimeSpan.FromMilliseconds(500).TotalMilliseconds);
            var hash = new List<byte>(hashFunction.ComputeHash(fileStream) ?? throw new Exception("Could not get hash"));

            if (previousHash.SequenceEqual(hash))
            {
                stillCompiling = false;
            }
            else
            {
                previousHash.Clear();
                previousHash.AddRange(hash);
            }
        }
        compilationTimeout.Stop();
    }


    private static float[] ConvertDepthBitmapToFloats(SKBitmap depthBitmap)
    {
        int total = depthBitmap.Width * depthBitmap.Height;
        var depthFloats = new float[total];
        ReadOnlySpan<byte> src = depthBitmap.GetPixelSpan();
        int bytesPerPixel = depthBitmap.BytesPerPixel;

        switch (depthBitmap.ColorType)
        {
            case SKColorType.Gray8:
                // Single-channel image: only 8-bit depth is available.
                for (int i = 0; i < total; i++)
                {
                    depthFloats[i] = src[i] / 255f;
                }
                break;
            case SKColorType.Bgra8888:
            case SKColorType.Rgba8888:
                // 16-bit unpack: EdenDepthCapture.fx stores the high byte in the
                // first channel (R in RGBA, B in BGRA - both hold the high byte)
                // and the low byte in G. depth = (high * 256 + low) / 65535.
                //
                // Backward compatible with old grayscale captures: for those
                // R == G == B, so (g * 256 + g) / 65535 == g / 255, i.e. the
                // previous 8-bit decode (within sub-1-level dithering error).
                for (int i = 0; i < total; i++)
                {
                    int baseIndex = i * bytesPerPixel;
                    int high = src[baseIndex];     // high byte (R or B, duplicated in both)
                    int low = src[baseIndex + 1];  // low byte (G in both layouts)
                    depthFloats[i] = (high * 256 + low) / 65535f;
                }
                break;
            default:
                throw new NotSupportedException(
                    $"Unsupported depth image color type: {depthBitmap.ColorType}. Re-export as 8-bit grayscale or 8-bit RGBA.");
        }

        return depthFloats;
    }

    private IDXGISwapChain1 MakeSwapChain()
    {
        SwapChainDescription1 swapChainDescription = new()
        {
            Width = 1920,
            Height = 1440,
            Format = Format.B8G8R8A8_UNorm,
            BufferCount = 2,
            BufferUsage = Usage.RenderTargetOutput,
            SampleDescription = SampleDescription.Default,
            Scaling = Scaling.Stretch,
            SwapEffect = SwapEffect.FlipSequential,
            AlphaMode = AlphaMode.Ignore
        };

        SwapChainFullscreenDescription fullscreenDescription = new SwapChainFullscreenDescription
        {
            Windowed = true
        };

        var factory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(false);
        return factory.CreateSwapChainForHwnd(_device, _hwnd, swapChainDescription, fullscreenDescription);
    }

    private void SaveImage(string imageName, MappedSubresource toPrint, uint width, uint heigth)
    {
        // Type is Opaque so the RGB values are preserved and ALpha stays at 255
        var info = new SKImageInfo((int)width, (int)heigth,
            SKColorType.Bgra8888, SKAlphaType.Opaque);

        using var pixmap = new SKPixmap(info, toPrint.DataPointer, (int)toPrint.RowPitch);
        using var image = SKImage.FromPixels(pixmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var fs = File.OpenWrite(imageName);
        data.SaveTo(fs);
    }
}

