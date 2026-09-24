# Skate Runner Game Design

## New Chat Handoff

- Active Unity project path: `/Users/maxfinch/Documents/SkateApp/SkateRunnerUnity/SkateRunnerUnity`.
- Main gameplay scripts are in `Assets/Scripts/Runtime` inside that nested Unity project.
- The older outer `SkateRunnerUnity/Assets` scaffold is not the active Unity project.
- Current prototype is placeholder primitives, not final art.
- Default control scheme is `PushAndFlick`: tap/click to start, quick release flicks trigger tricks, holding briefly starts pushing, drag while holding carves across the road, head-on trick entry starts rails automatically, double tap queues manuals, release coasts.
- Latest input tuning: push starts after `0.14s`, trick swipes have `0.36s` grace before hold/push dominates, quick flick max is `0.46s`, and ollie/down inputs use a moderate straight zone so kickflip/heelflip do not eat every diagonal.
- Current highest-priority tuning risk: kickflip/heelflip recognition versus push responsiveness. Test this on phone before locking controls.
- Current second priority: rail/manual feel, especially trick-to-grind and soft mistake recovery.
- Do not start final character/art replacement until the basic control, rail, manual, scoring, and result loops feel good.

## Concept

A polished iOS skateboarding runner built around swipe-based trick input, obstacle avoidance, grinding, scoring, upgrades, and a final run-ending bonus moment.

The game should feel closer to an arcade skate runner than a fully free-form skate simulator. Inputs are gesture-driven and expressive, but the player does not need the precision or complexity of a full skateboarding simulation.

## Current Direction

- Platform: iOS first
- Engine preference: Unity
- Unity project path: `/Users/maxfinch/Documents/SkateApp/SkateRunnerUnity/SkateRunnerUnity`
- Render pipeline target: Universal Render Pipeline
- Visual target: clean, smooth, polished, high-quality mobile game
- Controls: gesture-only system, no visible gameplay buttons
- Camera style: portrait, behind-the-skater 3D lane runner like Subway Surfers
- Movement style: three-lane runner with automatic forward motion
- First prototype character: skater on a skateboard using placeholder primitives

## Core Loop

1. Start run
2. Skate forward automatically
3. Swipe to jump, flip, grind, dodge, and chain tricks
4. Avoid or clear obstacles
5. Collect rewards and build score multiplier
6. Trigger a final bonus moment when the run ends
7. Earn coins/gold based on distance, tricks, combo, grind time, and final performance
8. Upgrade board, character, stats, cosmetics, and trick rewards
9. Run again

## Controls

The game should avoid on-screen buttons during gameplay.

Possible gesture ideas:

- Up swipe: ollie / jump
- Diagonal swipe: kickflip
- Left/right swipe: lane shift
- Down swipe: crouch, manual, or prepare for grind
- Quick release swipe before the push delay takes over: ollie or board trick
- Diagonal release swipe while lined up head-on with a rail: trick into automatic grind
- Double tap shortly before a manual pad: manual
- Release hold or reach rail end: drop out of grind
- Flick up/diagonal during grind: trick out for extra points

Gestures should be forgiving and arcade-friendly. The goal is stylish, readable trick input, not a hardcore simulator.

## Trick System

Tricks should reward timing and chaining.

- Kickflip
- Heelflip
- Shove-it
- Ollie
- Grind
- Manual
- Combo chains
- Clean landings
- Longer grind time for more points
- Risk/reward multiplier for late tricks near obstacles

## Obstacles

Obstacle ideas:

- Traffic cones
- Construction barriers
- Trash cans
- Benches
- Rails
- Curbs
- Stairs
- Moving pedestrians
- Security guards
- Cars / scooters
- Puddles or oil slicks
- Breakable crates

Some obstacles should be hazards, while others should be trick opportunities.

## Final Moment

The end of a run should trigger a short bonus sequence instead of simply showing a game over screen.

Possible version:

- The skater reaches a final ramp, stair set, rail, or boss challenge
- Distance, speed, combo, and trick score convert into launch power or bonus strength
- Player performs one last timed swipe sequence
- Rewards are based on final distance, style, landing quality, and combo multiplier

This should be the game's equivalent of the satisfying end-of-run payoff common in mobile runner games.

## MVP

First playable version should include:

- Unity project scaffold
- Portrait behind-the-skater lane runner
- Forward auto-skate movement
- Swipe gesture detection
- Jump / ollie
- One flip trick
- One grind interaction
- Basic obstacles
- Collision / fail state
- Score
- Coins
- Restart flow
- Simple final bonus moment

## Open Questions

- Realistic skatepark theme, street theme, or exaggerated arcade world?
- Premium game, ad-supported, in-app purchases, or no monetization initially?

## Implementation Notes

- Unity command-line project creation failed in Codex because the local licensing client timed out.
- The project has been scaffolded manually and should be opened through Unity Hub.
- After opening `/Users/maxfinch/Documents/SkateApp/SkateRunnerUnity/SkateRunnerUnity`, run `Skate Runner > Build Prototype Scene` to generate `Assets/Scenes/Prototype.unity`.
- Mouse drag works in the Unity editor for gesture testing.
- Keyboard debug controls exist for fast Unity Play Mode testing, but final mobile gameplay should stay swipe-only.
- Obstacles use trigger hazards so collision reliably starts the final bonus/game-over flow.
- The road is now built from recycled segments so the runner can continue indefinitely until a fail/final-bonus trigger.
- The final bonus is now a visible mega-ramp sequence: obstacle hit clears the lane, spawns a ramp ahead, auto-centers the skater, launches, spins, awards distance bonus, then ends the run.
- The prototype now increases speed over time and uses denser obstacle/coin patterns.
- Non-lethal score features now include kicker ramps, stair gaps, and manual pads.
- Hazard concepts now include construction barriers, parked scooters, trash can stacks, security blockers, and wet concrete blockers.
- Diagonal swipes now map to multiple board tricks: kickflip, heelflip, shove-it, and 360 flip.
- Art direction and asset sourcing notes live in `ART_ASSET_PLAN.md`.
- Normal tricks rotate only the board; full skater/body flips are reserved for the final mega-ramp bonus.
- Missions track coins, tricks, grinds, non-lethal feature hits, and final launch distance.
- Gold rewards come from coins, score, and completed missions.
- First upgrade stats are speed, pop/jump, trick value, and coin bonus.
- First chaser/security mechanic uses rail clips/manual misses/negative pickups to build pressure; repeated mistakes before pressure decays trigger the final fail flow.
- Kicker ramps and bounce stairs boost the player only; the player must swipe during the trick window to score a trick from them.
- The final mega-ramp awards extra distance for repeated diagonal trick swipes while airborne.
- Gesture input currently uses the original easier diagonal recognition because the stricter lane-priority version made kickflips and heelflips too hard to trigger.
- Long-term control direction should keep lane movement reliable while making tricks context-sensitive and easy: normal lane swipes for dodging, diagonal swipes for tricks, queued diagonal-hold for rails, and special feature windows for ramps/stairs.
- Current control sandbox defaults to `PushAndFlick`: tap to start, hold briefly to push/accelerate, drag while holding to carve responsively across the road, release to stop pushing while briefly preserving speed before slowly coasting down, and quick release flicks for ollies/tricks. Holding no longer fires normal tricks or grind input in this mode because holding represents pushing. The push start has a short `0.14s` delay so fast swipes can resolve as tricks before steering/pushing begins. `HoldDragSteer` and `SwipeLane` remain available as fallback test modes.
- Double tap is now the manual-pad input. Manual pads no longer accept generic ollie/trick entry; without a queued double tap, contact is a manual miss/security knock.
- In `PushAndFlick`, straight ollies and straight down inputs have more room than the last tuning pass, while obvious horizontal swipes stay lane/carve input and diagonal swipes still reach kickflip/heelflip/shove-it/360 flip.
- The final mega-ramp should borrow the satisfying mobile-runner payoff pattern from games like Twerk Race/Run: visible ramp, exaggerated launch, repeated input for extra reward, and a clear win/reward screen, but with skating tricks instead of copying their theme.
- The final mega-ramp now targets at least 3 seconds of baseline hangtime, and repeated swipes during the launch add air tricks, vertical lift, forward speed, score, and distance.
- The final mega-ramp HUD now includes a distance/trick readout, power meter, and pulse feedback when the player swipes for extra air.
- The run-complete screen now has a short restart lockout so final-ramp spam swipes do not immediately start the next run before the player can read stats.
- Run-complete results hide the active gameplay HUD and summarize launch distance, air trick count, coins, gold, and mission progress.
- After the final-ramp landing, the skater and camera are centered for the result moment so the stop state feels intentional.
- Rail misses are now mistake states instead of instant fails: clipping or dragging into the side of a rail bumps the skater back/sideways and adds security pressure immediately; two mistakes within the pressure window trigger the chaser/fail flow.
- Rails now use simple head-on entry: line up with the rail, do a trick before contact, and the rail automatically starts a grind. Side approaches or contact without the trick buffer cause a rail clip/security mistake.
- Rails are intentionally oversized in the prototype so the player can learn the trick-on timing before real art narrows the visuals.
- Diagonal trick flicks now briefly buffer rail entry, so a trick aimed at an upcoming rail can become a grind even if the hold timing is not frame-perfect.
- Grinding now has two exits: release/reach rail end drops the skater off with no penalty, while flicking up or diagonal during the grind performs a trick out and awards extra points.
- Long rails and manual pads reserve physical spawn space so other obstacles/features do not appear inside them.
- Manual pads are long two-lane reward features with one open edge lane. Double tapping shortly before the pad starts a manual and shows a balance bar; rolling, landing, or swerving onto one without that input causes a manual miss/security mistake instead of ending the run.
- Manual-pad misses now shove the skater sideways/backward, slow forward speed, and behave like a soft recovery mistake locked out for 15 seconds, so approaching a pad flat or from behind does not trigger the final mega-ramp.
- First negative pickups are loose gravel, wet paint, sketchy cracks, security cones, and mud patches. They reduce score/combo/control briefly instead of ending the run.
- Coin pickup, trick, final-air, rail clip, and fail feedback now route through larger event pop text; rail/fail moments also flash the screen.
- Spawned hazards, coins, rails, ramps, stairs, manual pads, final-ramp pieces, and bonus markers are pooled instead of destroyed during normal cleanup.
- Prototype HUD now uses a generated Canvas instead of small legacy IMGUI labels.
- Current UI target is clean hyper-casual runner readability: big result stat, separated rewards, mission progress, and upgrade costs. This is not final visual branding yet.
- The run-complete UI now uses a structured results panel with header, final launch stat, reward rows, mission strip, upgrade strip, and restart-lock footer.
- The prototype now has a generated start panel with click/tap-to-play before the first run.
- Restart now resets the player, spawner, recycled road segments, and follow camera so replay starts on a clean road.
- App screen planning lives in `APP_SCREEN_PLAN.md`; the near-term UI goal is a clean mobile game flow, not a copied Donald Run layout.
- iPhone testing notes live in `IPHONE_TESTING.md`.
