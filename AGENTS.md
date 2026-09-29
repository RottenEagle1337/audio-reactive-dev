# AGENTS.md

Introduction for AI agents working on this project: structure, conventions and how to verify changes.

## Overview

**AudioPlugin** is a Unity project for audio-reactive visuals. At runtime it captures sound (the system output via
WASAPI loopback, or a microphone), analyzes it (band level with onsets, spectrum) and binds the results to scene
properties and VFX Graph. The model is LASP / TouchDesigner audioAnalysis, kept minimal.

- **Unity**: 6000.6.0f1 (Unity 6), see `ProjectSettings/ProjectVersion.txt`.
- **Rendering**: Universal Render Pipeline (URP) 17.6.0.
- **Language**: C# (.NET Standard 2.1, LangVersion 9.0).
- **Platforms**: Windows x64 (shipped). Linux x64 and macOS binaries are built by the native CI but not shipped yet.
- **Packages** (`Packages/manifest.json`): URP, VFX Graph 17.6, Input System, uGUI, Test Framework,
  `com.unity.pipeline` (required by the Unity CLI), IDE packages and the standard modules.
- The main code is the embedded UPM package `com.rotteneagle.audioreactive` (`Packages/`); the project is its demo
  and test bench.
- Git: three repositories (see "Repositories"); this project is `audio-reactive-dev`.

## Structure

```
Packages/com.rotteneagle.audioreactive/        ← package v1.0.0-preview.1, namespace AudioReactive
├── package.json, README.md (the package's GitHub page, roadmap), CHANGELOG.md, Third Party Notices.md
├── Documentation~/images/  README images (hero.gif, inspector.png, sample-*.png); not imported by Unity
├── Runtime/  (AudioReactive.Runtime.asmdef)
│   ├── Capture/   AudioCaptureNative (P/Invoke, internal), AudioCaptureContext, DeviceSelection,
│   │              AudioSystem (stream cache per device, refcount), InputStream
│   ├── Analysis/  AudioLevelTracker, SpectrumAnalyzer (components), SpectrumFrame (shared FFT),
│   │              Fft, BiquadFilter
│   ├── Binding/   PropertyBinder, ComponentPropertyBinder<T> (Float/Vector3/EulerRotation/Color),
│   │              MaterialPropertyBinder<T> (MaterialFloat/MaterialColor)
│   └── Plugins/Windows/x86_64/AudioCapture.dll   ← (Linux/x86_64 and macOS: slots for the other OS builds)
├── Vfx/      (AudioReactive.Vfx.asmdef, only with com.unity.visualeffectgraph) VFX binders,
│             one class per file (otherwise Unity cannot serialize the MonoBehaviour)
├── Tests/Editor/ (AudioReactive.Editor.Tests) EditMode tests
├── LICENSE.md (Unlicense), .gitignore, .gitattributes: the package is its own git repository
├── Samples~/ LevelBasics, BandsAndOnsets, PropertyBinders, Spectrum, VfxGraph (scene + README;
│             sample scripts in per-group asmdefs AudioReactive.Samples.<Group>)
└── Editor/   (AudioReactive.Editor.asmdef, AudioReactive.EditorTools), UI Toolkit:
    AudioLevelTrackerEditor, SpectrumAnalyzerEditor, DeviceSelectionDrawer, PropertyBinderDrawer,
    Meters/ (MeterScheduler, QuadMesh, LevelMeterElement, SpectrumGraphElement)

Assets/
├── Editor/SampleDevTools.cs    menu Audio Reactive/Dev: Unlock / Lock Samples, Import All Samples (test)
├── Editor/ReadmeCapture.cs     Capture README Frames: Camera.main → PNG frames (Temp/ReadmeCapture);
│                               CaptureWindow("InspectorWindow", path): screenshot of an editor window
├── Scenes/SampleScene.unity    demo: /Audio/{Low, High, Spectrum}, Sphere, Directional Light, Audio VFX
├── VFX/AudioReactiveDemo.vfx   demo graph (exposed AudioLevel -> spawn rate, SpectrumTexture, SpectrumSize)
├── Settings/                   URP assets
└── InputSystem_Actions.inputactions   Input System template asset (not used by code)
```

Native sources: `Native/` at the project root, the `audio-reactive-native` submodule. Contents:
- CMake and a VS 2022 sln; miniaudio 0.11.25;
- README with the C API v3;
- smoke test `tests/smoke.cpp`; CI `.github/workflows/build.yml`.

Builds deploy to the relative path `../Packages/...`, only when that folder exists. The old copies
`E:\MyProjects\C++\AudioCapture` and `E:\MyProjects\C++\AudioCapture_legacy` are **not to be used**; they are to
be deleted once the user confirms.

## Repositories

Three GitHub repositories owned by `RottenEagle1337`:

| Repository | Path here | Contents |
|---|---|---|
| `audio-reactive` | `Packages/com.rotteneagle.audioreactive` (submodule) | UPM package; installed from `https://github.com/RottenEagle1337/audio-reactive.git#v<version>` |
| `audio-reactive-native` | `Native/` (submodule) | C++ plugin; Actions build Windows / Linux / macOS, a `v*` tag publishes Release `AudioCapture-<tag>-plugins.zip` |
| `audio-reactive-dev` | root | this Unity project |

- The package stays embedded and editable (Unlock / Lock Samples work).
- Changes are committed in the submodule first, then the new submodule revision is committed in Dev.
- Native has its own versions (`v1.0.0` = C API 3).
- The roadmap lives in the package README.

## Package release (checklist)

1. Lock Samples (the package must contain `Samples~`, not `Samples`).
2. Compilation 0 errors / 0 warnings, then
   `unity command run_tests --mode EditMode --filter AudioReactive.Editor.Tests --filter_type assembly`.
3. Play-test the dev scene and import all samples (`SampleDevTools.ImportAll`), then delete `Assets/Samples`.
4. If the native code changed:
   1. Push a `v*` tag in `audio-reactive-native` and wait for its Release.
   2. Close Unity. Copy the binaries from `AudioCapture-<tag>-plugins.zip` into `Runtime/Plugins/`. Keep the
      `.meta` files, and ship only verified platforms.
   3. Open Unity and repeat steps 2–3.
   4. Record the native version in the CHANGELOG.
5. Bump the `package.json` version and update the CHANGELOG (Keep a Changelog). Commit in the package
   repository and tag `v<version>`.
6. In Dev, update the submodules (the package to its tag) and commit.

`*.csproj`, `AudioPlugin.slnx`, `Library/`, `Temp/`, `Logs/`, `UserSettings/` are generated; do not edit them.

## Samples

- **Source.** `Samples~` (Unity does not see it).
  - To edit: menu **Audio Reactive/Dev/Unlock Samples** (`Samples~` → `Samples`, edit in place, GUIDs kept).
  - Afterwards: **Lock Samples** (saves and closes sample scenes, restores `Samples~`).
  - From the CLI: `eval_file` calling `SampleDevTools.Unlock()` / `Lock()` through reflection (the class lives in
    Assembly-CSharp-Editor).
- **Self-contained groups.** Each group has its own materials and no references to other groups. URP.
- **"As a user" check:** Lock → `SampleDevTools.ImportAll()` → scenes in
  `Assets/Samples/Audio Reactive/<version>/` → Play test → delete `Assets/Samples`.
- **VFX graphs.** The sample graphs were built programmatically through the internal VFX API: a temporary
  assembly with the friend name `Unity.Testing.VisualEffectGraph.Editor`, since removed. They can now be edited
  in the VFX Graph editor.

## Architecture: layers and data flow

LASP model: there is no source component; every tracker selects its own device.

1. **Capture.**
   - Plugin: driver thread → lock-free frame ring (16384 frames, mono/L/R), plus a watchdog that writes zeros
     while loopback is silent.
   - `AudioSystem.Acquire(DeviceSelection)` returns a shared `InputStream` per device (refcount, closed at 0).
   - `InputStream` updates lazily, once per frame, on first access: `AC_ReadNew` for each requested channel →
     new samples + history (~340 ms). Device loss is polled once per second.
   - No execution order is needed.
2. **Analysis** (component `Update`).
   - `AudioLevelTracker`:
     - new channel samples → biquads (LowPass/BandPass/HighPass) → 10 ms MS follower → dB → auto gain / gain +
       dynamic range → fall-down (LASP) → `NormalizedLevel`;
     - onset: energy rises above the running average × threshold, with re-arm and refractory period →
       `Envelope`, `On Onset`;
     - then the tracker applies its binders.
   - `SpectrumAnalyzer`: `InputStream.GetSpectrum(channel, size)` (one FFT per stream/channel/size per frame) →
     log bands and 10 octaves → dB → auto gain → attack/release → RFloat `Texture`.
3. **Binding.**
   - The tracker holds `[SerializeReference] List<PropertyBinder>`.
   - Each binder takes Level or Envelope, applies a curve, and lerps value0 → value1.
   - The property setter is a delegate (`Delegate.CreateDelegate`), with no allocations. Only properties are
     supported, not fields.
   - VFX: `VFXAudioLevelBinder`, `VFXAudioSpectrumBinder`, `VFXAudioWaveformBinder` for the standard VFX
     Property Binder.
4. **Monitors** exist only in the Inspector (UI Toolkit).
   - A meter is a `VisualElement` with `generateVisualContent`; all rectangles go into one mesh (`QuadMesh`).
   - A shared 30 Hz `MeterScheduler` runs only in Play Mode and only for visible elements.
   - Repaint happens only when values change; text updates at 10 Hz.

## Cross-platform

- **Windows** (WASAPI, loopback): built and verified.
- **Linux** (PulseAudio/ALSA) and **macOS** (Core Audio): built by CI (`audio-reactive-native`), not verified in
  Unity yet.
- **Loopback outside Windows** goes through input devices (a monitor source, BlackHole); the Inspector shows a hint.
- **No plugin for the platform:** capture is marked unavailable (one warning) and trackers output silence.

## Working with the project through the Unity CLI

The Editor connects via `unity status` (package `com.unity.pipeline`). Edit scenes, assets and packages through
Editor commands (`unity command ...`), not by hand-editing YAML.

- `unity command eval_file --file x.cs` runs C# in the Editor. No `using` is allowed in the code; use fully
  qualified type names (`AudioReactive.AudioLevelTracker`) and `return` the result.
- Compilation: `AssetDatabase.Refresh` through eval (or `unity command recompile`), then
  `unity command console_status` (`compilationFailed`, `consoleErrors`) and `unity command console --level error`.
- Scenes: `get_scene_hierarchy`, `remove_component`, `save_scene`; `editor_play` / `editor_stop`.
- Packages: `package_remove` / `package_add` trigger a domain reload. The CLI response may end with a network
  error; check the result in `Packages/manifest.json`.
- Play Mode driven from the CLI stalls while the Editor window is in the background (`runInBackground` is off in
  Player Settings). Set `UnityEngine.Application.runInBackground = true` through eval after entering Play Mode.
  This is a runtime-only flag and resets on exit.

## Updating the native plugin

1. Close Unity (`unity projects close F:\Unity\AudioPlugin`): the Editor keeps the library loaded.
2. Build from `Native/` either way; both deploy to `Runtime/Plugins/Windows/x86_64/`:
   - `cmake --preset windows-x64` + `cmake --build --preset windows-x64`. Use the cmake shipped with VS 2022,
     under `E:\Microsoft Visual Studio\2022\Community\` + `Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe`.
     Add `-DAUDIOCAPTURE_BUILD_TESTS=ON` and run `ctest --preset windows-x64` for the smoke test.
   - `MSBuild AudioCapture.sln /p:Configuration=Release /p:Platform=x64`.

   Linux / macOS binaries come from the native CI; on those OSes, the `linux-x64` / `macos-universal` presets also
   work locally.
3. `unity open F:\Unity\AudioPlugin`. When the ABI changes, bump `kApiVersion` (C++) and
   `AudioCaptureNative.ApiVersion` (C#); a mismatch is logged at startup.

Release binaries in the package come from the native CI Release, not from local builds (see the release checklist).

## Verifying changes

**EditMode tests** live in `Packages/com.rotteneagle.audioreactive/Tests/Editor`.
- They cover Fft, biquads, binders, DeviceSelection and AudioSystem refcounting; the package is listed in
  `testables` in `Packages/manifest.json`.
- Run them with `unity command run_tests` (see the release checklist).
- Tests get internal access through `Runtime/AssemblyInfo.cs`.

**Enter Play Mode Options.** The project runs without domain reload. Statics are reset in
`RuntimeInitializeOnLoadMethod(SubsystemRegistration)`; reset any new static state the same way.

**Manual check:**
1. Compilation without errors or warnings (see the CLI section).
2. Play Mode with real audio.
3. Read `NormalizedLevel` / `Envelope` / `LogBands` / `OctaveLevels` through eval, and watch the Inspector meters.

**Per-frame allocations.** Measure with `ProfilerRecorder` ("GC Allocated In Frame") against the same scene with
the package components disabled. The difference should be ~0; Editor background is ~14 KB/frame and varies by a
few KB.

Note: lambdas added from eval (e.g. `AddListener`) are not invoked; poll state instead.

## Code conventions

- All code, comments, tooltips and logs are **in English**.
- Every file starts with a `// FileName.cs` header describing its purpose and key decisions.
- Namespace `AudioReactive`; editor code `AudioReactive.EditorTools`; VFX `AudioReactive.Vfx`.
- Components get `[AddComponentMenu("Audio Reactive/...")]`.
- The package is minimal (like LASP). New functionality goes into settings of the existing trackers or new
  `PropertyBinder` types; do not add presets or genre templates.
- Component settings are `[SerializeField] private` fields with public properties; results are read-only.
- **No allocations** in per-frame paths.
- Do not use `??` / `??=` with `UnityEngine.Object`.
- No runtime OnGUI: debugging happens only in inspectors (UI Toolkit, see "Monitors").
- Analysis code contains no `#if UNITY_STANDALONE_*`; platform differences live only in native code.
- Move `.meta` files together with their assets: GUIDs link scene references.

## Limitations

- The package ships the plugin for Windows x64 only (Editor and Player); Linux / macOS after verification on those
  OSes.
- WASAPI shared mode: latency ≥ the Windows audio engine period (~10 ms) + one frame.
- Backup of the state before the reorganization: `F:\Backups\AudioPlugin_2026-09-28\AudioPlugin_full.zip`.
