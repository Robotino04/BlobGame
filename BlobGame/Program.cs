﻿// Entry point.

using BlobGame;
using System.Text;
using System.Text.Json;

bool socketMode = false;
int numParallelGames = 1;
bool useSeparateThreads = false;
int seed = 0;
int port = 0;

//TmpSerializeText();

try {
    if (args.Length > 0) {
        for (int i = 0; i < args.Length; i++) {
            if (!args[i].StartsWith("--"))
                continue;

            if (args[i] == "--debug")
                Application.DRAW_DEBUG = true;

            if (args[i] == "--sockets" &&
                i + 4 < args.Length &&
                int.TryParse(args[i + 1], out numParallelGames) &&
                bool.TryParse(args[i + 2], out useSeparateThreads) &&
                int.TryParse(args[i + 3], out seed) &&
                int.TryParse(args[i + 4], out port)) {
                socketMode = true;
            }
        }
    }

    if (socketMode) {
        SocketApplication.Initialize(numParallelGames, useSeparateThreads, seed, port);
        SocketApplication.Start();
    } else {
        Application.Initialize();
        Application.Start();
    }
} catch (Exception e) {
    StringBuilder sb = new StringBuilder();

    Exception? ex = e;
    while (ex != null) {
        sb.AppendLine(ex.ToString());
        ex = ex.InnerException;
    }

    sb.AppendLine();
    sb.AppendLine(e.StackTrace);

    File.WriteAllText("error.log", sb.ToString());

    throw;
}



void DEBUG_SerializeText() {
    Dictionary<string, string> scrolls = new Dictionary<string, string> {
        { "Classic", "Play the classic mode!\nCombine pieces and gain points." },
    };
    string json = JsonSerializer.Serialize(scrolls, new JsonSerializerOptions() { WriteIndented = true });
    File.WriteAllText(@"G:\Coding\C#\BlobGame\BlobGame\Resources\References\Texts\game_mode_descriptions.json", json);

    Dictionary<string, string> d2 = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(@"G:\Coding\C#\BlobGame\BlobGame\Resources\References\Texts\game_mode_descriptions.json"));
}
