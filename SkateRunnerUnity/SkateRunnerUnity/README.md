# Skate Runner Unity Prototype

This is the Unity project shell for the first Skate Runner prototype.

Unity batch project creation could not complete from Codex because the local Unity licensing client timed out, so the initial project files were scaffolded manually. The real Unity Hub project now lives at:

`/Users/maxfinch/Documents/SkateApp/SkateRunnerUnity/SkateRunnerUnity`

Then run:

`Skate Runner > Build Prototype Scene`

That menu item creates `Assets/Scenes/Prototype.unity` with a playable Subway Surfers-style skate runner prototype.

## Prototype Controls

- Left/right swipe: change lanes
- Up swipe: ollie
- Up-left swipe: kickflip
- Up-right swipe: heelflip
- Down-left swipe: shove-it
- Down-right swipe: 360 flip
- Kicker ramps and bounce stairs pop the player up; swipe during the trick window to score from them
- Final mega-ramp: spam diagonal trick swipes in the air to add distance
- Diagonal swipe and hold near rail: grind
- Release hold: exit grind
- Tap/click after game over: restart

Mouse drag works in the Unity editor for quick testing.

## Editor Debug Controls

- A / left arrow: lane left
- D / right arrow: lane right
- W / up arrow / space: ollie
- Q / E: kickflip
- R: shove-it
- F: 360 flip
- G near rail: hold grind, release to exit
- After game over, 1/2/3/4 buy speed/pop/trick/coin upgrades if enough gold

## Progression Prototype

- Missions track coins, tricks, grinds, skate spots, and final launch distance.
- Run rewards convert score, coins, and mission completions into gold.
- Long/sketchy grind exits build security pressure; repeated sketchy exits before it decays trigger the final fail flow.
