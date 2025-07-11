using System.Numerics;
using ZeroElectric.Vinculum;

namespace BlobGame.Game.Gui;
public static class RenderUtils {
    public static void DrawTextblock(string text, Vector2 position, float fontSize, Color color, Font font, float fontSpacing = float.NaN) {
        if (float.IsNaN(fontSpacing))
            fontSpacing = fontSize;

        string[] lines = text.Split('\n');

        for (int i = 0; i < lines.Length; i++) {
            Raylib.DrawTextPro(font, lines[i], position + new Vector2(0, i * 1.5f * fontSize), Vector2.Zero, 0, fontSize, fontSpacing, color);
        }
    }

}
