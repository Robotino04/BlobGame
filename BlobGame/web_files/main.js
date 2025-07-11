import { dotnet } from "./_framework/dotnet.js";

const { getAssemblyExports, getConfig } = await dotnet
  .withDiagnosticTracing(true)
  .create();

const config = getConfig();

dotnet.instance.Module["canvas"] = document.getElementById("canvas");
await dotnet.ready;

const exports = await getAssemblyExports("Toasted.dll");
console.log(dotnet);
console.log(exports);

document.getElementById("loading").style.display = "none";
document.getElementById("start").style.display = "inline";

async function start_game() {
  await dotnet.instance.runMain();
  document.getElementById("start").style.display = "none";
  document.getElementById("container").style.borderStyle = "solid";
  on_frame();
}

async function on_frame() {
  if (exports.BlobGame.Application.OnBrowserFrameRequest()) {
    let width = exports.BlobGame.Application.GetWidth();
    let height = exports.BlobGame.Application.GetHeight();

    let canvas = document.getElementById("canvas");
    canvas.width = width;
    canvas.height = height;
    canvas.clientTop;

    let rect = canvas.getBoundingClientRect();

    exports.BlobGame.Application.SetMouseScale(
      height / rect.height,
      width / rect.width,
    );

    requestAnimationFrame(on_frame);
  }
}

window.start_game = start_game;
