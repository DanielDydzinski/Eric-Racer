---
name: mp-test
description: How to test Eric Racer multiplayer on one PC — build a dev player, run a host plus clients that auto-drive, and compare their logs. Use after any change to networking, spawning, race state or anything synced.
---

# Multiplayer test on one PC

## 1. Build
`manage_build action=build target=windows64 output_path=Builds/Dev/EricRacer.exe development=true`.
The MCP disconnects while building. Wait for a new `Build Finished, Result` line in `%LOCALAPPDATA%/Unity/Editor/Editor.log` (run a background `until grep` loop).

## 2. Run
```
powershell -ExecutionPolicy Bypass -File .claude/skills/mp-test/run-local.ps1 -Clients 2 -Seconds 25
```
This starts one `-host` window and N `-join 127.0.0.1` windows, all with `-autodrive -netlog`. Each window writes its own log to `Builds/Dev/logs/*.log`. The script stops them and prints the key lines.

Launch switches (see `LaunchOptions`): `-host`, `-join <ip>`, `-name <n>`, `-autodrive` (bot driver), `-netlog` (`[NetSync]` lines every 2 s).

## 3. Read the results
- `[Connection] Hosting` / `[Connection] Connected` on every window, and no `Exception`.
- `[NetSync]` lines: each window lists every kart. `kartN*` is the kart that window owns. At similar times, the same kart's position should match across windows (allow about 1 m for interpolation delay at speed).
- The kart count equals the player count on every window.
- Leaving: kill a client mid-run and check the host still logs NetSync with one kart fewer.

## 4. Things a real-network test still needs (ask the user)
- The Windows Firewall prompt on the host's first launch: "Allow" on private networks.
- LAN discovery across two real PCs (broadcasts don't prove much on one machine).
- How it feels on real gamepads.
