﻿using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using ZeroElectric.Vinculum;

namespace BlobGame.Game;
/// <summary>
/// Class for the arena's walls.
/// </summary>
internal sealed class Wall : GameObject {
    /// <summary>
    /// The physics engine fixtures attached to the wall's body.
    /// </summary>
    protected override List<Fixture> Fixtures { get; }

    /// <summary>
    /// The width of the wall.
    /// </summary>
    private float Width { get; }
    /// <summary>
    /// The height of the wall.
    /// </summary>
    private float Height { get; }

    /// <summary>
    /// Creates a new wall with the given parameters.
    /// </summary>
    /// <param name="name">The name of wall</param>
    /// <param name="world">The world to add the physics engine body to.</param>
    /// <param name="bounds">The bounds of the wall. Used to position and scale it.</param>
    public Wall(string name, World world, Rectangle bounds)
        : base(name, world, new Vector2(bounds.x, bounds.y), 0, BodyType.Static) {
        Width = bounds.width;
        Height = bounds.height;

        Fixtures = new List<Fixture>(){
            Body.CreateRectangle(Width / POSITION_MULTIPLIER, Height / POSITION_MULTIPLIER, 1, new Vector2(Width / POSITION_MULTIPLIER / 2, Height / POSITION_MULTIPLIER / 2))
        };
        Fixtures[0].Restitution = 0;
        Fixtures[0].Friction = 0.15f;
        ZIndex = 0;
    }

    /// <summary>
    /// Custom wall drawing logic.
    /// </summary>
    protected internal override void DrawInternal() {
        if (Application.Instance.DrawDebug)
            Raylib.DrawRectangleLinesEx(new Rectangle(0, 0, Width, Height), 2, Raylib.WHITE);
    }
}
