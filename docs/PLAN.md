# Eric Racer: Weekend Plan

A LAN multiplayer kart racer for Eric's 5th birthday.
**Deadline:** Sunday 11 Oct 2026 (party day). **Platform:** Windows x64 for now (Linux later; the code stays platform-agnostic).

**Decisions made (Thu):** Windows only for now · gamepads available (plus keyboard fallback) · **no** auto-accelerate · "Happy Birthday Eric!" title screen · **Fortnite-style 3D lobby** for character select · family character models (from Pinoc) get imported later, so the character system has to accept new models **without code changes**.

---

## 1. What I found in the project (audit)

| # | Finding | Impact | Action |
|---|---------|--------|--------|
| A1 | ✅ FIXED · **Git repo was nested** at `Eric Racer/Eric Racer/.git` (remote `DanielDydzinski/Eric-Racer`, 1 commit). The project root is not under version control. | Nothing is tracked | Move `.git`, `.gitignore`, `.gitattributes` to the project root and delete the empty folder |
| A2 | `activeInputHandler: 1` means **new Input System only**, but `KeyboardInput.cs` and `CineMachineTargeteer.cs` call the legacy `UnityEngine.Input` | **The kart can't be driven right now** (it throws `InvalidOperationException`) | New Input System kart input (keyboard plus gamepad for free) |
| A3 | The lap and objective system is **global and single-player**: `static Action`s (`Objective.OnUnregisterPickup`, `TimeDisplay.OnUpdateLap`), any collider tagged `Player` counts, and `GameFlowManager` uses `FindObjectsOfType` and loads Win/Lose scenes | Can't track 4 racers | Replace it with our own per-kart race layer (see §3). The track geometry, checkpoint positions and VFX are still reused |
| A4 | `ArcadeKart` is solid physics that reads input via the `IInput` interface (a Strategy pattern already) and has `SetCanMove()` | Good to build on | Reuse it with minimal changes |
| A5 | No respawn: kids **will** fall off or flip | Frustrating for kids | Add auto-respawn plus a "help" button |
| A6 | ML-Agents, Barracuda, Tutorials (IET framework), Visual Scripting, Collab, Share and 7 `*_MLAgent` prefabs plus `.nn` models are installed | Bloat, slower compiles, build risk | Delete them (Phase 0) |
| A7 | Build Settings point at stale paths (`Assets/Karting/...`) | Broken build list | Rebuild the scene list with our own scenes |
| A8 | *(deferred, Windows only for now)* **Linux Build Support is not installed** for 6000.3.6f1 (only Windows and Android) | Can't build for the Linux laptops | **You:** Unity Hub → Installs → 6000.3.6f1 → Add modules → *Linux Build Support (Mono)* |
| A9 | Fun assets already included: Cat, Chicken and Knight racers, hats, kart skins, Rocket kit, 4 tracks (Oval, Country, Mountain, Winding), RTC race tracks | Nice character selection for a 5-year-old | Use them through ScriptableObject catalogs |
| A10 | Assets are 307 MB, the largest file is under 50 MB, and AddOns are 266 MB | GitHub is fine without LFS | Trim unused addons after we choose characters and tracks |

---

## 2. Key technical decisions

| Topic | Decision | Why |
|------|----------|-----|
| Netcode | **Netcode for GameObjects 2.x + Unity Transport** | Official, MCP-friendly, mature |
| Connection | **LAN with automatic host discovery** (UDP broadcast). The host's game shows up as *"Daddy's Race, Join!"*, with manual IP entry as a fallback | Same house: no internet, no cloud account, no join codes for a 5-year-old to type. Relay isn't needed |
| Authority | **Owner-authoritative kart movement** (each PC simulates its own kart and syncs its transform with interpolation). **Server-authoritative race state** (countdown, laps, positions, results) | Zero input lag for the kids. On a family LAN cheating isn't a concern. The race rules stay in one place |
| Remote karts | Kinematic, interpolated proxies. Your kart bumps off them, but they don't get shoved | Simple and stable. Full physics prediction isn't worth it this weekend |
| Input | New Input System, `InputActionAsset` (keyboard WASD and arrows, plus gamepad) | Fixes A2. Gamepads work automatically |
| Scenes | `Boot` → `MainMenu` → `Race_<Track>` (loaded by NGO scene management, so clients follow the host) | Clean flow, no singletons-from-nowhere |
| Build | Windows x64 (Mono), zipped and copied by USB or a shared folder | Fast builds. Linux is one more build target later |
| Lobby | **3D Fortnite-style lobby scene**: 4 pedestals, each player's character idles on their spot with a name and ready badge, and changes live as they browse | The "wow" moment for the kids, and it doubles as character select |
| Characters | `CharacterDefinition` SO + an `ICharacterView` adapter, with a shared **Humanoid** Animator controller | New family models from Pinoc = import the FBX, set the rig to Humanoid, create one SO asset. No code |
| Testing | **Multiplayer Play Mode** package (virtual players inside the editor) plus 2 local builds | Fast iteration without walking between laptops |

---

## 3. Architecture

All our code lives in `Assets/_EricRacer/` with its own assembly (`EricRacer.Runtime`, plus `EricRacer.Editor`). We **don't edit Karting Package code** except for small, marked patches (tagged `// ERIC-PATCH:`).

```
Assets/_EricRacer/
  Scripts/
    Core/        GameBootstrap, ServiceLocator-free wiring, event channels (SO)
    Input/       KartInputActions + InputSystemKartInput (IInput strategy)
    Kart/        NetworkKart, KartVisuals (applies CharacterDefinition), KartRespawner, NameTag
    Race/        RaceManager (server), RaceStateMachine + states, Checkpoint, TrackLayout, RaceProgress
    Net/         ConnectionManager (host/join/leave), LanDiscovery, PlayerSession (name, character, color)
    UI/          MainMenu, Lobby, HUD (lap, position, countdown), Results, presenters subscribe to events
  Data/          *.asset ScriptableObject instances
  Prefabs/       NetworkKart.prefab, UI prefabs
  Scenes/        Boot, MainMenu, Race_Oval, ...
  Settings/      Input actions, NetworkPrefabs list
```

### Patterns: where each one is used and why

| Pattern | Where | Purpose |
|---------|-------|---------|
| **State machine** | `RaceStateMachine`: `Lobby → Countdown → Racing → Results → (Lobby)`. The server drives it and the current state is a `NetworkVariable<RaceStateId>` | One source of truth for race flow. Each state is a small class with `Enter/Tick/Exit` instead of a giant `Update()` with booleans |
| **State machine** (small) | `ConnectionManager`: `Offline → Hosting/Connecting → Connected → Disconnected` | Clean handling of drop-outs and "back to menu" |
| **Strategy** | `IInput` implementations: `InputSystemKartInput` (human), and `DisabledInput` (during countdown) | Swap control schemes per player without touching `ArcadeKart` |
| **Observer** | C# `event`s on `RaceManager` and `RaceProgress`, `NetworkVariable.OnValueChanged`, and SO **event channels** (`RaceEventChannel`, `VoidEventChannel`) for cross-scene messages | UI never polls the race logic and race logic never knows about the UI |
| **ScriptableObjects (data)** | `CharacterDefinition` (name, icon, body/head/helmet prefabs, color), `CharacterCatalog`, `TrackDefinition` (scene, display name, thumbnail, default laps), `TrackCatalog`, `RaceSettings` (laps, countdown, max players, catch-up boost), `KartTuning` (wraps `ArcadeKart.Stats`) | Designer-tweakable without code. The network only syncs **catalog indices** (one `byte`) instead of references |
| **Composition** | `NetworkKart` prefab = `BaseKartClassic` + `NetworkObject` + `NetworkTransform` (owner auth) + our components | `ArcadeKart` stays untouched |
| **Object pooling** | Reuse the existing `PoolObjectDef` for VFX/UI where needed | Avoids GC spikes on older laptops |

### Character system (ready for the Pinoc family models)
```
CharacterDefinition (SO)           CharacterCatalog (SO)  ── index (byte) synced over network
  displayName, portrait sprite       List<CharacterDefinition>
  lobbyPrefab   (full body, idles on a pedestal)
  driverPrefab  (sits in the kart; may be the same model)
  kartColor / kart skin
        │ prefab root has a component implementing
        ▼
ICharacterView  (Adapter + Strategy)
  PlayIdle() · PlaySelected() · PlayVictory() · PlayDriving(steer)
  ├─ HumanoidCharacterView: drives the shared EricRacer_Humanoid.controller (Pinoc models)
  └─ KartRacerCharacterView: wraps the existing Cat/Chicken/Knight part prefabs
```
**Adding a family member later:** import the FBX with animations → Rig = *Humanoid* → make a prefab with `HumanoidCharacterView` → create a `CharacterDefinition` → add it to the catalog. Done. *(Needed from the Pinoc models: a humanoid skeleton, and ideally idle, wave/selected, victory and sitting clips. Missing clips fall back to idle.)*

### Lobby scene (Fortnite-style)
```
LobbyStage: 4 pedestals (slot 0 = host) · spotlight per slot · birthday banner · confetti
Per player slot ── LobbySlotView observes PlayerSession (name, characterIndex, isReady)
                    └─ swaps the ICharacterView model when characterIndex changes (pooled)
Local controls:  ◀ ▶ (D-pad/stick/arrow keys) browse characters · A/Enter = Ready · B = un-ready
Host panel:      track picker · laps · START (enabled when everyone is ready)
Flow:            LobbyState(server) ─ all ready + host START ─► NGO loads Race scene ─► Countdown
After results:   "Race again" ─► back to the Lobby scene, still connected
```
`PlayerSession` is a per-client `NetworkObject` that persists across scenes (DontDestroyOnLoad via NGO) and holds `NetworkVariable`s: name, characterIndex, slot and isReady. Lobby and race both read from it, and it's the single source of player identity.

### Race data flow (server-authoritative)
```
Owner PC:  kart hits Checkpoint trigger ─► RaceProgress.ReportCheckpointServerRpc(index)
Server:    validates the order (prevents shortcut and backwards-driving exploits) ─► updates NetworkVariables
           (lap, nextCheckpoint, finishTime) ─► RaceManager recalculates positions (lap, checkpoint, distance)
All PCs:   OnValueChanged ─► HUD shows "Lap 2/3" and "2nd" ─► when all have finished (or a timeout after the first) ─► Results
```

### Superpowers (each family character has their own)
| Character | Superpower | Status |
|-----------|-----------|--------|
| **Eric** | **Transforms into a big truck or tractor** for a few seconds: bigger, heavier, and opponents he drives into get **squished** (flattened and briefly slowed) | Needs the truck/tractor assets from you. A placeholder (scaled-up kart) is used until then |
| **Kamil** (Dad) | **Drops a bomb behind him**: after a short fuse or on contact it pops, and karts in the radius spin out and are briefly slowed | Can be built with existing assets (Rocket kit, explosion VFX) |
| **Liliana** (sister) | TBD | Plugs into the same framework |
| **Justyna** (Mom) | TBD | Plugs into the same framework |

**Design rule for a 5-year-old's party:** every power is *funny, never punishing*. Effects last 1–2 s, nothing removes a lap or position directly, and cooldowns are around 10–15 s so everyone gets plenty of turns.

```
SuperPowerDefinition (abstract SO, Strategy)        StatusEffectDefinition (SO)
  icon, displayName, cooldown, duration               id, duration, ArcadeKart.Stats modifiers (negative = slow),
  abstract Activate(PowerContext ctx)                  visual cue (squash scale, spin, stars VFX)
  ├─ TransformPower    (Eric):  swap visual → truck prefab, scale, stat buff, enables a SquishZone trigger
  ├─ DropHazardPower   (Kamil): spawns a networked Bomb hazard behind the kart
  └─ (future) Liliana / Justyna powers: just a new subclass + asset
CharacterDefinition.superPower ─► which power this character gets (data only)

KartAbilityController (NetworkBehaviour on each kart)
  Power button (gamepad X / keyboard Space) ─► RequestActivateServerRpc
  Server: validates cooldown and race state ─► power.Activate(ctx) ─► cooldown NetworkVariable (HUD meter observes it)
KartStatusEffects (NetworkBehaviour on each kart)
  Server decides the hit (bomb blast, squish) ─► ApplyEffectClientRpc to the victim's OWNER
  Owner applies the stat modifiers through ArcadeKart.AddPowerup() (it already supports timed stat modifiers),
  and every PC plays the visual (squash, spin, stars) through the OnEffectStarted/Ended events (Observer)
Hazards (Bomb): server-spawned NetworkObjects with a fuse/trigger, pooled
```
Why this shape: powers are **data plus small strategy classes**, so a new family power never touches the kart, network or UI code. Because the owner applies the effects to their own physics, an effect feels instant on the victim's screen. The server is the only one who decides *who got hit*, so all four screens agree.

---

## 4. Step-by-step plan

Every step ends with **a definition of done (DoD)** that I check through MCP: it compiles, the console is clean, and it was play-tested.

### Phase 0: Foundation (Thu evening, about 1.5 h)
- [x] 0.1 ~~Linux Build Support~~ (deferred, Windows only for now)
- [x] 0.2 **You:** restart Claude Code so the Unity MCP tools load in this session (`/mcp` shows UnityMCP connected)
- [x] 0.3 Move the git repo to the project root (done; `.gitignore` verified: Library, Temp, Logs, Builds, *.csproj, *.sln ignored) → baseline committed
- [x] 0.4 Remove bloat: the `ml-agents`, `barracuda`, `learn.iet-framework`, `visualscripting`, `collab-proxy` and `connect.share` packages; `ML-Agents/`, `Scripts/AI/`, `Tutorials/`, `TutorialInfo/`, `*_MLAgent` prefabs, `.nn` files and training scenes
- [x] 0.5 Installed `com.unity.netcode.gameobjects` 2.13.3 (+ Unity Transport) and `com.unity.multiplayer.playmode` 2.0.2
- [x] 0.6 Create `_EricRacer/` folders and the asmdefs
- [x] 0.7 Player Settings: company and product name, Windows x64 Mono, fullscreen window, resizable, `Run In Background = true` (essential for netcode)
- **DoD:** ✅ clean compile, Windows smoke build launches (Builds/Smoke)

### Phase 1: Drivable local kart (Fri morning, about 2 h)
- [x] 1.1 `KartInputActions` asset plus `InputSystemKartInput : BaseInput` (Strategy). Remove the legacy `Input` usages (A2). Reserve the **Power** (X / Space) and **Respawn** (Y / R) actions now
- [x] 1.2 `KartTuning` SO plus "Kid tuning" (slightly slower top speed, more grip, softer steering)
- [x] 1.3 `KartRespawner`: auto-respawn to the last checkpoint when flipped, fallen or stuck for 3 s, plus a button (R / gamepad Y)
- [x] 1.4 Cinemachine follow camera bound at runtime to the *local* kart
- **DoD:** ✅ (auto-verified) tuning applied, camera follows, fall + flip respawn, zero console errors · ⏳ **you:** drive a lap of `Race_Oval` with keyboard and gamepad

### Phase 2: Networking MVP (Fri, about 4 h). The riskiest phase, so it goes first
- [x] 2.1 `NetworkKart` prefab (`NetworkObject`, owner-auth `NetworkTransform`, interpolation). Remote karts are kinematic with input disabled
- [x] 2.2 `ConnectionManager` (state machine): Host / Join(ip) / Leave, connection approval (max 4, reject mid-race joins)
- [x] 2.3 `LanDiscovery`: the host broadcasts and clients list the hosts they find
- [x] 2.4 Spawn karts on grid slots (`TrackLayout.spawnPoints`), one per client
- [x] 2.5 **Skill:** `mp-test` (how I run a 2–4 player test via Multiplayer Play Mode and local builds)
- **DoD:** ✅ 3 local windows (host + 2 clients, auto-drive): all see all 3 karts, positions agree, grid slots correct, closing a client normally is clean · ⚠️ killing a client stalls the host on loopback (see Risks) · ⏳ **you (Fri):** real test PC + laptop over Wi-Fi incl. Task Manager crash test

### Phase 3: Race loop (Sat morning, about 4 h)
- [x] 3.1 `Checkpoint` and `TrackLayout` (ordered checkpoints, finish line, spawn grid), reusing the positions of the existing `LapObject`s
- [x] 3.2 `RaceProgress` (per-kart NetworkVariables) and server validation
- [x] 3.3 `RaceStateMachine` + states: Lobby (all ready) → Countdown (synced to `ServerTime`, reusing `RaceStart.playable`) → Racing → Results
- [x] 3.4 Live positions (1st, 2nd and so on), finish detection, results, "Race again" back to Lobby without reconnecting
- [x] 3.5 Delete the old `GameFlowManager`/`Objective`/`TimeDisplay` usage from our race scenes
- **DoD:** ✅ editor: grid→countdown→GO unlock, wrong-gate ignored, 3 laps, finish, results, race again · ✅ 3 windows: grid waits for all, synced countdown, client finish validated by server · ⏳ **you:** play a 3-lap race in the editor with a gamepad

### Phase 4: Title, 3D Lobby and HUD (Sat afternoon, about 4 h)
- [ ] 4.1 **"Happy Birthday Eric!" title screen**: a 3D scene with a kart slowly spinning, a big title, balloons/confetti, then **Host** / **Join** (auto-list of found games) / name entry. Fully gamepad-navigable (Input System UI module)
- [ ] 4.2 `CharacterDefinition` / `CharacterCatalog` / `ICharacterView` (+ the `KartRacerCharacterView` adapter for Cat, Chicken and Knight, + `HumanoidCharacterView` ready for Pinoc). Include an (optional for now) `superPower` field
- [ ] 4.3 `PlayerSession` network object (name, characterIndex, slot, isReady)
- [ ] 4.4 **Fortnite-style lobby scene**: pedestals, live character browsing, ready badges, host track/laps picker and a START button
- [ ] 4.5 The chosen character drives the kart in the race (`driverPrefab` spawned into the kart seat)
- [ ] 4.6 HUD: big lap counter, position badge, 3-2-1-GO, name tags above karts
- **DoD:** title → lobby (4 players browsing and readying) → race → results → back to lobby, using only gamepads

### Phase 5: Birthday polish (Sat evening / Sun morning, timeboxed)
Ranked; we stop wherever time runs out:
0. **Superpower framework** (`SuperPowerDefinition`, `KartAbilityController`, `KartStatusEffects`, HUD cooldown meter) plus **Kamil's bomb** as the first power, made with existing assets. The current Cat/Chicken/Knight characters get the bomb or a placeholder "Eric transform" so it can be played and tested before the family models arrive
1. Gentle catch-up boost for whoever is in last place (so Eric always has a chance 😉) plus a podium victory animation on the results screen
2. Confetti finish for 1st place and gamepad rumble on bumps
3. Speed-boost pads (reusing `ArcadeKartPowerup`, made network-safe)
4. A second track
5. Audio polish (engine sounds for remote karts, music)

### Phase 6: Ship (Sun, about 3 h, **not optional**)
- [ ] 6.1 Build script (`EricRacer.Editor/BuildMenu`): one click builds Win64 into `Builds/` and zips it (a Linux target can be added later)
- [ ] 6.2 Test on **every** real machine, including the Windows firewall prompt ("Allow on private networks")
- [ ] 6.3 A one-page "How to play" for the adults (start host, join, controls)
- **DoD:** a 4-player race on the real hardware

### MVP cut line ✂️
If we fall behind, **Phases 0–3 + 4.1 (title) + a simple lobby (4.2–4.4 without the 3D stage polish) + 6** is a complete, playable birthday game. Everything else is bonus.

### Later (after the party)
- Family characters from Pinoc (pipeline above; zero code): Eric, Kamil, Liliana, Justyna
- Eric's truck/tractor transform with the real assets (swap the placeholder prefab in `TransformPower` asset)
- Liliana's and Justyna's superpowers (new `SuperPowerDefinition` subclass + asset each)
- Linux build target
- More tracks

---

## 5. Risks and mitigations

| Risk | Mitigation |
|------|-----------|
| Guest Wi-Fi or "client isolation" blocks LAN discovery | Manual IP join fallback. Ideally everyone is on the main Wi-Fi, or a phone hotspot as plan C |
| Windows Firewall blocks the host | Test in Phase 2. Click *Allow* on first launch (private network) |
| **A crashed/killed game stalls the host for everyone** (found Phase 2) | Reproduced on one PC: after a client process is *killed*, the host stops receiving from all clients until they time out. This is the Windows UDP "port unreachable / connection reset" behaviour, not handled by Unity Transport 2.6.0. Closing a window normally is fine. Mitigation: disconnect timeout lowered to 10 s. Expected not to happen between real PCs (Windows Firewall stealth mode suppresses the ICMP reply), **verify in the real-hardware test** by ending a laptop's game in Task Manager mid-race |
| Weak laptop GPUs | Quality presets, Low as the default on laptops; test a build on the weakest laptop on Friday |
| Owner-auth physics looks jittery for remote karts | NetworkTransform interpolation plus a higher tick rate (60) on the LAN |
| Scope creep | MVP cut line. Phase 5 is strictly ranked and timeboxed |

## 6. Conventions
See `CLAUDE.md` at the project root.
