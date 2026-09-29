# audio-reactive-dev

Development project of [Audio Reactive](https://github.com/RottenEagle1337/audio-reactive), a minimal low-latency
audio-reactive toolkit for Unity 6. It contains the demo scene, the tools used to author the package samples,
and both the package and its native plugin as git submodules.

**To use the toolkit, install the package; you do not need this project.** It is for working on the package itself.

| Repository | Path here | Contents |
|---|---|---|
| [audio-reactive](https://github.com/RottenEagle1337/audio-reactive) | `Packages/com.rotteneagle.audioreactive` | Unity package (embedded, editable) |
| [audio-reactive-native](https://github.com/RottenEagle1337/audio-reactive-native) | `Native/` | C++ capture plugin, CMake / Visual Studio builds, CI |
| **audio-reactive-dev** | | this Unity project |

## Getting started

```
git clone --recursive https://github.com/RottenEagle1337/audio-reactive-dev.git
```

Already cloned without `--recursive`: `git submodule update --init`.

- Unity 6000.6 (see `ProjectSettings/ProjectVersion.txt`), URP, Visual Effect Graph.
- `Assets/Scenes/SampleScene.unity`: demo with Low / High trackers, a spectrum analyzer, binders and a VFX graph.
  Play music on the default output device and enter Play Mode.

## Working on the package

- The package is embedded (`Packages/`), so edits apply immediately. Commit them in the submodule
  (`audio-reactive`), then commit the new submodule revision here.
- Samples live in the package's `Samples~` folder, which Unity ignores. Use the **Audio Reactive/Dev** menu:
  - **Unlock Samples**: `Samples~` → `Samples`, so the samples can be edited in place (GUIDs kept);
  - **Lock Samples**: back to `Samples~`; do this before committing;
  - **Import All Samples (test)**: imports every sample exactly as a user would, into `Assets/Samples`
    (delete that folder afterwards).
- EditMode tests: *Window > General > Test Runner*, assembly `AudioReactive.Editor.Tests`.

## Working on the native plugin

Close Unity (it keeps the library loaded), then build from `Native/`:

```
cmake --preset windows-x64
cmake --build --preset windows-x64
```

The build copies the binary into the package's `Runtime/Plugins/`. For releases, the package ships binaries from
the native repository's CI Releases instead of local builds. See the
[native README](https://github.com/RottenEagle1337/audio-reactive-native#readme).

## License

[Unlicense](LICENSE.md) (public domain).
