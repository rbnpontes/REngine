using REngine.Core.WorldManagement;
using REngine.RPI;
using REngine.RPI.Batches;

namespace REngine.Game.Components;

public sealed class SpriteInstanceSerializer(
    IServiceProvider serviceProvider,
    SpriteInstanceSystem instanceSystem) :
    ComponentSerializer<SpriteInstanceComponent>(serviceProvider)
{
    private struct SerializeData
    {
        public SpriteInstanceType InstanceType;
        public SpriteBatchItem[] Items;
    }

    public override Component Create() => new SpriteInstanceComponent(instanceSystem);
    public override Type GetSerializeType() => typeof(SerializeData);
    public override object OnSerialize(Component component)
    {
        if (component is not SpriteInstanceComponent sprite)
            throw new InvalidCastException($"Expected '{nameof(SpriteInstanceComponent)}");
        return new SerializeData()
        {
            InstanceType = sprite.InstanceType,
            Items = sprite.Items
        };
    }

    public override Component OnDeserialize(object componentData)
    {
        var data = (SerializeData)componentData;
        var component = new SpriteInstanceComponent(instanceSystem);
        component.InstanceType = data.InstanceType;
        component.Items = data.Items;
        return component;
    }
}

[ComponentSerializer(typeof(SpriteInstanceSerializer))]
public sealed class SpriteInstanceComponent : Component
{
    private readonly SpriteInstanceSystem pSystem;
    public int Id { get; private set; }

    public Transform2D Transform
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pSystem.GetTransform(Id);
        }
    }
    public SpriteEffect? Effect
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pSystem.GetEffect(Id);
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            pSystem.SetEffect(Id, value);
        }
    }
    public SpriteInstanceType InstanceType
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pSystem.GetInstanceType(Id);
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            pSystem.SetInstanceType(Id, value);
        }
    }
    public SpriteBatchItem[] Items
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pSystem.GetItems(Id);
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            pSystem.SetItems(Id, value);
        }
    }

    public SpriteInstanceComponent(SpriteInstanceSystem system)
    {
        pSystem = system;
        Id = system.Create(this);
    }
    
    protected override void OnDispose()
    {
        pSystem.Destroy(Id);
    }
}