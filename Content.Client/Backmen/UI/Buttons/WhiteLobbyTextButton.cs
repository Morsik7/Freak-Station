using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = Robust.Shared.Maths.Color;

namespace Content.Client.Backmen.UI.Buttons;

[Virtual]
public class WhiteLobbyTextButton : TextureButton
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private readonly Font _font;
    private string _buttonText = string.Empty;

    [DataField]
    public string ButtonText
    {
        get => _buttonText;
        set
        {
            _buttonText = value;
            RebuildTexture();
        }
    }

    // Compatibility with code that expects Button.Text on lobby buttons.
    public string Text
    {
        get => ButtonText;
        set => ButtonText = value;
    }

    public WhiteLobbyTextButton()
    {
        IoCManager.InjectDependencies(this);
        _font = new VectorFont(_resourceCache.GetResource<FontResource>("/Fonts/Bedstead/Bedstead.otf"), 15);
    }

    private void RebuildTexture()
    {
        if (string.IsNullOrEmpty(_buttonText))
            return;

        var size = MeasureText(_font, _buttonText);
        var image = new Image<Rgba32>((int) size.X, (int) size.Y);
        image[0, 0] = new Rgba32(0, 0, 0, 0);
        TextureNormal = Texture.LoadFromImage(image);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        if (string.IsNullOrEmpty(_buttonText))
            return;

        switch (DrawMode)
        {
            case DrawModeEnum.Normal:
                DrawText(handle, ToggleMode ? Color.Red : Color.White);
                break;
            case DrawModeEnum.Pressed:
                DrawText(handle, Pressed ? Color.Green : Color.Red);
                break;
            case DrawModeEnum.Hover:
                DrawText(handle, Color.Yellow);
                break;
            case DrawModeEnum.Disabled:
                DrawText(handle, Color.Gray);
                break;
        }
    }

    private void DrawText(DrawingHandleScreen handle, Color color)
    {
        RebuildTexture();
        handle.DrawString(_font, Vector2.Zero, _buttonText, color);
    }

    private static Vector2 MeasureText(Font font, string text)
    {
        var textSize = font.GetHeight(0.9f);
        var width = textSize * text.Length / 1.5f;
        return new Vector2(width, textSize);
    }
}
