﻿namespace BlobGame.App;
using System.Reflection;
using System.Runtime.InteropServices;

/// <summary>
/// Provides utility methods for working with files, particularly for retrieving paths for saves and resources.
/// </summary>
internal static class Files {

    /// <summary>
    /// Gets the directory where saved files are stored.
    /// </summary>
    private static string ConfigDirectory {
        get {
            bool hasUnixSettings = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") != null || Environment.GetEnvironmentVariable("HOME") != null;
            if (hasUnixSettings) {
                return Path.Combine(
                    Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? (Path.Combine(Environment.GetEnvironmentVariable("HOME")!, ".config")),
                    String.Concat(Application.Instance.Name.Split(Path.GetInvalidFileNameChars()))
                );
            }

            return "Config";
        }
    }
    /// <summary>
    /// Gets the directory where resource files are stored.
    /// </summary>
    private static string ResourceDirectory { get; } = Path.Combine(RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")) ? "." : Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? AppContext.BaseDirectory, "Resources");

    /// <summary>
    /// Retrieves the full path to a save file, and ensures the save directory exists.
    /// </summary>
    /// <param name="fileName">The name of the save file.</param>
    /// <returns>The full path to the save file.</returns>
    public static string GetConfigFilePath(string fileName) {
        // Check if the save directory exists, and create it if it doesn't
        if (!Directory.Exists(ConfigDirectory))
            Directory.CreateDirectory(ConfigDirectory);

        // Return the full path to the save file within the save directory
        return Path.Combine(ConfigDirectory, fileName);
    }

    /// <summary>
    /// Retrieves the full path to a resource file.
    /// </summary>
    /// <param name="paths">An array of subdirectories and/or the filename to construct the path. Useful for nested directories.</param>
    /// <returns>The full path to the resource file.</returns>
    public static string GetResourceFilePath(params string[] paths) {
        // Combine the execution directory of the assembly with the resource directory and provided subpaths
        return Path.Combine(ResourceDirectory, Path.Combine(paths));
    }
}
