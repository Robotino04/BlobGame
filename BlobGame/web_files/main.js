import { dotnet } from "./_framework/dotnet.js";

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
  .withDiagnosticTracing(true)
  .create();

setModuleImports("main.js", {
  openUrl: (url) => {
    console.log("[JS] Opening ", url);
    window.open(url, "_blank");
  },
});

const config = getConfig();

dotnet.instance.Module["canvas"] = document.getElementById("canvas");
await dotnet.ready;

const exports = await getAssemblyExports("Toasted.dll");
console.log(dotnet);
console.log(exports);

const FS = dotnet.instance.Module.FS;

FS.mkdirTree("/home/web_user/.config/");
FS.mount(FS.filesystems.IDBFS, {}, "/home/web_user/.config/");

FS.syncfs(true, function (err) {
  if (err) {
    console.error("Error syncing from IndexedDB:", err);
  } else {
    console.log("Filesystem synced from IndexedDB!");
  }
});

document.getElementById("loading").style.display = "none";
document.getElementById("start").style.display = "inline";

async function start_game() {
  const syncFS = () => {
    FS.syncfs(false, function (err) {
      if (err) {
        console.error("Error saving to IndexedDB:", err);
      } else {
        console.log("Game files saved to IndexedDB!");
      }
    });
  };
  setInterval(syncFS, 1000);
  addEventListener("beforeunload", (event) => {
    syncFS();
  });

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
