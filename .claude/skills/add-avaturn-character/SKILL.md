---
name: add-avaturn-character
description: Turn an Avaturn (or any rigged humanoid .glb/.fbx) avatar into an Eric Racer racer - Humanoid avatar, kart seat fit, CharacterDefinition, catalog. Use when the user imports a new family member model.
---

# Adding an Avaturn racer

Avaturn exports a `.glb` with textured skinned meshes and a 52-bone skeleton using Mixamo names **without** a prefix (`Hips, Spine, Spine1, Spine2, Neck, Head, LeftArm, LeftForeArm, LeftUpLeg, LeftHandIndex1..3`...), in T-pose, about 1.8 m tall. `com.unity.cloud.gltfast` imports it (installing glTFast needs a Unity restart and then an MCP reconnect).

(Pinoc `.ply`/vsplat files are Gaussian splats with no triangles, so they **can't** be used as characters. Pinoc FBX animations on a `mixamorig` skeleton **can**: set Rig = Humanoid.)

## Steps (keep asset creation and reference wiring in separate `execute_code` calls)
1. **Inspect** the GLB JSON (PowerShell: read the chunk length at byte 12, parse JSON from byte 20) for meshes, `skins[0].joints` and textures.
2. **Avatar:** instantiate the imported model and build a `HumanDescription`:
   - Spine1→Chest, Spine2→UpperChest, Arm→UpperArm, ForeArm→LowerArm, UpLeg→UpperLeg, Leg→LowerLeg, ToeBase→Toes.
   - Fingers: `{Side}Hand{Thumb|Index|Middle|Ring|Pinky}{1,2,3}` map to `{Side} {Thumb|Index|Middle|Ring|Little} {Proximal|Intermediate|Distal}`.
   - The skeleton list is every transform's local pose (T-pose).
   - `AvatarBuilder.BuildHumanAvatar` → save it as `Characters/<Name>/<Name>Avatar.asset`. Check that `isValid` and `isHuman` are both true.
3. **Prefab `Characters/Character_<Name>.prefab`:**
   - The model instance as root, plus `Animator` (avatar, `PlayerController.controller`, no root motion) and `CharacterView`.
   - Scale **0.75** fits an adult in the kart.
   - `CharacterView.seatOffset` = **(0, -0.04, -0.11)** for adult proportions.
4. **Verify the seat visually:** put the prefab in a `KartDisplay` at the `KartVisual/PlayerIdle` pose and sample `PlayerIdle.FBX`'s "PlayerIdle" clip with `AnimationMode`. Render side and front views to a RenderTexture, then save to `Temp/Screens`. Look for hips in the seat, hands at the wheel, and feet at the pedals.
5. **Data:** create `Data/Characters/Character_<Name>.asset` (call 1). Then set displayName, driverPrefab and kartColor, and append it to `CharacterCatalog` (call 2). Check the `.asset` on disk with grep.
6. Run `ReferenceValidator.Validate()` (it must be empty). Do a play test in the lobby (browse to them), then a **build** test with `-host -players 1 -autodrive -character <index> -screenshot 15`. The screenshot lands in `%USERPROFILE%/AppData/LocalLow/Dydzinski Family/Eric Racer/shot_<name>.png`. Check for pink materials.
