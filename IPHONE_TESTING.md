# iPhone Testing Notes

## Current Status

The installed Unity editor has an `iOSSupport` folder at:

`/Applications/Unity/Hub/Editor/6000.0.29f1/PlaybackEngines/iOSSupport`

So the earlier `Native extension for iOS target not found` log line is not enough by itself to prove iOS Build Support is missing. It can appear while Unity scans native plugins for several targets.

## Fastest Real Device Path

1. In Unity, run `Skate Runner > Build Prototype Scene`.
2. Run `Skate Runner > Configure iOS Prototype Settings`.
3. Open Build Profiles or File > Build Settings.
4. Confirm iOS is selected.
5. Build the Xcode project.
6. Open the generated project in Xcode.
7. Select your iPhone and run.

This gives real touch, real device timing, and the closest feel to the final game.

## Unity Remote

Unity Remote 5 must be installed on the iPhone/iPad, not the Mac.

It is only useful for quick touch feel. It streams the editor view and sends touch input back, so it is not a real performance test.

If Unity Remote does not support the device or is unreliable, skip it and use the Xcode device build path.

