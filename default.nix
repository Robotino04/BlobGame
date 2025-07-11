let
  pkgs = import <nixpkgs> {};
in
  pkgs.buildDotnetModule rec {
    pname = "Toasted!";
    version = "0.0.1";
    src = ./.;

    projectFile = "BlobGame/BlobGame.csproj";
    nugetDeps = ./deps.nix;

    dotnet-sdk = pkgs.dotnetCorePackages.dotnet_9.sdk;
    dotnet-runtime = pkgs.dotnetCorePackages.dotnet_9.runtime;

    runtimeDeps = with pkgs; [
      xorg.libX11
      xorg.libXi
      glfw2
      glfw3
      libGLU
      libGL
      alsa-lib
    ];

    nativeBuildInputs = with pkgs; [
      makeWrapper
      # required by wasm-tools
      python3
    ];

    LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath (runtimeDeps ++ (with pkgs; [raylib]));

    fixupPhase = ''
      mkdir -p $out/bin/Resources/
      find . -name "MelbaToast.theme"
      cp ./BlobGame/Resources/Themes/*.theme $out/bin/Resources/

      wrapProgram $out/bin/Toasted \
        --set TOASTED_RESOURCES $out/bin/Resources/
    '';
  }
