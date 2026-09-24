# Project working notes

- Read `README.md` for setup, controls, current animation tuning, and the code map. Product direction and unfinished work live in `docs/DESIGN.md` and `docs/ROADMAP.md`.
- The only Unity project is `SkateRunnerUnity/`. Runtime code is in `SkateRunnerUnity/Assets/Scripts/Runtime/`. The older outer scaffold and nested `My project` were removed; do not recreate nested Unity projects.
- `Assets/Scenes/Prototype.unity` is the current hand-edited playable scene. `PrototypeSceneBuilder.BuildPrototypeScene` overwrites it and recreates primitive placeholders without the animated rider. Do not use the builder as routine setup or verification. If changing scene generation later, preserve the animated setup explicitly.
- Keep Unity assets and their `.meta` files together when moving or renaming them. Preserve GUIDs and scene overrides. Avoid editing scene files while the user has unsaved Unity scene changes.
- Serialized scene/import settings can override C# defaults. Check both before tuning behavior. `Skater_Crouch` is the coast model/clip, not the separate unused `Skater_CrouchIdle` asset.
- The CharacterController owns movement; the rider Animator only poses the body. Keep root motion disabled. Board spins and body animation have separate responsibilities.
- Current input is `PushAndFlick`. Rails use attached entry kickers; manuals use ollie/flip entry. Older double-tap manual and trick-only rail notes were superseded.
- Favor playable, readable changes and forgiving gesture controls. Keep visible gameplay buttons out unless requested.
- Use appropriate Unity compile/Play-mode checks. Report when visual/device validation has not been performed. Do not regenerate scenes just to test code.
- Keep docs concise and current; update existing docs instead of adding duplicate handoff/history files. Generated Unity caches, builds, and IDE files stay out of Git.
