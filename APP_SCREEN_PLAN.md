# Skate Runner App Screen Plan

## Direction

The first complete app flow should feel like a polished arcade runner: fast to start, readable after a run, and clean enough to support upgrades, missions, and cosmetics without crowding the screen.

The Donald Run screenshots are useful as a rough category reference, but this project should avoid copying the cluttered hex-button look. Skate Runner should use a cleaner street/skate UI with strong contrast, large touch targets, and a clear character/board preview.

## Build Order

1. Stabilize the run loop, restart, scoring, missions, and performance.
2. Add a simple screen state system around the working gameplay.
3. Build the first clean menu and run-complete screens.
4. Add upgrade and board/skater selection screens.
5. Replace placeholder art and add real animation.
6. Polish transitions, effects, sound, and store-ready presentation.

## Core Screens

### Main Menu

- Large skater and board preview.
- Primary play button.
- Gold total.
- Active missions.
- Small entry points for upgrades, skater/board select, and settings.
- No gameplay controls here; this is just app navigation.

### Run HUD

- Score, coins, combo, current trick/event message, mission progress, and security pressure.
- Big enough to read on iPhone.
- No gameplay buttons because the run is swipe-only.

### Run Complete

- Final score.
- Final mega-ramp distance.
- Coins/gold earned.
- Mission completions.
- Upgrade prompts when affordable.
- Play again button.

### Final Bonus

- Big "mega ramp" or "final trick" moment.
- Visible power/trick feedback while airborne.
- Repeated swipes add distance, airtime, trick count, and bonus score.
- The payoff should feel like the end-stage reward screens common in hyper-casual runners, but it should stay skate-themed: launch distance, trick chain, landing quality, sponsor cash, or crowd hype.

### Upgrades

- Speed.
- Pop/jump height.
- Trick score multiplier.
- Coin bonus.
- Later: grind balance, security forgiveness, magnet, board handling.

### Skater / Board Select

- Character preview.
- Board preview.
- Locked/unlocked status.
- Cosmetic stats only if they are simple and understandable.

### Settings

- Sound.
- Music.
- Haptics.
- Sensitivity.
- Restore purchases later, only if in-app purchases are added.

## Timing

Do not over-invest in final UI art before the gameplay loop is stable. A basic screen shell can come soon, but real graphic design, animation, and store-level polish should wait until restart, gesture feel, scoring, missions, upgrades, and performance are reliable.
