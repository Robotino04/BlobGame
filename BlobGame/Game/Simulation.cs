﻿using BlobGame.Game.Blobs;
using BlobGame.Game.GameObjects;
using BlobGame.Game.Util;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using Raylib_CsLo;

namespace BlobGame.Game;

/// <summary>
/// Interface which exposes the game's simulation logic to allow outside control and getting the state of the game.
/// </summary>
public interface ISimulation {
    /// <summary>
    /// The game's game objects. This includes blobs and walls.
    /// </summary>
    IReadOnlyGameObjectsCollection GameObjects { get; }
    /// <summary>
    /// The type of the currently spawned blob.
    /// </summary>
    eBlobType CurrentBlob { get; }
    /// <summary>
    /// The type of the next blob to be spawned.
    /// </summary>
    eBlobType NextBlob { get; }
    /// <summary>
    /// Wether or not the player can currently spawn a blob. This is false when the last spawned blob is still falling.
    /// </summary>
    bool CanSpawnBlob { get; }
    /// <summary>
    /// The current score of the game
    /// </summary>
    int Score { get; }
    /// <summary>
    /// Flag indicating wether or not the game is over. The game is over when a blob "overfills" the arena.
    /// </summary>
    bool IsGameOver { get; }
}

/// <summary>
/// The game's simulation. Handles the game's logic without any drawing logic.
/// It is separated from the drawing logic to allow for a headless version of the game. *wink wink* Tutel *wink wink*
/// </summary>
internal sealed class Simulation : ISimulation {
    /// <summary>
    /// Constants derived from the original game. Manually figured out and hard coded.
    /// </summary>
    internal const float GRAVITY = 111.3f;
    internal const float ARENA_WIDTH = 670;
    internal const float ARENA_HEIGHT = 846;
    internal const float ARENA_HEIGHT_LOWER = 750;
    internal const float ARENA_WALL_THICKNESS = 20;
    internal const float ARENA_SPAWN_Y_OFFSET = -22.5f;
    internal const int HIGHEST_SPAWNABLE_BLOB_INDEX = 4;

    /// <summary>
    /// Collection of all game objects. This includes blobs and walls.
    /// </summary>
    private GameObjectCollection _GameObjects { get; }
    /// <summary>
    /// Public read-only accesor for the game objects collection.
    /// </summary>
    public IReadOnlyGameObjectsCollection GameObjects => _GameObjects;

    /// <summary>
    /// The wall game object representing the arena ground.
    /// </summary>
    private Wall GroundWall { get; set; }

    /// <summary>
    /// Random generator used for determining the spawned blob types.
    /// </summary>
    private Random Random { get; }
    /// <summary>
    /// The physics world used for the simulation.
    /// </summary>
    private World World { get; }

    /// <summary>
    /// The type of the currently spawned blob.
    /// </summary>
    public eBlobType CurrentBlob { get; private set; }
    /// <summary>
    /// The type of the next blob to be spawned.
    /// </summary>
    public eBlobType NextBlob { get; private set; }
    /// <summary>
    /// Wether or not the player can currently spawn a blob. This is false when the last spawned blob is still falling.
    /// </summary>
    public bool CanSpawnBlob { get; private set; }

    /// <summary>
    /// The current score of the game
    /// </summary>
    public int Score { get; private set; }
    /// <summary>
    /// Flag indicating wether or not the game is over. The game is over when a blob "overfills" the arena.
    /// </summary>
    public bool IsGameOver { get; private set; }

    /// <summary>
    /// Since each collision occurs twice (with swapped fixtures) this keeps track of which pairs were already handled. Is reset every physics tick.
    /// </summary>
    private List<(Guid id1, Guid id2)> HandledCollisionPairs { get; }
    /// <summary>
    /// List of blobs to be added to the game. This is used to avoid modifying the game objects collection while iterating over it.
    /// </summary>
    private List<(eBlobType type, Vector2 position)> ToBeAdded { get; }
    /// <summary>
    /// List of blobs to be removed from the game. This is used to avoid modifying the game objects collection while iterating over it.
    /// </summary>
    private List<Blob> ToBeRemoved { get; }
    /// <summary>
    /// Keeps track of the last spawned blob. This is used to determine wether or not the player can spawn a new blob.
    /// </summary>
    private Blob? LastSpawned { get; set; }

    /// <summary>
    /// Creates a new simulation with the given seed.
    /// </summary>
    /// <param name="seed">The seed used to initialize the random blob generator.</param>
    internal Simulation(int seed) {
        _GameObjects = new GameObjectCollection();

        Random = new Random(seed);
        World = new World(new Vector2(0, GRAVITY));

        HandledCollisionPairs = new List<(Guid, Guid)>();
        ToBeAdded = new List<(eBlobType, Vector2 position)>();
        ToBeRemoved = new List<Blob>();
        LastSpawned = null;

        Score = 0;
        IsGameOver = false;
    }

    /// <summary>
    /// Initializes the simulation. Creates the arena walls physics bodies. Determines the first blob types to be spawned.
    /// </summary>
    internal void Load() {
        CurrentBlob = GenerateRandomBlobType();
        NextBlob = GenerateRandomBlobType();
        _GameObjects.AddRange(CreateArena(World));
        GroundWall = (GameObjects.FindByName("Ground") as Wall)!;

        CanSpawnBlob = true;
    }

    /// <summary>
    /// Used to update the game simulation. Is called every frame. Simulates the physics, handles game object adding and removing and check for game over conditions.
    /// </summary>
    /// <param name="dT"></param>
    internal void Update(float dT) {
        if (IsGameOver)
            return;

        // Each tick is split into 50 smaller ticks to make the physics more stable.
        // 50 sub ticks is the sweet spot where the game feels imilar to the original.
        for (int i = 0; i < 50; i++) {
            World.Step(dT / 50f);

            HandledCollisionPairs.Clear();
            foreach (Blob blob in ToBeRemoved)
                RemoveBlob(blob);
            ToBeRemoved.Clear();

            foreach ((eBlobType type, Vector2 position) blob in ToBeAdded)
                CreateBlob(blob.position, blob.type);
            ToBeAdded.Clear();
        }

        CheckGameOver();
    }

    /// <summary>
    /// Attempts to spawn a blob. Returns true if a blob was spawned, false otherwise.
    /// </summary>
    /// <param name="t">The t value where to spawn the blob above the arena. 0 is all the way left, 1 all the way right.</param>
    /// <param name="blob"></param>
    /// <returns></returns>
    internal bool TrySpawnBlob(float t, out Blob? blob) {
        blob = null;
        if (!CanSpawnBlob || IsGameOver)
            return false;

        eBlobType type = CurrentBlob;

        CurrentBlob = NextBlob;
        NextBlob = GenerateRandomBlobType();

        float x = (t - 0.5f) * ARENA_WIDTH;
        float y = ARENA_SPAWN_Y_OFFSET;

        blob = CreateBlob(new Vector2(x, y), type);
        LastSpawned = blob;
        CanSpawnBlob = false;
        return true;
    }

    /// <summary>
    /// Creates a new blob of the given type at the given position and adds it to the game.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private Blob CreateBlob(Vector2 position, eBlobType type) {
        Blob blob = Blob.Create(World, position, type);
        _GameObjects.Add(blob);
        blob.Body.OnCollision += OnBlobCollision;

        return blob;
    }

    /// <summary>
    /// Removes a blob from the game.
    /// </summary>
    /// <param name="blob"></param>
    private void RemoveBlob(Blob blob) {
        if (!GameObjects.Contains(blob))
            return;

        _GameObjects.Remove(blob);
        blob.Body.OnCollision -= OnBlobCollision;
        World.Remove(blob.Body);
    }

    /// <summary>
    /// Checks if the game's game over condition(s) are met and sets the game over flag accordingly.
    /// </summary>
    private void CheckGameOver() {
        GameObjects.Enumerate(gO => {
            bool tmp = gO is Blob blob && blob.Position.Y <= 0 && blob != LastSpawned;
            IsGameOver |= tmp;
        });
    }

    /// <summary>
    /// Callback from the physics engine to handle collisions between blobs and/or walls.
    /// </summary>
    /// <param name="sender">The object initializing the collision.</param>
    /// <param name="other">The other object involved in the collision.</param>
    /// <param name="contact">The contact data of the collision.</param>
    /// <returns>???? No idea. Everyone uses true everywhere. Can't find any docs for it either.</returns>
    private bool OnBlobCollision(Fixture sender, Fixture other, Contact contact) {
        if (IsGameOver)
            return true;

        CheckForBlobSpawnReenable(sender, other);

        Blob? senderBlob = sender.Body.Tag as Blob;
        Blob? otherBlob = other.Body.Tag as Blob;

        if (senderBlob == null || otherBlob == null)
            return true;

        if (senderBlob.Type != otherBlob.Type || senderBlob.Type == eBlobType.Watermelon)
            return true;

        if (HandledCollisionPairs.Contains((otherBlob.Id, senderBlob.Id)))
            return true;

        Score += senderBlob.Score;
        Vector2 midPoint = (senderBlob.Position + otherBlob.Position) / 2f;
        ToBeRemoved.Add(senderBlob);
        ToBeRemoved.Add(otherBlob);
        ToBeAdded.Add((senderBlob.Type + 1, midPoint));

        HandledCollisionPairs.Add((senderBlob.Id, otherBlob.Id));

        return true;
    }

    /// <summary>
    /// Checks if the last spawned blob has collided with either the ground wall or another blob. If it is, reenables blob spawning.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="other"></param>
    private void CheckForBlobSpawnReenable(Fixture sender, Fixture other) {
        bool isLastSpawnedBlob =
            sender.Body.Tag is Blob senderBlob && senderBlob == LastSpawned ||
            other.Body.Tag is Blob otherBlob && otherBlob == LastSpawned;
        bool isGroundWall =
            sender.Body.Tag is Wall senderWall && senderWall == GroundWall ||
            other.Body.Tag is Wall otherWall && otherWall == GroundWall;
        bool isOtherBlob =
            sender.Body.Tag is Blob senderBlob2 && senderBlob2 != LastSpawned ||
            other.Body.Tag is Blob otherBlob2 && otherBlob2 != LastSpawned;

        if (isLastSpawnedBlob && (isGroundWall || isOtherBlob)) {
            LastSpawned = null;
            CanSpawnBlob = true;
        }
    }

    /// <summary>
    /// Generates a random blob type up to the maximum spawnable blob type.
    /// </summary>
    /// <returns></returns>
    private eBlobType GenerateRandomBlobType() {
        return (eBlobType)Random.Next(HIGHEST_SPAWNABLE_BLOB_INDEX + 1);
    }

    /// <summary>
    /// Creates the arena walls and returns them.
    /// </summary>
    /// <param name="world">The physics engine world</param>
    /// <returns>Returns an enumerable with the wall game objects.</returns>
    private static IEnumerable<GameObject> CreateArena(World world) {
        float totalWidth = ARENA_WIDTH + 2 * ARENA_WALL_THICKNESS;

        float x = -totalWidth / 2;
        float y = 0;

        GameObject[] walls = new GameObject[] {
            new Wall("Ground", world, new Rectangle(x, y + ARENA_HEIGHT, totalWidth, ARENA_WALL_THICKNESS)),
            new Wall("Left Wall", world, new Rectangle(x, y - 100, ARENA_WALL_THICKNESS, ARENA_HEIGHT + 100)),
            new Wall("Right Wall", world, new Rectangle(x + ARENA_WIDTH + ARENA_WALL_THICKNESS, y - 100, ARENA_WALL_THICKNESS, ARENA_HEIGHT + 100))
        };

        return walls;
    }
}
