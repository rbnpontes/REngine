using System.Collections.Concurrent;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Resources;
using REngine.Core.Threading;
using REngine.RHI;
using REngine.RPI.Events;
using REngine.RPI.Utils;

namespace REngine.RPI;

internal sealed class FontSystem : IFontSystem, IDisposable
{
    struct FontEntry
    {
        public ITexture Atlas;
        public Font Font;
    }
    private readonly IServiceProvider pServiceProvider;
    private readonly ITextureManager pTextureManager;
    private readonly ILogger<IFontSystem> pLogger;
    private readonly IExecutionPipeline pExecutionPipeline;
    private readonly PipelineStateManagerEvents pPipelineStateEvents;
    private readonly EngineEvents pEngineEvents;
    
    private readonly ConcurrentDictionary<ulong, FontEntry> pFontEntries = new();
    private readonly ConcurrentQueue<Font> pFonts2Process = new();
    
    private bool pDisposed;

    public FontSystem(
        ITextureManager textureManager,
        IExecutionPipeline executionPipeline,
        ILoggerFactory loggerFactory,
        PipelineStateManagerEvents pipelineEvents,
        IServiceProvider provider,
        EngineEvents engineEvents)
    {
        pServiceProvider = provider;
        pTextureManager = textureManager;
        pLogger = loggerFactory.Build<IFontSystem>();
        pPipelineStateEvents = pipelineEvents;
        pEngineEvents = engineEvents;
        pExecutionPipeline = executionPipeline;
        
        pipelineEvents.OnDisposed.Once(HandlePipelineStateDispose);
        engineEvents.OnStart.Once(HandleEngineStart);
    }

    private async Task HandleEngineStart(object sender)
    {
        await EngineGlobals.MainDispatcher.Yield();
        pExecutionPipeline.AddEvent(DefaultEvents.RenderBeginId, OnRenderBegin);
    }

    private async Task HandlePipelineStateDispose(object sender)
    {
        await EngineGlobals.MainDispatcher.Yield();
        Dispose();
    }

    public bool HasPendingFonts => !pFonts2Process.IsEmpty;
     
    public void Dispose()
    {
        if (pDisposed)
            return;
        pDisposed = true;
        
        ClearAllFonts();
    }

    private void OnRenderBegin(IExecutionPipeline obj)
    {
        // Usually, there's no reason to process a lot of fonts
        // in one frame, in fact there's no reason to someone
        // load more than 1 font per frame
        // and if this occurs, well they need to wait.
        if (!pFonts2Process.TryDequeue(out var font))
            return;

        var backend = pServiceProvider.Get<IGraphicsDriver>().Backend;
        pLogger.Info($"Building Font '{font.Name}'");

        var builder = new SdfBuilder(font.Atlas)
        {
            Radius = 4,
            Cutoff = 0.45f
        };
        var texture = AllocateTexture(font, builder.Build(), backend);
        var fontEntry = new FontEntry
        {
            Atlas = texture,
            Font = font
        };
        // If some reason this object has been add, then it needs to be destroyed
        if(!pFontEntries.TryAdd(Hash.Digest(font.Name), fontEntry))
            DisposableQueue.Enqueue(texture);
    }

    private ITexture AllocateTexture(Font font, Image image, GraphicsBackend backend)
    {
        // ReSharper disable once InvertIf
        if (backend == GraphicsBackend.OpenGL)
        {
            var tmp = image;
            image = new Image();
            image.SetData(new ImageDataInfo()
            {
                Data = new byte[tmp.Size.Width * tmp.Size.Height * 4],
                Components = 4,
                Size = tmp.Size
            });
            
            for (ushort x = 0; x < tmp.Size.Width; ++x)
            for (ushort y = 0; y < tmp.Size.Height; ++y)
                image.SetPixel(tmp.GetPixel(x, y), x, y);
        }

        return pTextureManager.Create(new TextureDesc()
        {
            Name = $"Font ({font.Name}) Texture",
            AccessFlags = CpuAccessFlags.None,
            Size = new TextureSize(image.Size.Width, image.Size.Height),
            BindFlags = BindFlags.ShaderResource,
            Usage = Usage.Immutable,
            Dimension = TextureDimension.Tex2D,
            Format = backend == GraphicsBackend.OpenGL ? TextureFormat.RGBA8UNorm : TextureFormat.R8UNorm
        }, [new ByteTextureData(image.Data, image.Stride)]);
    }
    
    public void SetFont(Font font)
    {
        if (pFontEntries.ContainsKey(Hash.Digest(font.Name)))
            return;
        pFonts2Process.Enqueue(font);
    }

    public ITexture? GetFontAtlas(string fontName)
    {
        return GetFontAtlas(Hash.Digest(fontName));
    }
    public ITexture? GetFontAtlas(ulong fontId)
    {
        if(pFontEntries.TryGetValue(fontId, out var fontEntry))
            return fontEntry.Atlas;
        return null;
    }
    public Font? GetFont(string fontName)
    {
        return GetFont(Hash.Digest(fontName));
    }
    public Font? GetFont(ulong fontId)
    {
        if (pFontEntries.TryGetValue(fontId, out var fontEntry))
            return fontEntry.Font;
        return null;
    }
    public void ClearFont(string fontName)
    {
        ClearFont(Hash.Digest(fontName));
    }
    public void ClearFont(ulong fontId)
    {
        pFontEntries.TryRemove(fontId, out var fontEntry);
        DisposableQueue.Enqueue(fontEntry.Atlas);
    }
    public void ClearAllFonts()
    {
        pLogger.Info("Clearing all Fonts");
        var fontEntries = pFontEntries.Values.ToArray();
        pFontEntries.Clear();
        foreach (var fontEntry in fontEntries)
            DisposableQueue.Enqueue(fontEntry.Atlas);
    }
}