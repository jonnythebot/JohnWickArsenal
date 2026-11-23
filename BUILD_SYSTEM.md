# Local Unity and Android build

GitHub Actions checks the repository source. Unity builds run on a computer with Unity Hub, Unity Personal active, and Android Build Support installed.

## Set up the editor

1. Install [Unity Hub](https://unity.com/download) on a supported desktop operating system. Sign in and add or activate your Unity Personal license in Hub.
2. Install [Unity 2022.3.20f1](https://unity.com/releases/editor/whats-new/2022.3.20f1), changeset `61c2feb0970d`. In Hub, add **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK** to that editor installation.
3. Clone this repository or check out the prepared branch. In Unity Hub, choose **Add** and open the repository's `UnityProject` folder. Let the Package Manager resolve dependencies and regenerate `Packages/packages-lock.json`. Check the Console for red errors.
4. Choose **File → Build Settings → Android → Switch Platform**. In the Project browser, inspect the imported assets under `Assets/JohnWickArsenal`. Review suit seams, scale, and materials before creating or modifying a wardrobe prefab.
5. Close that project in Unity before running the command-line build. Unity allows one editor per project at a time.

## Run checks or export cosmetics

Use Python 3 and pass the path to your installed Unity executable. Omit `--unity` if the editor is installed in a standard Hub location. Run these commands from the repository root.

```sh
python3 Tools/validate_android_sources.py
python3 Tools/build_local.py --check --unity "/path/to/Unity"
python3 Tools/build_local.py --unity "/path/to/Unity"
```

On Windows, use `py -3` and pass the full path to `Editor\Unity.exe`. On macOS, pass the path to `Unity.app/Contents/MacOS/Unity`. On Linux and macOS, `./build.sh` accepts the same build arguments.

`--check` verifies the installed Android modules without launching Unity. The export command runs the locally activated editor, imports the project, and writes `johnwickarsenal-cosmetics.bundle` inside a fresh `BuildOutput/cosmetics-android-*` folder. The Unity log is `BuildOutput/unity-android-*.log`. Treat an editor error, missing bundle, or invalid bundle header as a failed export.

## Nomad runtime and wardrobe work

The Android bundle produced here is for cosmetic asset review. A playable Blade & Sorcery: Nomad mod requires the game-specific catalog, compatible runtime assembly, and Android Addressables export from [Warpfrog's BasSDK](https://dev.azure.com/Warpfrog/BasSDK/_git/BasSDK). Use the editor version pinned by that SDK for this second project. The checked 1.3 SDK revision `0099eff1fdb9ba0eeac7cdd921568b87aba8f083` specifies Unity 2021.3.38f1.

The existing runtime, prefab generators, and tests depend on missing game APIs and incomplete legacy build paths. Their C# sources remain in the repository, and their Unity project assemblies are gated from default compilation. Keep the gates disabled during cosmetic development. A runtime build requires a separate migration to the real ThunderRoad SDK. The new local export uses the imported cosmetic models and avoids the old raw mod/DLL packaging path.

For wearable suits, fit the garment to the game body, transfer weights to the stock skeleton, and author the Manikin and wardrobe data in the official SDK. Verify each imported mesh and material on Android, build catalog and Addressables output, then test equip, movement, and visibility on Quest. [Warpfrog armor guide](https://kospy.github.io/BasSDK/Components/Guides/Armors/)

The earlier shell helpers are preserved with a `.disabled` extension under `Tools/Legacy`. Their output formats and licensing instructions describe the historical workflow.

## Source audit

- The original checkout has no checked-in prefab, scene, compiled runtime DLL, or Nomad Addressables export. The previous GitHub workflow could synthesize a dummy DLL. The current checks compile no managed assemblies and produce no release artifact.
- The original package manifest referenced an invalid URP identifier and an unavailable Addressables version. The active project uses the built-in render pipeline with Addressables 1.21.21 available for authoring. The stale package lock was removed so Unity Hub can regenerate a resolved graph.
- The five BlendSwap downloads under `AssetDownloads/blendswap` contain HTML login pages. They sit outside `UnityProject/Assets` and require proper source downloads before anyone can use them as meshes.
- Source validation, shell syntax, workflow lint, and a simulated editor invocation can run without Unity. Real Unity package resolution, C# compilation, texture import and material appearance, and Android bundle export still need the local editor.
