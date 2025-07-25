﻿using BlobGame.App;
using BlobGame.Game.GameModes;
using BlobGame.Game.Scenes;
using ZeroElectric.Vinculum;
using System.Numerics;

namespace BlobGame.Game.GameControllers;

internal class MouseController : IGameController {
    public MouseController() {
    }

    public void Load() {
        Input.RegisterHotkey("drop_piece", MouseButton.MOUSE_BUTTON_LEFT);
        Input.RegisterHotkey("hold_piece", MouseButton.MOUSE_BUTTON_MIDDLE);
    }

    /// <summary>
    /// Retrieves the current value of t, which represents the position of the dropper above the arena.
    /// </summary>
    /// <returns>The current value of t.</returns>
    public float GetCurrentT() {
        Vector2 mPos = GameScene.ScreenToArenaPosition(Raylib.GetMousePosition());
        float t = mPos.X / IGameMode.ARENA_WIDTH;
        return t;
    }

    /// <summary>
    /// Attempts to spawn a blob in the provided game simulation.
    /// </summary>
    /// <param name="simulation">The game simulation in which to spawn the blob.</param>
    /// <param name="t">The t value at which the blob is spawned, which represents the position of the dropper above the arena..</param>
    /// <returns>True if blob spawning was attempted, otherwise false.</returns>
    public bool SpawnBlob(IGameMode simulation, out float t) {
        t = GetCurrentT();

        if (!simulation.CanSpawnBlob)
            return false;

        if (!Input.IsHotkeyActive("drop_piece"))
            return false;

        Input.WasMouseHandled[MouseButton.MOUSE_BUTTON_LEFT] = true;

        return true;
    }

    public bool HoldBlob() {
        if (!Input.IsHotkeyActive("hold_piece"))
            return false;

        Input.WasMouseHandled[MouseButton.MOUSE_BUTTON_MIDDLE] = true;

        return true;
    }

    public void Update(float dT, IGameMode simulation) { }

    public void Close() {
        Input.UnregisterHotkey("drop_piece");
        Input.UnregisterHotkey("hold_piece");
    }
}
