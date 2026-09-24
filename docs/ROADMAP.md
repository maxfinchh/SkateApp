# Remaining work

## Next priorities

- Verify the latest rider scale (1.4), push angle (+8° import offset), shortened push loop (frames 0–50), and foot placement on the board in Play mode.
- Add rider animations for carve, ollie/airborne, landing, grind, manual, crash, and final launch. Coordinate board flips with the rider's feet. Coast and push already work through `IsPushing`.
- Test push responsiveness versus quick diagonal flick recognition on a real iPhone. Current input defaults: push delay 0.14s, swipe grace 0.36s, flick window 0.46s, ollie width ratio 0.32. Check serialized scene settings before changing defaults.
- Make manual balance interactive; the current sweeping marker cannot cause a balance failure.
- Support clean airborne rail landings from separate kickers while keeping side/low impacts penalized.

## Art and presentation

- Choose a consistent street/skatepark style and collect visual references.
- Replace the board and remaining primitive environment assets, starting with rails, ramps, obstacles, and coins.
- Polish the existing start/HUD/results flow; add dedicated upgrade, cosmetic selection, and settings screens when needed.
- Add sound, music, haptics, and more polished trick/reward feedback.
- Decide cosmetic progression and monetization before implementing purchase/ad flows.

## Technical follow-through

- Update the scene builder to reproduce the animated player before relying on it to rebuild the scene.
- Validate native iPhone builds, touch behavior, safe-area UI, and frame rate. Compare fallback control schemes only if useful; `PushAndFlick` is the current default.
- Exercise restart, final bonus, reward persistence, rail/manual mistake recovery, and spawn spacing during playtests.
- Tune oversized prototype rails and character/collision proportions as final assets arrive.

The duplicate Unity projects and redundant planning logs were consolidated on September 24, 2026. Completed implementation history and superseded design notes remain in Git; this file tracks remaining work only.
