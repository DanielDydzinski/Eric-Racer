---
name: unity-change
description: Checklist for making any code, scene or prefab change in the Eric Racer Unity project through the Unity MCP, ending with verification. Use before and after every implementation step.
---

# Making a Unity change safely

## Before
1. Read the current step in `docs/PLAN.md` and its definition of done (DoD).
2. Make sure the editor is **not** in Play Mode (exit it through the MCP if it is). Edits made during Play Mode are lost.
3. Check that the console starts clean. If there are existing errors, note them so new ones can be told apart.

## Doing it
- Scripts: write them to `Assets/_EricRacer/Scripts/<Area>/` with namespace `EricRacer.<Area>`. One class per file, and the file name matches the class.
- After writing scripts: ask the MCP to refresh assets, **wait for compilation to finish**, then read the console. Fix every error before touching scenes (a scene edit can't add a component whose script failed to compile).
- Scenes and prefabs: edit through the MCP tools, never by hand-editing YAML. Save the scene or prefab afterwards.
- New ScriptableObject types: add a `[CreateAssetMenu(menuName = "Eric Racer/...")]` and create the instances in `Assets/_EricRacer/Data/`.
- Networked prefabs: add them to the `NetworkPrefabs` list asset in `Assets/_EricRacer/Settings/`.
- Karting Package patches: minimal, tagged `// ERIC-PATCH:`.

## Gotchas learned in this project
- `execute_code` uses the CodeDom compiler (C# 6): no local functions, no `is T x` patterns, and write `UnityEngine.Object` explicitly (plain `Object` is ambiguous).
- `AssetDatabase.DeleteAsset(s)` is blocked in `execute_code`; use `manage_asset action=delete` (batched with `batch_execute`).
- The editor is usually **unfocused** while I work, so Play Mode doesn't tick. Pause, then use `EditorApplication.Step()` in a loop inside `execute_code` to advance frames deterministically.
- Simulated Input System devices don't reach actions while the editor is unfocused (separate editor/player input state). Test the logic by calling methods directly, and leave real controller tests to the user.
- During a player build the MCP disconnects. Wait for `Build Finished` in `%LOCALAPPDATA%/Unity/Editor/Editor.log` instead of polling the build status.
- Right after `manage_editor play`, the first `execute_code` often returns `success:false` with no message (domain reload). Just retry it.
- When frame-stepping, put scene loads (StartHost, LoadScene) in their own `execute_code` call and step in the next one.
- Triggers only fire if the kart actually **overlaps** them during a physics step. When teleporting through a gate, place the kart inside it and step a few frames.
- "JobTempAlloc ... older than 4 frames" warnings come from pausing and stepping, not from game code.
- Remove any test devices (`InputSystem.RemoveDevice`) before leaving Play Mode, because they persist in the editor.

## After (verification)
1. Console: zero errors and no new warnings from our code.
2. Enter Play Mode through the MCP and exercise the change (read logs or check a screenshot), then exit Play Mode.
3. For a multiplayer change, also follow the `mp-test` skill once it exists (Phase 2).
4. Tick the step in `docs/PLAN.md`.
5. Report to the user what changed and how it was verified. **Don't commit** unless asked.
