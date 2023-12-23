using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI.Constants;
using REngine.RPI.Resources;
using REngine.RPI.Structs;

namespace REngine.RPI.Features;

public sealed class SpriteFeature(
    BatchSystem batchSystem,
    SpriteInstancedBatchSystem instancedBatchSys,
    IShaderResourceBindingCache srbCache,
    IBufferManager bufferMgr) : GraphicsRenderFeature
{
    private readonly BatchGroup pBatchGroup = batchSystem.GetGroup(SpriteSystem.BatchGroupName);
    
    private IPipelineState? pInstancedPipeline;
    private IPipelineState? pTexturedInstancedPipeline;

    public override bool IsDirty { get; protected set; } = true;
    
    public override IRenderFeature MarkAsDirty()
    {
        IsDirty = true;
        return this;
    }

    protected override void OnSetup(in RenderFeatureSetupInfo setupInfo)
    {
        pInstancedPipeline ??= CreatePipeline(setupInfo, false, true);
        pTexturedInstancedPipeline ??= CreatePipeline(setupInfo, true, true);

        IsDirty = false;
    }
    
    private void SetupInstanceSpriteBatch(SpriteInstance batch)
    {
        if (!batch.IsDirty ||  pInstancedPipeline is null)
            return;
        
        batch.Lock();
        
        var resourceMapping = new ResourceMapping();
        resourceMapping
            .Add(ShaderTypeFlags.Vertex, ConstantBufferNames.Frame, bufferMgr.GetBuffer(BufferGroupType.Frame))
            .Add(ShaderTypeFlags.Vertex, ConstantBufferNames.Object, bufferMgr.GetBuffer(BufferGroupType.Object));

        var pipeline = pInstancedPipeline;
        if (batch.Texture is not null)
        {
            pipeline = pTexturedInstancedPipeline;
            resourceMapping.Add(ShaderTypeFlags.Pixel, TextureNames.MainTexture,
                batch.Texture.GetDefaultView(TextureViewType.ShaderResource));
        }
        
        var srb = srbCache.Build(pipeline, resourceMapping);
        instancedBatchSys.SetPipelineState(batch.Id, pipeline);
        instancedBatchSys.SetShaderResourceBinding(batch.Id, srb);
        instancedBatchSys.RemoveDirtyState(batch.Id);
        
        batch.Unlock();
    }
    
    private ICommandBuffer? pCommandBuffer;
    protected override void OnExecute(ICommandBuffer command)
    {
        pCommandBuffer = command;

		instancedBatchSys.UpdateTransforms();
		instancedBatchSys.ForEach(SetupInstanceSpriteBatch);

		var backbuffer = GetBackBuffer();
        var depthbuffer = GetDepthBuffer();

        if (backbuffer is null || depthbuffer is null)
            return;

        command.SetRT(backbuffer, depthbuffer);

        foreach (var batch in pBatchGroup)
            batch.Render(command);
        
        instancedBatchSys.ForEach(ExecuteSpriteInstanceBatch);
    }

    private unsafe void ExecuteSpriteInstanceBatch(SpriteInstance batch)
    {
        if (pCommandBuffer is null)
            return;
        
        batch.Lock();
        
        var enabled = batch.Enabled;
        var color = batch.Color;
        var pipeline = batch.PipelineState;
        var srb = batch.ShaderResourceBinding;
        var numInstances = batch.InstanceCount;

        var instancingBuffer = instancedBatchSys.GetInstancingBuffer(batch.Id);
        if (instancingBuffer.Desc.Usage == Usage.Dynamic)
        {
            instancedBatchSys.GetTransforms(batch.Id, out var transforms);
            var ptr = pCommandBuffer.Map(instancingBuffer, MapType.Write, MapFlags.Discard);
            var size = (ulong)(Unsafe.SizeOf<Matrix3x3>() * numInstances);
            fixed (Matrix3x3* transformsPtr = transforms)
                Buffer.MemoryCopy(transformsPtr, ptr.ToPointer(), size, size);
            pCommandBuffer.Unmap(instancingBuffer, MapType.Write);
        }
        else if (instancedBatchSys.IsDirtyInstances(batch.Id))
        {
            instancedBatchSys.GetTransforms(batch.Id, out var transforms);
            pCommandBuffer.UpdateBuffer(instancingBuffer, 0, new ReadOnlySpan<Matrix3x3>(transforms));
            instancedBatchSys.ClearTransforms(batch.InstanceCount); // Free memory
            instancedBatchSys.RemoveDirtyInstancesState(batch.Id);
        }

        batch.Unlock();

        if (!enabled)
            return;
        
        var objectBuffer = bufferMgr.GetBuffer(BufferGroupType.Object);
        var mappedData = pCommandBuffer.Map<Vector4>(objectBuffer, MapType.Write, MapFlags.Discard);
        mappedData[0] = color.ToVector4();
        pCommandBuffer.Unmap(objectBuffer, MapType.Write);

        pCommandBuffer
            .SetVertexBuffer(instancingBuffer, true)
            .SetPipeline(pipeline)
            .CommitBindings(srb)
            .Draw(new DrawArgs
            {
                NumVertices = 4,      
                NumInstances = (uint)numInstances
            });
    }
    
    private static IPipelineState CreatePipeline(in RenderFeatureSetupInfo setupInfo, bool hasTexture, bool instanced)
    {
        GraphicsPipelineDesc desc = new()
        {
            Name = "Sprite PSO"
        };
        if (instanced)
            desc.Name = "[Instanced]" + desc.Name;
        if (hasTexture)
            desc.Name = "[Textured]" + desc.Name;

        desc.Output.RenderTargetFormats[0] = setupInfo.GraphicsSettings.DefaultColorFormat;
        desc.Output.DepthStencilFormat = setupInfo.GraphicsSettings.DefaultDepthFormat;
        desc.BlendState.BlendMode = BlendMode.Alpha;
        desc.PrimitiveType = PrimitiveType.TriangleStrip;
        desc.RasterizerState.CullMode = CullMode.Both;
        desc.DepthStencilState.EnableDepth = true;

        desc.Shaders.VertexShader = CreateShader(setupInfo, hasTexture, instanced, ShaderType.Vertex);
        desc.Shaders.PixelShader = CreateShader(setupInfo, hasTexture, instanced, ShaderType.Pixel);

        if (instanced)
            for (var i = 0u; i < 3; ++i)
            {
                desc.InputLayouts.Add(
                    new PipelineInputLayoutElementDesc
                    {
                        InputIndex = i,
                        Input = new InputLayoutElementDesc
                        {
                            BufferIndex = 0,
                            ElementType = ElementType.Vector3,
                            InstanceStepRate = 1
                        }
                    }
                );
            }
        
        if(hasTexture)
            desc.Samplers.Add(new ImmutableSamplerDesc
            {
                Name = TextureNames.MainTexture,
                Sampler = new SamplerStateDesc(TextureFilterMode.Trilinear, TextureAddressMode.Clamp)
            });

        return setupInfo.PipelineStateManager.GetOrCreate(desc);
    }
    private static IShader CreateShader(in RenderFeatureSetupInfo setupInfo, bool hasTexture, bool instanced, ShaderType shaderType)
    {
        var shaderCi = new ShaderCreateInfo()
        {
            Type = shaderType
        };
        string assetPath;
        switch (shaderType)
        {
            case ShaderType.Vertex:
            {
                shaderCi.Name = "Sprite Vertex Shader";
                assetPath = "Shaders/spritebatch_vs.hlsl";
                if (instanced)
                    assetPath = "Shaders/spritebatch_instanced_vs.hlsl";
            }
                break;
            case ShaderType.Pixel:
            {
                shaderCi.Name = "Sprite Pixel Shader";
                assetPath = "Shaders/spritebatch_ps.hlsl";
            }
                break;
            case ShaderType.Compute:
            case ShaderType.Geometry:
            case ShaderType.Hull:
            case ShaderType.Domain:
            default:
                throw new NotImplementedException();
        }

        shaderCi.SourceCode = setupInfo.AssetManager.GetAsset<ShaderAsset>(assetPath).ShaderCode;
        if (hasTexture)
        {
            shaderCi.Name = "[TEXTURED]"+shaderCi.Name;
            shaderCi.Macros.Add("RENGINE_ENABLED_TEXTURE", "1");
        }

        if (!instanced) return setupInfo.ShaderManager.GetOrCreate(shaderCi);
        
        shaderCi.Name = "[INSTANCED]" + shaderCi.Name;
        shaderCi.Macros.Add("RENGINE_INSTANCED", "1");

        return setupInfo.ShaderManager.GetOrCreate(shaderCi);
    }
}