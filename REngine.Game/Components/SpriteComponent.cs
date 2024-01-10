using System.Drawing;
using System.Numerics;
using REngine.Core.Events;
using REngine.Core.WorldManagement;
using REngine.RPI;

namespace REngine.Game.Components;

public sealed class SpriteSerializer(
    IServiceProvider serviceProvider,
    SpriteSystem spriteSystem) : ComponentSerializer<SpriteComponent>(serviceProvider)
{
    private struct SerializeData
    {
        public Vector2 Anchor;
        public Color Color;
    }
    public override Component Create() => spriteSystem.Create();
    public override Type GetSerializeType() => typeof(SerializeData);
    public override object OnSerialize(Component component)
    {
        if (component is not SpriteComponent sprite)
            throw new InvalidCastException($"Expected '{nameof(SpriteComponent)}");
        return new SerializeData
        {
            Anchor = sprite.Anchor,
            Color = sprite.Color
        };
    }

    public override Component OnDeserialize(object componentData)
    {
        var data = (SerializeData)componentData;
        var component = spriteSystem.Create();
        component.Anchor = data.Anchor;
        component.Color = data.Color;
        return component;
    }
}

[ComponentSerializer(typeof(SpriteSerializer))]
public sealed class SpriteComponent(int id, SpriteSystem system) : Component
{
    public int Id => id;
    public Transform2D Transform => system.GetTransform(id);

    public SpriteEffect? Effect
    {
        get => system.GetEffect(id);
        set => system.SetEffect(id, value);
    }

    public Vector2 Anchor
    {
        get => system.GetAnchor(id);
        set => system.SetAnchor(id, value);
    }

    public Color Color
    {
        get => system.GetColor(id);
        set => system.SetColor(id, value);
    }
    
    public EventQueue ClickEvent => system.GetClickEvent(id);
    public EventQueue MouseEnterEvent => system.GetMouseEnterEvent(id);
    public EventQueue MouseExitEvent => system.GetMouseExitEvent(id);
    protected override void OnDispose() => system.Destroy(this);
}