# SkateApp

A portrait iOS skateboarding runner built with Unity 6 and URP. The playable prototype includes push/carve controls, flick tricks, rails, manual pads, pooled obstacles, scoring, missions, upgrades, and a final mega-ramp bonus.

## Open and play

1. In Unity Hub, add the existing **`SkateRunnerUnity`** folder beside this README. Use Unity **6000.0.29f1** (see `ProjectSettings/ProjectVersion.txt`).
2. Open `Assets/Scenes/Prototype.unity` and press Play.
3. Click/tap to start. Hold to push, drag while holding to carve, release to coast, and use quick release flicks for tricks.

The former nested project was moved here on September 24, 2026. Remove stale Hub entries for the old nested path and `My project`; do not create another project. The first open after cleanup rebuilds Unity's generated cache and can take longer.

**Do not run `Skate Runner > Build Prototype Scene` just to play.** That developer tool rebuilds and overwrites the scene with primitive placeholders; it does not recreate the manually added animated skater.

## Layout

```text
SkateApp/
├── README.md                 Setup, controls, and code map
├── AGENTS.md                 Working notes for coding agents
├── docs/
│   ├── DESIGN.md             Game direction and current mechanics
│   └── ROADMAP.md            Remaining work and open decisions
└── SkateRunnerUnity/         The only Unity project
    ├── Assets/
    │   ├── characters/Skater/  Mixamo models, clips, Animator Controller
    │   ├── Materials/
    │   ├── Scenes/             Prototype.unity
    │   ├── Scripts/
    │   │   ├── Runtime/
    │   │   └── Editor/
    │   └── Settings/           URP renderer/pipeline/volume assets
    ├── Packages/               Package manifest and lockfile
    └── ProjectSettings/        Shared Unity configuration
```

Unity recreates `Library`, `Temp`, `Logs`, `UserSettings`, and IDE project files locally. These are ignored by Git. Keep asset `.meta` files: their GUIDs preserve scene, material, and animation references. `Assets/Settings/SampleSceneProfile.asset` is still used by the render pipelines despite its template name.

## Controls

The default scheme is `PushAndFlick`; `HoldDragSteer` and `SwipeLane` remain optional test modes.

| Input | Action |
| --- | --- |
| Hold / drag while holding | Push / carve |
| Release | Coast with brief speed carry |
| Quick upward flick | Ollie |
| Up-left / up-right flick | Kickflip / heelflip |
| Down-left / down-right flick | Shove-it / 360 flip |
| Approach rail's attached kicker head-on | Ride kicker into automatic grind |
| Ollie or flip into manual pad | Start manual and earn entry bonus |
| Up/diagonal flick during grind | Trick out |
| Repeated trick flicks during final launch | Add airtime, distance, and score |
| Click/tap after results lockout | Restart |

Editor debug keys: A/D or arrows for lane changes; W/up/space for ollie; Q/E for flip; R for shove-it; F for 360 flip; G for grind. After a run, 1–4 purchase speed/pop/trick/coin upgrades when affordable. Test push animation with a held mouse press in the Game view.

## Code and animation map

Runtime scripts live in `SkateRunnerUnity/Assets/Scripts/Runtime`:

- `GestureInput`: mouse/touch recognition and control schemes.
- `PlayerController`: movement, tricks, grind/manual behavior, and the Animator's `IsPushing` Boolean.
- `SkateRunnerGameManager`: run states, scoring, restart, and final bonus flow.
- `ObstacleSpawner`, `PooledObject`, `RoadSegmentLooper`: world generation, reuse, and road recycling.
- `GrindRail`, `RailEntryKicker`, `SkateFeature`, `RunnerObstacle`, `CoinPickup`: world interactions.
- `HUDController`, `FollowCamera`: UI and camera.
- `MissionTracker`, `PlayerUpgrades`, `ChaserPressure`: progression and mistake pressure.

The scene's `Skater Player/Trick Root/Skater_Crouch` is the visible rider. Despite the filename, `Skater_Crouch` provides **coasting**. `Skater_Push` provides pushing; `Skater_CrouchIdle` is imported but not connected. `SkaterAnimator` switches Coast ↔ Push with `IsPushing`. Root motion is disabled because the player script controls travel.

Current visual tuning: rider scale **1.4** on all axes; push clip frames **0–50**, looping with Loop Pose, original orientation with an **8° offset**. These are feel adjustments, not final art. The capsule is disabled and the board remains a placeholder. Body animations for airborne tricks, grinds, manuals, and crashes still need work.

## iPhone testing

Install iOS Build Support for the matching Unity editor. Open the existing Prototype scene, use `Skate Runner > Configure iOS Prototype Settings`, then build an Xcode project through Unity's Build Profiles. Open that output in Xcode, select your signing team and iPhone, and run. Keep build output in an ignored `Builds/` folder.

Unity Remote 5 on the phone can help test touch input but streams the editor view; use a native device build to assess performance.

## Validation

After changes, let Unity compile and inspect the Console. Play-test start, coast/push switching, flicks, rail entry/exit, manual entry/misses, final bonus, and restart as relevant. There is no dedicated gameplay test suite yet. Generated `.csproj` files can also be built with `dotnet build` once Unity has generated them, but that does not verify animation appearance or gameplay.

See [design](docs/DESIGN.md) and [remaining work](docs/ROADMAP.md).
