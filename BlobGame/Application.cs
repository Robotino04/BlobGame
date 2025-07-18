using BlobGame.App;
using BlobGame.Audio;
using BlobGame.Drawing;
using BlobGame.ResourceHandling;
using ZeroElectric.Vinculum;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Runtime.InteropServices.JavaScript;

namespace BlobGame;
/// <summary>
/// The main application class. Initializes the window, drawing engine and controllers. Manages the game loop and threads.
/// </summary>
internal sealed partial class Application {
    public static Application Instance { get; private set; }

    /// <summary>
    /// Set to true to enable debug drawing.
    /// </summary>
    public bool DrawDebug { get; set; }
    /// <summary>
    /// The name of the application. Used for the window title.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The frames per second the game is targeting.
    /// </summary>
    public int Fps { get; }
    /// <summary>
    /// The updates per second the game is targeting.
    /// </summary>
    public int Ups { get; }

    ///// <summary>
    ///// The base width of the game window. All resolutions use this to scale components to the right proportions.
    ///// </summary>
    internal const int BASE_WIDTH = 1920;
    ///// <summary>
    ///// The base height of the game window. All resolutions use this to scale components to the right proportions.
    ///// </summary>
    internal const int BASE_HEIGHT = 1080;

    ///// <summary>
    ///// Multiplier to convert world coordinates to screen coordinates. Used to scale components to the right proportions.
    ///// </summary>
    public float WorldToScreenMultiplierX => Raylib.GetScreenWidth() / (float)BASE_WIDTH;
    ///// <summary>
    ///// Multiplier to convert world coordinates to screen coordinates. Used to scale components to the right proportions.
    ///// </summary>
    public float WorldToScreenMultiplierY => Raylib.GetScreenHeight() / (float)BASE_HEIGHT;

    /// <summary>
    /// The thread the game logic is running on.
    /// </summary>
    private Thread GameThread { get; }

    /// <summary>
    /// Flag indicating whether the game is running.
    /// </summary>
    public bool IsRunning { get; private set; }
    /// <summary>
    /// Keeps trakc of the time since the game started.
    /// </summary>
    public float Time { get; private set; }

    public static bool IsBrowser { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));

    public Settings Settings { get; }
    /// <summary>
    /// Static constructor. Initializes the game state, creates threads and hides the console window.
    /// </summary>
    public Application(string name, int fps, int ups) {
        Instance = this;

        Name = name;
        Fps = fps;
        Ups = ups;

        IsRunning = false;
        GameThread = new Thread(RunGameThread);

        Settings = new Settings();
    }

    /// <summary>
    /// Calls the initialization methods of all components.
    /// </summary>
    public void Initialize() {
        ResourceManager.Initialize();
        AudioManager.Initialize();
        Input.Initialize();
        Renderer.Initialize();
        //GUIHandler.Initialize();
        Game.GameManager.Initialize();
    }

    /// <summary>
    /// Starts the game loop and threads. Also creates the window. Handles the shutdown of all components.
    /// </summary>
    public void Start() {
        if (IsRunning)
            return;

        IsRunning = true;

        //Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);
        Raylib.InitWindow(Settings.AVAILABLE_RESOLUTIONS[3].w, Settings.AVAILABLE_RESOLUTIONS[3].h, Name);
        Raylib.SetTargetFPS(Fps);
        Raylib.SetExitKey(KeyboardKey.KEY_NULL);
        Raylib.InitAudioDevice();

        //Image icon = LoadIcon();
        //Raylib.SetWindowIcon(icon);

        Settings.Load();
        ResourceManager.Load();
        AudioManager.Load();
        Input.Load();
        Renderer.Load();
        Game.GameManager.Load();

        if (IsBrowser) {
            Console.WriteLine("Startup finished");
            // the browser will regularly call OnBrowserFrameRequest for us
            return;
        } else {
            GameThread.Start();
            while (IsRunning) {
                if (Raylib.WindowShouldClose()) {
                    IsRunning = false;
                }
                // Raylib will limit us to the framerate automatically here
                RenderFrame();
            }
        }


        ResourceManager.Unload();

        GameThread.Join();
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }

    public void Exit() {
        IsRunning = false;
    }

    /// <summary>
    /// Calculates one step in the game. This is the main job of the game thread.
    /// </summary>
    private void Tick(float deltaTime) {
        Game.GameManager.Update(deltaTime);
    }

    /// <summary>
    /// Renders a single frame. This is the main job of the render thread.
    /// </summary>
    private void RenderFrame() {
        ResourceManager.Update();
        AudioManager.Update();
        Input.Update();
        Renderer.Draw();

    }

    Stopwatch sw = new Stopwatch();

    /// <summary>
    /// JS-accessible method that will get called by requestAnimationFrame.
    /// This means it will run at the display refresh rate so no manual sleeping required.
    /// </summary>
    // has to be static for wasm
    [SupportedOSPlatform("browser")]
    [JSExport]
    public static bool OnBrowserFrameRequest() {
        Instance.sw.Stop();
        float deltaTime = Instance.sw.ElapsedMilliseconds / 1000f;
        Instance.Time += deltaTime;
        Instance.sw.Restart();

        // can't call WindowShouldClose in the browser because that uses emscripten_sleep which isn't linked by dotnet
        // it's not like quitting would work anyways

        // no multithreading in wasm so we just interleave game and render thread
        Instance.Tick(deltaTime);
        Instance.RenderFrame();

        return Instance.IsRunning;
    }

    [SupportedOSPlatform("browser")]
    [JSExport]
    public static int GetWidth() {
        return Raylib.GetRenderWidth();
    }

    [SupportedOSPlatform("browser")]
    [JSExport]
    public static int GetHeight() {
        return Raylib.GetRenderHeight();
    }

    // used to compensate for the canvas not filling the entire screen
    [SupportedOSPlatform("browser")]
    [JSExport]
    public static void SetMouseScale(float xScale, float yScale) {
        Raylib.SetMouseScale(xScale, yScale);
    }


    /// <summary>
    /// Starts and runs the game's logic loop. Including calculating delta time and ensuring thread sleep.
    /// </summary>
    private void RunGameThread() {
        float baseDeltaTime = 1f / Ups;
        float deltaTime = baseDeltaTime;

        while (IsRunning) {
            sw.Restart();

            Tick(deltaTime);

            sw.Stop();
            int sleepTime = (int)Math.Max(0, 1000 / Ups - sw.ElapsedMilliseconds);
            deltaTime = MathF.Max(baseDeltaTime, sw.ElapsedMilliseconds / 1000f);
            Thread.Sleep(sleepTime);
            Time += deltaTime;
        }

        Game.GameManager.Unload();
    }

    private static Image LoadIcon() {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = "BlobGame.Resources.icon.png";

        using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        byte[] imageData;
        using (MemoryStream ms = new MemoryStream()) {
            stream.CopyTo(ms);
            ms.Position = 0;
            imageData = ms.ToArray();
        }

        Image image;
        unsafe {
            fixed (byte* imagePtr = imageData) {
                image = Raylib.LoadImageFromMemory(".png", imagePtr, imageData.Length);
            }
        }

        return image;
    }

}
