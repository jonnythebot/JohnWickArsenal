# Local Unity and Android build

GitHub Actions checks the repository source. Unity builds run on a computer with Unity Hub, Unity Personal active, and Android Build Support installed.

## Set up the editor

1. Install [Unity Hub](https://unity.com/download) on a supported desktop operating system. Sign in and add or activate your Unity Personal license in Hub.
2. Install [Unity 2022.3.20f1](https://unity.com/releases/editor/whats-new/2022.3.20f1), changeset `61c2feb0970d`. In Hub, add **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK** to that editor installation.
3. Clone this repository or check out the prepared branch. In Unity Hub, choose **Add** and open the repository's `UnityProject` folder. Let the Package Manager resolve dependencies and regenerate `Packages/packages-lock.json`. Check the Console for red errors.
4. Choose **File → Build Settings → Android → Switch Platform**. In the Project browser, inspect the imported assets under `Assets/JohnWickArsenal`. Review suit seams, scale, and materials before creating or modifying a wardrobe prefab.
5. In the **John Wick Arsenal** menu, choose **Validate Cosmetic Imports**, then **Export Android Cosmetic Review Bundle** to export from the open editor. For command-line builds, close the project first. Unity allows one editor per project at a time.

## Run checks or export cosmetics

Use Python 3 and pass the path to your installed Unity executable. Omit `--unity` if the editor is installed in a standard Hub location. Run these commands from the repository root.

```sh
python3 Tools/validate_android_sources.py
python3 Tools/build_local.py --check --unity "/path/to/Unity"
python3 Tools/build_local.py --validate-imports --unity "/path/to/Unity"
python3 Tools/build_local.py --unity "/path/to/Unity"
```

On Windows, use `py -3` and pass the full path to `Editor\Unity.exe`. On macOS, pass the path to `Unity.app/Contents/MacOS/Unity`. On Linux and macOS, `./build.sh` accepts the same build arguments.

`--check` verifies the installed Android modules without launching Unity. `--validate-imports` launches Unity and checks the imported renderers and material references. It writes `cosmetic-imports.json` with the editor version, Android target, and imported model paths. The export command adds `johnwickarsenal-cosmetics.bundle` alongside that report in a fresh `BuildOutput/cosmetics-android-*` folder. Editor logs are in `BuildOutput/unity-android-*.log`.

## Nomad runtime and wardrobe work

The Unity 2022.3.20f1 Android bundle is for authoring and cosmetic review. For Nomad 1.3.2, the briefcase uses [Warpfrog's BasSDK](https://dev.azure.com/Warpfrog/BasSDK/_git/BasSDK) 1.3. The checked revision `0099eff1fdb9ba0eeac7cdd921568b87aba8f083` specifies **Unity 2021.3.38f1** and mod manifest compatibility `1.0.0.0`. Install that editor and its Android modules alongside the authoring editor.

The existing runtime, prefab generators, and tests depend on missing game APIs and incomplete legacy build paths. Their C# sources remain in the repository, and their Unity project assemblies are gated from default compilation. Keep the gates disabled during cosmetic development. A runtime build requires a separate migration to the real ThunderRoad SDK. The new local export uses the imported cosmetic models and avoids the old raw mod/DLL packaging path.

### Export the briefcase prop for Nomad

1. Clone the official BasSDK to a separate folder and run `git lfs pull` there. Sign in to Unity Hub and activate the SDK's editor on your computer.
2. From John Wick Arsenal, run the SDK preflight, then run the export. The tool builds from a temporary copy of the SDK and checks the generated item data, Addressables catalog and bundles.

   ```sh
   python3 Tools/build_nomad_cosmetics.py --check --sdk "/path/to/BasSDK" --unity "/path/to/2021.3.38f1/Unity"
   python3 Tools/build_nomad_cosmetics.py --sdk "/path/to/BasSDK" --unity "/path/to/2021.3.38f1/Unity"
   ```

3. A verified export writes `JohnWickArsenalCosmetics_v*.zip` and `unity-nomad.log` in a fresh `BuildOutput/nomad-cosmetics-*` folder. Extract the `JohnWickArsenalCosmetics` folder into `Android/data/com.Warpfrog.BladeAndSorcery/files/Mods` on the Quest. In Sandbox, check the spawn book for **Briefcase**, then spawn, grab, and drop it. Check the handle position, collision and materials. [Warpfrog's install guide](https://kospy.github.io/BasSDK/Components/Guides/HowToInstallMods.html)

### Fit and verify wearable suits

The briefcase prop uses built-in ThunderRoad item components. The existing suit meshes are static authoring inputs. Fit the garment to the stock character in the matching bind pose, check scale against the official example, transfer and normalize weights, and export a skinned FBX. Use the stock rig from a local SDK copy. Run the Blender check after skinning.

```sh
blender --background --python-exit-code 1 --python Tools/BlenderScripts/audit_wardrobe.py -- \
  --reference "/path/to/BasSDK/Assets/SDK/Examples/Characters/Sources/HumanMale_Complete_Model.fbx" \
  "/path/to/fitted-garment.fbx"
```

The check reports unweighted vertices, incompatible bone hierarchy, weight sums and placement bounds. Review deformation and clipping yourself. Then create the Manikin part, body occlusion masks, wardrobe definition and item prefab in BasSDK. Export for Android and test equip, movement and visibility on Quest. [Warpfrog armor guide](https://kospy.github.io/BasSDK/Components/Guides/Armors/)

The earlier shell helpers are preserved with a `.disabled` extension under `Tools/Legacy`. Their output formats and licensing instructions describe the historical workflow.

## Source audit

- The original checkout has no checked-in prefab, scene, compiled runtime DLL, or Nomad Addressables export. The previous GitHub workflow could synthesize a dummy DLL. The current checks compile no managed assemblies and produce no release artifact.
- The original package manifest referenced an invalid URP identifier and an unavailable Addressables version. The active project uses the built-in render pipeline with Addressables 1.21.21 available for authoring. The stale package lock was removed so Unity Hub can regenerate a resolved graph.
- The original C# dependency checker also fails the static C# parser. Its assembly is gated with the other legacy tools.
- The five BlendSwap downloads under `AssetDownloads/blendswap` contain HTML login pages. They sit outside `UnityProject/Assets` and require proper source downloads before anyone can use them as meshes.
- Source validation, shell syntax, workflow lint, and mesh/texture and skinning checks run without Unity. Unity package resolution, C# compilation, material appearance, and Android export still need the local editor.

The [cosmetic asset inventory](UnityProject/Assets/JohnWickArsenal/Models/Cosmetics/ASSET_CREDITS.txt) lists the suit replacement, its backup, and the three new cosmetic source models. Blender import and rendering were checked. Local Unity import and Quest performance still need to be measured.
