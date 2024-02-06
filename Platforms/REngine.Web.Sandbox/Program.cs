using System.Drawing;
using System.Text;
using REngine.Core.Web;
using REngine.RHI;
using REngine.RHI.Web.Driver;

namespace REngine.Web.Sandbox;

public class Program
{
    private static void OnMessageEvent(MessageEventData eventData)
    {
        StringBuilder log = new StringBuilder();
        log.AppendLine("[REngine][Driver]:");
        log.AppendLine($"\tSeverity: {eventData.Severity}");
        log.AppendLine($"\tMessage: {eventData.Message}");
        log.AppendLine($"\tFunction: {eventData.Function}");
        log.AppendLine($"\tFile: {eventData.File}");
        log.Append($"\tLine: {eventData.Line}");
        
        WebConsole.Log(log);
    }

    public static async Task Main()
    {
        Console.WriteLine("[REngine]: Initializing Driver");
    
        var (driver, swapChain) = DriverFactory.Build(new DriverFactoryCreateInfo()
        {
            CanvasSelector = "#canvas",
            MessageEvent = OnMessageEvent,
        });

        var vertexShader = driver.Device.CreateShader(new ShaderCreateInfo()
        {
            Name = "Vertex Shader",
            SourceCode = @"
                struct PSInput {
                    float4 pos : SV_POSITION;
                    float3 color : COLOR;
                };
                void main(in uint vertId : SV_VertexID, out PSInput vs_input) {
                    float4 pos[3];
                    pos[0] = float4(-0.5, -0.5, 0.0, 1.0);
                    pos[1] = float4(+0.0, +0.5, 0.0, 1.0);
                    pos[2] = float4(+0.5, -0.5, 0.0, 1.0);
                    
                    float3 col[3];
                    col[0] = float3(1.0, 0.0, 0.0);
                    col[1] = float3(0.0, 1.0, 0.0);
                    col[2] = float3(0.0, 0.0, 1.0);

                    vs_input.pos = pos[vertId];
                    vs_input.color = col[vertId];
                }
            ",
            Type = ShaderType.Vertex
        });
        var pixelShader = driver.Device.CreateShader(new ShaderCreateInfo()
        {
            Name = "Pixel Shader",
            SourceCode = @"
                struct PSInput {
                    float4 pos : SV_POSITION;
                    float3 color : COLOR;
                };
                struct PSOutput {
                    float4 color : SV_TARGET;
                };

                void main(in PSInput vs_input, out PSOutput ps_output) {
                    ps_output.color = float4(vs_input.color.rgb, 1.0);
                }
            ",
            Type = ShaderType.Pixel
        });

        var pipelineDesc = new GraphicsPipelineDesc();
        pipelineDesc.Name = "Triangle PSO";
        pipelineDesc.Shaders.VertexShader = vertexShader;
        pipelineDesc.Shaders.PixelShader = pixelShader;
        pipelineDesc.Output.RenderTargetFormats = [swapChain.Desc.Formats.Color];
        pipelineDesc.Output.DepthStencilFormat = swapChain.Desc.Formats.Depth;
        pipelineDesc.PrimitiveType = PrimitiveType.TriangleList;
        pipelineDesc.RasterizerState.CullMode = CullMode.Both;
        pipelineDesc.DepthStencilState.EnableDepth = false;

        var pipelineState = driver.Device.CreateGraphicsPipeline(pipelineDesc);
        vertexShader.Dispose();
        pixelShader.Dispose();
        
        var looper = Looper.Build(() =>
        {
            driver.ImmediateCommand
                .SetRT(swapChain.ColorBuffer, swapChain.DepthBuffer)
                .ClearRT(swapChain.ColorBuffer, Color.Black)
                .ClearDepth(swapChain.DepthBuffer, ClearDepthStencil.Depth, 1.0f, 0)
                .SetPipeline(pipelineState)
                .Draw(new DrawArgs()
                {
                    NumInstances = 1,
                    NumVertices = 3
                });
        });

        Console.WriteLine("[REngine]: Finished");
    }
}