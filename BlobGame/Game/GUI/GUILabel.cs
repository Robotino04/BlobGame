﻿using BlobGame.Drawing;
using BlobGame.ResourceHandling;
using BlobGame.ResourceHandling.Resources;
using BlobGame.Util;
using ZeroElectric.Vinculum;
using System.Numerics;

namespace BlobGame.Game.Gui;

internal enum eTextAlignment { Left, Center, Right }

internal sealed class GuiLabel : GuiElement {
    private string _Text { get; set; }
    public string Text {
        get => _Text;
        set {
            _Text = value ?? string.Empty;
            CalculateTextPosition();
        }
    }

    private eTextAlignment _TextAlignment { get; set; }
    public eTextAlignment TextAlignment {
        get => _TextAlignment;
        set {
            _TextAlignment = value;
            CalculateTextPosition();
        }
    }

    public ColorResource Color { get; set; }

    public bool DrawOutline { get; set; }
    public ColorResource OutlineColor { get; set; }

    private float FontSize { get; }
    private float FontSpacing { get; }

    private Vector2 TextPosition { get; set; }

    private Font LastUsedFont { get; set; }

    public GuiLabel(string boundsString, string text, Vector2? pivot = null)
        : this(GuiBoundsParser.Parse(boundsString), text, pivot) {
    }

    public GuiLabel(Rectangle bounds, string text, Vector2? pivot = null)
        : this(bounds.X, bounds.Y, bounds.width, bounds.height, text, pivot) {
    }

    public GuiLabel(float x, float y, float w, float h, string text, Vector2? pivot = null)
        : base(x, y, w, h, pivot) {

        _Text = text;
        FontSize = h * 0.6f;
        FontSpacing = FontSize / 64f;
        TextAlignment = eTextAlignment.Center;
        Color = ColorResource.WHITE;

        DrawOutline = false;
        OutlineColor = ResourceManager.ColorLoader.Get("outline");
    }

    protected override void DrawInternal() {
        if (!LastUsedFont.Equals(Renderer.GuiFont.Resource))
            CalculateTextPosition();
        LastUsedFont = Renderer.GuiFont.Resource;


        if (DrawOutline) {
            Vector2 textSize = GetTextSize();
            Vector2 outlineSize = Raylib.MeasureTextEx(Renderer.GuiFont.Resource, Text, FontSize * 1.05f, FontSpacing);
            Vector2 outlineOffset = new Vector2(
                (outlineSize.X - textSize.X) / 2f,
                (outlineSize.Y - textSize.Y) / 2f
                );

            Raylib.DrawTextEx(Renderer.GuiFont.Resource, Text, TextPosition - outlineOffset, FontSize * 1.05f, FontSpacing, OutlineColor.Resource);
        }

        Raylib.DrawTextEx(Renderer.GuiFont.Resource, Text, TextPosition, FontSize, FontSpacing, Color.Resource);
    }

    internal Vector2 GetTextSize() {
        return Raylib.MeasureTextEx(Renderer.GuiFont.Resource, Text, FontSize, FontSpacing);
    }

    private void CalculateTextPosition() {
        Vector2 textSize = GetTextSize();
        switch (_TextAlignment) {
            case eTextAlignment.Center:
                TextPosition = new Vector2(Bounds.X + (Bounds.width - textSize.X) / 2f, Bounds.Y + (Bounds.height - FontSize) / 2f);
                break;
            case eTextAlignment.Left:
                TextPosition = new Vector2(Bounds.X, Bounds.Y + (Bounds.height - FontSize) / 2f);
                break;
            case eTextAlignment.Right:
                TextPosition = new Vector2(Bounds.x + Bounds.width - textSize.X, Bounds.Y + (Bounds.height - FontSize) / 2f);
                break;
        }
    }
}
