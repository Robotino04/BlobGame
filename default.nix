let
  pkgs = import <nixpkgs> {};
in
  pkgs.buildDotnetModule rec {
    pname = "Toasted!";
    version = "0.0.1";
    src = ./.;

    projectFile = "BlobGame/BlobGame.csproj";
    nugetDeps = ./deps.nix;

    dotnet-sdk = pkgs.dotnetCorePackages.sdk_7_0;
    dotnet-runtime = pkgs.dotnetCorePackages.runtime_7_0;

    runtimeDeps = with pkgs; [
      xorg.libX11
      xorg.libXi
      glfw
      libGLU
      libGL
      alsa-lib
    ];

    nativeBuildInputs = [
      pkgs.makeWrapper
    ];

    dotnetInstallFlags = ["--framework" "net7.0"];

    fixupPhase = ''
      mkdir -p $out/bin/Resources/
      find . -name "MelbaToast.theme"
      cp ./BlobGame/Resources/Themes/*.theme $out/bin/Resources/

      wrapProgram $out/bin/Toasted \
        --set TOASTED_RESOURCES $out/bin/Resources/
    '';
  }
