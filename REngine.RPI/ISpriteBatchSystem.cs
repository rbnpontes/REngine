using System.Numerics;
using REngine.RHI;

namespace REngine.RPI;

public interface ISpriteBatchSystem
{
    public SpriteBatch Allocate();
    public void Destroy(int id);
    public object GetObjectSync(int id);
    public Vector3 GetPosition(int id);
    public Vector2 GetAnchor(int id);
    public Vector2 GetOffset(int id);
    public Vector2 GetSize(int id);
    public float GetAngle(int id);
    public ITexture? GetTexture(int id);
    public IShaderResourceBinding? GetShaderResourceBinding(int id);
    public IPipelineState? GetPipelineState(int id);
    public void SetPosition(int id, Vector3 position);
    public void SetAnchor(int id, Vector2 anchor);
    public void SetOffset(int id, Vector2 offset);
    public void SetSize(int id, Vector2 size);
    public void SetAngle(int id, float angle);
    public void SetTexture(int id, ITexture texture);
}