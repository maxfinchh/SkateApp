# Skate Runner TODO

## New Chat Handoff

- Active Unity project: `/Users/maxfinch/Documents/SkateApp/SkateRunnerUnity/SkateRunnerUnity`.
- Active runtime scripts: `/Users/maxfinch/Documents/SkateApp/SkateRunnerUnity/SkateRunnerUnity/Assets/Scripts/Runtime`.
- Root planning docs: `/Users/maxfinch/Documents/SkateApp/GAME_DESIGN.md` and `/Users/maxfinch/Documents/SkateApp/TODO.md`.
- Current default controls are `PushAndFlick`: quick release flicks do tricks; holding briefly starts push/carve.
- Latest input values in `GestureInput`: `pushStartHoldSeconds = 0.14`, `pushSwipeGraceSeconds = 0.36`, `pushFlickMaxSeconds = 0.46`, `pushOllieWidthRatio = 0.32`.
- Current feel target: push should start fast enough to not feel laggy, but kickflip/heelflip should be easy from angled quick flicks.
- Do not edit the older outer scaffold at `/Users/maxfinch/Documents/SkateApp/SkateRunnerUnity/Assets` unless explicitly requested.
- Next likely work: phone-test trick recognition and push feel, tune rail/manual interaction timing, then begin basic animation planning before final art.

## Next Decisions

- Choose initial street/skatepark art direction.
- Decide whether the final bonus moment is a mega ramp, stair set, rail chain, or rival challenge.
- Decide first upgrade categories.

## Prototype Tasks

- [x] Create Unity project at `SkateRunnerUnity/SkateRunnerUnity`.
- [x] Add Unity package manifest with URP dependency.
- [x] Add editor scene builder.
- [x] Add player controller with automatic forward movement.
- [x] Add swipe gesture recognizer.
- [x] Add jump / ollie gesture.
- [x] Add kickflip gesture.
- [x] Add grind rail interaction.
- [x] Add simple obstacle spawning.
- [x] Add collision / fail state.
- [x] Make obstacle collisions reliable with trigger hazards.
- [x] Add score, coin, combo, and grind scoring.
- [x] Add restart flow.
- [x] Add first final bonus state.
- [x] Add visible final mega ramp sequence with launch distance scoring.
- [x] Add recycled road segments so the road no longer runs out.
- [x] Add simple speed ramp over time.
- [x] Add denser obstacle/coin patterns.
- [x] Add non-lethal scoring features: kicker ramps, stair gaps, and manual pads.
- [x] Change stair gaps into bounce stairs.
- [x] Require trick swipes after kicker/bounce boosts instead of auto-tricking.
- [x] Add final mega-ramp spam-swipe distance bonus.
- [x] Tune final mega-ramp for longer airtime and a bigger trick-spam window.
- [x] Retune final mega-ramp to target at least 3 seconds of airtime before landing.
- [x] Let any final-ramp swipe add air trick boost, while normal gameplay keeps lane/trick separation.
- [x] Make lane swipes easier to hit by giving clear horizontal gestures priority over diagonal tricks.
- [x] Restore the easier original diagonal trick recognition after lane-priority tuning made kickflips/heelflips too hard.
- [x] Add a post-run restart delay so final-ramp spam swipes cannot instantly start another run.
- [x] Reset recycled road segments and follow camera when starting a new run.
- [x] Expand board trick logic beyond one kickflip.
- [x] Add more skater-readable hazard concepts.
- [x] Add first mission tracker.
- [x] Add first gold reward and upgrade stats.
- [x] Add first security/chaser pressure mechanic.
- [x] Add missed-rail mistake logic: missed grind bumps the player back and two rail mistakes within 15 seconds trigger the chaser/fail flow.
- [x] Add rail trick-on targeting so diagonal-hold can flip onto rails from behind or adjacent lanes.
- [x] Make grind rails longer and more forgiving to target.
- [x] Add rail clip feedback text and a screen flash for mistake/fail feedback.
- [x] Make manual pads longer, add coin lines on top, and turn missed manual entries into security-trip mistakes instead of hard fails.
- [x] Retune rails longer/easier, make manual-pad misses non-lethal with a 15-second repeat lockout, and add first negative pickups.
- [x] Add keyboard debug controls for Unity Play Mode testing.
- [x] Open project in Unity Hub.
- [ ] Run `Skate Runner > Build Prototype Scene`.
- [ ] Press Play and tune gesture feel.
- [ ] Configure iOS module/build target from Unity Editor if needed.
- [ ] Clean up the duplicate outer scaffold folder after the nested Unity project is confirmed working.
- [ ] Replace placeholder cubes/capsules with actual skateboarder, board, ramp, rail, coin, and obstacle art.
- [x] Replace IMGUI prototype HUD with a mobile-safe UI Canvas.
- [ ] Add first app screen flow: main menu, run HUD, run complete, upgrades, skater/board select, settings.
- [ ] Test touch controls on a real iPhone through Unity iOS build or Unity Remote.
- [x] Add iPhone testing notes.
- [x] Add Unity menu item for iOS prototype settings.
- [x] Add object pooling for spawned coins, hazards, rails, ramps, and markers to reduce runtime allocations.
- [x] Add a visible final-ramp power/trick meter inspired by mobile runner bonus stages.
- [x] Hide active gameplay HUD during run-complete results.
- [x] Center the skater and camera on the result moment after the final-ramp landing.
- [x] Add coin/trick/final-air pop text through the event feedback panel.
- [x] Replace the run-complete debug block with a structured results panel, mission strip, upgrade strip, and restart-lock footer.
- [x] Label the final-ramp meter with distance, air tricks, and air power for clearer bonus-stage feedback.
- [x] Prototype a cleaner long-term control model where tricks are easy but movement remains reliable.
- [x] Add toggleable `HoldDragSteer` vs `SwipeLane` control sandbox.
- [x] Add temporary hold-drag control hint HUD.
- [x] Replace default lane-hold sandbox with push-and-flick controls: hold to push, drag to carve, flick to trick.
- [x] Add first click/tap-to-play start screen.
- [x] Make manual-pad misses shove the skater back/sideways and slow forward speed.
- [x] Retune push-and-flick speed so release coasts instead of braking hard.
- [x] Add a short diagonal trick buffer so rails are easier to grind without perfect hold timing.
- [x] Add push-speed carry so the skater keeps speed long enough to chain tricks after releasing.
- [x] Make rails drop the skater automatically at rail end, with flick-to-trick-out for bonus points.
- [x] Slow push acceleration so top speed takes longer to reach.
- [x] Make push-drag carving more responsive with less thumb travel.
- [x] Change `PushAndFlick` so holding only pushes/carves and tricks only fire from quick release flicks.
- [x] Reserve spawn space around long rails and manual pads so they do not overlap other physical features.
- [x] Add a push-start delay so quick swipes do not instantly become pushes.
- [x] Make ollie a narrow straight-up flick and give kickflip/heelflip wider diagonal input zones.
- [x] Tighten push-start delay after first test felt too slow.
- [x] Let held diagonal push gestures queue or snap into rail grinds without firing normal tricks.
- [x] Rebalance push-flick direction zones so straight ollies/down inputs recover space from kickflip/heelflip.
- [ ] Phone-test hold-drag controls against swipe-lane controls before locking final controls.
- [ ] Create basic animation plan for push, carve, ollie, board tricks, grinds, crash, and final mega-ramp.

## Design Tasks

- [x] Define first trick scoring formula.
- [x] Define first combo multiplier rules.
- Define obstacle categories.
- Define upgrade categories.
- [x] Define first final bonus moment rules.
- [x] Create first art/asset plan.
- [x] Create first app screen plan.
- Collect visual references for art direction.

## Notes For Codex

- Keep this file updated as design decisions become firm.
- Favor implementation steps that produce playable results quickly.
- Avoid adding visible gameplay buttons unless explicitly requested.
- Prioritize smooth gesture feel and readable game feedback.

## Current Prototype Controls

- Default sandbox mode is `PushAndFlick`.
- Click/tap to start the run.
- Hold briefly: push and accelerate.
- Drag while holding: carve smoothly across the road.
- Release: stop pushing, carry speed briefly, then coast down slowly to cruising speed.
- Quick release flick straight up: ollie.
- Quick release angled up-left/up-right flick: kickflip/heelflip.
- Quick release down-left/down-right flick: shove-it/360 flip.
- Hold a diagonal drag while pushing near a rail: flip onto the rail, or queue a short grind-entry buffer.
- Release hold while grinding or reach rail end: drop from rail with no penalty.
- Flick up/diagonal while grinding: trick out for bonus points.
- `HoldDragSteer` fallback mode maps screen thirds to lanes.
- `SwipeLane` fallback mode keeps the old left/right lane swipes and diagonal trick swipes.
- Tap/click after game over: restart.

## Current Editor Debug Controls

- A / left arrow: move left.
- D / right arrow: move right.
- W / up arrow / space: ollie.
- Q / E: kickflip.
- R: shove-it.
- F: 360 flip.
- G near rail: hold grind, release to drop.
- W/up/space or Q/E/R/F while grinding: trick out.
- After run ends, 1/2/3/4 buy speed/pop/trick/coin upgrades if enough gold.
