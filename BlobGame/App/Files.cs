namespace BlobGame.App;
/// <summary>
/// Provides utility methods for working with files, particularly for retrieving paths for saves and resources.
/// </summary>
internal static class Files {
    /// <summary>
    /// Retrieves the full path to a save file, and ensures the save directory exists.
    /// </summary>
    /// <param name="fileName">The name of the save file.</param>
    /// <returns>The full path to the save file.</returns>
    public static string GetConfigFilePath(string fileName) {
        string ConfigDirectory = "Config/";
        if (Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") != null){
            ConfigDirectory = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") + "/toasted/";
        }
        
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
        string ResourceDirectory = Environment.GetEnvironmentVariable("TOASTED_RESOURCES") ?? "Resources"; 
       
        // Combine the execution directory of the assembly with the resource directory and provided subpaths
        return Path.Combine(ResourceDirectory, Path.Combine(paths));
    }
}
