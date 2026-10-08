# Eric Racer: project conventions

LAN multiplayer kart racer (Unity 6000.3.6f1, URP, Netcode for GameObjects) built on the Karting Microgame. The master plan with phases and status is in `docs/PLAN.md`; keep its checkboxes up to date.

## Code layout
- All new code lives in `Assets/_EricRacer/Scripts/<Area>/` under namespace `EricRacer.<Area>`, in assemblies `EricRacer.Runtime` and `EricRacer.Editor`.
- Don't modify `Assets/Karting Package/**` scripts unless it's unavoidable. Keep any patch minimal and mark it with `// ERIC-PATCH: <reason>`.
- One responsibility per class. No `FindObjectOfType` in gameplay code; wire through serialized references, SO event channels or the spawning code.

## Patterns (use the ones that fit; don't force them)
- **Race and connection flow:** a state machine (`IState` with `Enter/Tick/Exit`), server-driven, with the state id synced through a `NetworkVariable`.
- **Kart control:** the `IInput` Strategy (`ArcadeKart` already consumes `IInput`). Decorators for assists.
- **Decoupling:** C# events, `NetworkVariable.OnValueChanged` and ScriptableObject event channels. UI subscribes and never polls the race logic.
- **Data:** ScriptableObjects (`CharacterDefinition`, `TrackDefinition`, `RaceSettings`, `KartTuning`, catalogs). Sync catalog **indices** over the network, never references or strings.
- **Authority:** each player owns their kart's movement. The server owns race state (laps, positions, timing). Clients report through ServerRpc and the server validates.

## Style
- `[SerializeField] private` fields with `m_` or camelCase matching the surrounding code. Public API is properties and methods.
- Unsubscribe in `OnDisable`/`OnNetworkDespawn` for every subscription.
- Few comments, just enough to explain *why*.

## Workflow
- Use the Unity MCP to edit scenes and prefabs, refresh, check compilation and read the console. Follow the `unity-change` skill.
- **Git:** never commit or push unless the user explicitly asks. Commit as the user, with **no** Claude co-author or "Generated with" lines.
- Target platform: Windows x64 (Mono) for now. Linux comes later, so avoid Windows-only APIs.
- Characters must be addable as data only (`CharacterDefinition` + `ICharacterView`). The user will import family models from Pinoc later.
