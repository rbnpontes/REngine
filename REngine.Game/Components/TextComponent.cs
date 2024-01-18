using System.Drawing;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;
using REngine.RPI.Effects;

namespace REngine.Game.Components;

public sealed class TextSerializer(
    IServiceProvider provider,
    TextSystem system) : ComponentSerializer<TextComponent>(provider)
{
    struct SerializeData
    {
        public Color Color;
        public bool IsDynamic;
        public string Text;
        public float VerticalSpacing;
        public float HorizontalSpacing;
        public string FontName;
    }

    public override Component Create() => new TextComponent(system);

    public override Type GetSerializeType() => typeof(SerializeData);
    public override object OnSerialize(Component component)
    {
        if (component is not TextComponent text)
            throw new InvalidCastException($"Expected '{nameof(TextComponent)}");
        return new SerializeData()
        {
            Color = text.Color,
            IsDynamic = text.IsDynamic,
            Text = text.Text,
            VerticalSpacing = text.VerticalSpacing,
            HorizontalSpacing = text.HorizontalSpacing,
            FontName = text.FontName
        };
    }

    public override Component OnDeserialize(object componentData)
    {
        var data = (SerializeData)componentData;
        var component = new TextComponent(system);
        component.Color = data.Color;
        component.IsDynamic = data.IsDynamic;
        component.Text = data.Text;
        component.VerticalSpacing = data.VerticalSpacing;
        component.HorizontalSpacing = data.HorizontalSpacing;
        component.FontName = data.FontName;
        return component;
    }
}

[ComponentSerializer(typeof(TextSerializer))]
public sealed class TextComponent : Component
{
    private readonly TextSystem pTextSystem;
    public int Id { get; }

    public Transform2D Transform
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pTextSystem.GetTransform(Id);
        }
    }

    public TextEffect? Effect
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pTextSystem.GetEffect(Id);
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            pTextSystem.SetEffect(Id, value);
        }
    }

    public bool IsDynamic
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pTextSystem.IsDynamic(Id);
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            pTextSystem.SetIsDynamic(Id, value);
        }
    }

    public string Text
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pTextSystem.GetText(Id);
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            pTextSystem.SetText(Id, value);
        }
    }

    public float VerticalSpacing
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pTextSystem.GetVerticalSpacing(Id);
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            pTextSystem.SetVerticalSpacing(Id, value);
        }
    }

    public float HorizontalSpacing
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pTextSystem.GetHorizontalSpacing(Id);
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            pTextSystem.SetHorizontalSpacing(Id, value);
        }
    }

    public Color Color
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pTextSystem.GetColor(Id);
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            pTextSystem.SetColor(Id, value);
        }
    }

    public string FontName
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pTextSystem.GetFontName(Id);
        }
        set
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            pTextSystem.SetFontName(Id, value);
        }
    }

    public RectangleF Bounds
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return pTextSystem.GetBounds(Id);
        }
    }

    public Font Font
    {
        set => SetFont(value);
    }
    
    public TextComponent(
        TextSystem textSystem
    )
    {
        pTextSystem = textSystem;
        Id = textSystem.Create(this);
    }

    public void SetFont(Font font)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        pTextSystem.SetFont(Id, font);
    }
    protected override void OnDispose()
    {
        pTextSystem.Destroy(this);
    }
}