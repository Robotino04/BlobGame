let
  pkgs = import <nixpkgs> {};
in
  pkgs.buildDotnetModule rec {
    pname = "Toasted!";
    version = "0.0.1";
    src = ./.;

    projectFile = "BlobGame/BlobGame.csproj";
    nugetDeps = ./deps.nix;

    dotnet-sdk = pkgs.dotnetCorePackages.sdk_9_0;
    dotnet-runtime = pkgs.dotnetCorePackages.runtime_9_0;

    runtimeDeps = with pkgs; [
      xorg.libX11
      xorg.libXi
      glfw2
      glfw3
      libGLU
      libGL
      alsa-lib
    ];

    nativeBuildInputs = [
      pkgs.makeWrapper
    ];

    fixupPhase = ''
      mkdir -p $out/bin/Resources/
      find . -name "MelbaToast.theme"
      cp ./BlobGame/Resources/Themes/*.theme $out/bin/Resources/

      wrapProgram $out/bin/Toasted \
        --set TOASTED_RESOURCES $out/bin/Resources/
    '';
  }
