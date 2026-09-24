# Game direction

## Experience

An iOS-first, portrait, behind-the-skater arcade runner. The goal is expressive skate tricks and forgiving gesture input, not a full physics skate simulator. Favor readable silhouettes, smooth movement, bright rewards, large hazards, and clear feedback at phone size.

The loop is: start → push/carve/trick → avoid hazards and collect rewards → finish with an exaggerated mega-ramp bonus → receive gold and mission rewards → upgrade → replay. The road recycles; three lanes support spawning and fallback controls, while default push-and-drag movement carves continuously across the road.

## Current mechanics

- Holding pushes and accelerates; releasing briefly carries speed, then returns toward cruising speed. Quick flicks trigger ollies and board tricks. Input sensitivity and push-versus-flick recognition remain tuning priorities.
- Normal flip tricks rotate the board. Whole-body spins are reserved for the final bonus. The rider currently has coast and push animations only.
- Rails are deliberately forgiving prototype geometry. Their attached kicker provides head-on entry, follows the ramp surface, and launches once at its edge. Grinds stay locked to rail height and end at the physical endpoint; tricking out earns a bonus. Side/low impacts cause security mistakes.
- Manual pads are long, two-lane features with an open bypass lane. An ollie or flip before landing enters a manual with bonus points. Misses shove/slow the player and have a repeat cooldown. The displayed balance marker is currently automatic.
- Kicker ramps and bounce stairs launch the skater; a timed trick input earns the feature reward. Spawn reservations prevent rails, manual pads, their kickers, and pickups from overlapping unfairly.
- Coins, trick/combo scoring, missions, and gold fund speed, pop, trick-value, and coin upgrades. Repeated rail/mistake pressure can trigger failure. The negative-pickup system exists, but the teal negative hazard was removed from regular spawning pending redesign.
- Run failure leads into a visible final mega-ramp: auto-center, launch, repeated trick input for extra airtime/distance/score, then results. The baseline targets at least three seconds of airtime. A restart lockout keeps final trick spam from skipping results.
- A Canvas provides the start panel, run HUD, event feedback, final-air meter, results, missions, and upgrade feedback. This is prototype UI, not finished branding.

## Art direction

Use downloaded assets for early style tests, with custom Blender work later for a distinctive skater silhouette, boards, and signature set pieces. Current rider assets come from Mixamo; environment and board geometry are mostly primitives.

Priorities: readable skateboard and skater proportions, synchronized rider/board animations, then rails/ramps/obstacles, street props, pickups, sound, effects, and UI polish. Keep the visual language consistent and hazards obvious at mobile scale.

Potential hazards include construction barriers, scooters, trash stacks, security blockers, wet concrete, road signs, open manholes, and bike racks. Scoring opportunities can include stairs, ledges, grind rails, manual pads, banks, wallrides, and coin lines. These are ideas, not all implemented features.

## App flow target

- Main menu: prominent character/board preview, Play, gold, missions, and small upgrade/cosmetic/settings entry points.
- Run HUD: score, coins, combo, current trick, mission/security feedback; gesture-driven gameplay.
- Final bonus/results: launch distance and tricks, separated rewards, mission progress, upgrades, and clear replay.
- Later: dedicated upgrades, skater/board selection, sound/music/haptics/sensitivity settings, and purchase restoration only if purchases are introduced.

Street versus skatepark styling, cosmetic strategy, and monetization remain open. The mega-ramp is the implemented final bonus; alternatives such as a stair set or rail chain are future design options.
