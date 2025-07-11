using BlobGame.Drawing;
using BlobGame.ResourceHandling.Resources;
using ZeroElectric.Vinculum;
using System.Numerics;

namespace BlobGame.Game.Gui;
internal sealed class GuiDynamicLabel : GuiElement {
    public string Text { get; set; }

    public ColorResource Color { get; set; }

    private float FontSize { get; }
    private float FontSpacing { get; }

    private Vector2 TextPosition { get; }

    public GuiDynamicLabel(float x, float y, string text, float fontSize, Vector2? pivot = null)
        : base(x, y,
            CalculateTextSize(fontSize, text).X,
            CalculateTextSize(fontSize, text).Y,
            pivot) {

        Text = text;
        FontSize = fontSize;
        FontSpacing = FontSize / 64f;
        TextPosition = new Vector2(x, y);
        Color = ColorResource.WHITE;
    }

    protected override void DrawInternal() {
        RenderUtils.DrawTextblock(Text, TextPosition, FontSize, Color.Resource, Renderer.GuiFont.Resource, FontSpacing);
    }

    public static Vector2 CalculateTextSize(float fontSize, string text) {
        //float fontSpacing = fontSize / 32f;
        string[] lines = text.Split('\n');
        float height = Math.Max(lines.Length - 1, 0) * 1.5f * fontSize + fontSize; // the last line doesn't need spacing
        float width = 0;
        for (int i = 0; i < lines.Length; i++) {
            Vector2 r = Raylib.MeasureTextEx(Renderer.GuiFont.Resource, lines[i], fontSize, 0); // TODO: it is rendered with 0 for now, probably fix eventually
            width = Math.Max(width, r.X);
        }

        return new Vector2(width, height);
    }

}
