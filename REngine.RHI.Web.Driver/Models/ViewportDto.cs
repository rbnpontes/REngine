using System.Runtime.InteropServices;

namespace REngine.RHI.Web.Driver.Models;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal unsafe struct ViewportDto
{
    public float TopLeftX;
    public float TopLeftY;

    public float Width;
    public float Height;
    public float MinDepth;
    public float MaxDepth;

    public ViewportDto()
    {
        this = default(ViewportDto);
        MaxDepth = 1;
    }

    public ViewportDto(Viewport viewport)
    {
        TopLeftX = viewport.Position.X;
        TopLeftY = viewport.Position.Y;
        
        Width = viewport.Size.X;
        Height = viewport.Size.Y;
        
        MinDepth = viewport.MinDepth;
        MaxDepth = viewport.MaxDepth;
    }

    public ref ViewportDto GetPinnableReference()
    {
        return ref this;
    }
}