# Swipe Survival

Swipe Survival is a choice-driven zombie survival game. Drag each card left or right to make a decision, then live with its effect on your food, water, ammunition, morale, and medicine. Keep every resource above zero and see how many days your group can survive.

## Download and play

Download the newest build from the **[Releases page](../../releases/latest)**. You do not need Unity to play.

| Device | Download | How to start | Status |
| --- | --- | --- | --- |
| Windows 10/11 (64-bit) | `SwipeSurvival-v1.0.2-Windows-x64.zip` | Extract the entire ZIP, then run `SwipeSurvival.exe`. Keep the `.exe`, `_Data` folder, and DLL files together. | Ready |
| macOS (Intel or Apple Silicon) | `SwipeSurvival-v1.0.2-macOS-universal.zip` | Extract the ZIP, then open `Swipe Survival.app`. | Ready |
| Web browser (desktop and phone) | — | Build the Web version (see below) and host the `Builds/Web` folder. | Not available yet |
| Linux | — | A Linux build must be exported from Unity first. | Not available yet |
| Android | — | An APK or Play Store build must be exported and tested first. | Not available yet |
| iPhone/iPad | — | An iOS build must be exported, signed with an Apple Developer account, and distributed through TestFlight or the App Store. | Not available yet |

### macOS security note

This independent build is not notarized by Apple, so macOS may block the first launch. Try Control-clicking the app and choosing **Open**. If macOS still reports that the app is damaged, open Terminal in the folder containing the app and run:

```sh
xattr -dr com.apple.quarantine "Swipe Survival.app"
```

Only bypass this warning for a build downloaded from this repository.

## How to play

1. Select **Play** from the main menu.
2. Click (or touch, on a phone) a card and drag it left or right.
3. Read the action shown on that side, then release the card to choose it.
4. Balance food, water, ammunition, morale, and medicine. If any resource reaches zero, the run ends.

One in-game day passes after every five decisions. Survive for as many days as you can and beat your high score.

## Run the project in Unity

This repository contains the source project for **Unity 6 (6000.0.49f1)**.

1. Install Unity Hub and Unity Editor `6000.0.49f1`.
2. Clone or download this repository.
3. In Unity Hub, choose **Add project from disk** and select the repository folder.
4. Open `Assets/Scenes/MainMenu.unity` and press Play.

Unity recreates ignored generated folders such as `Library` when the project opens for the first time.

## Building for another desktop platform

In Unity, open **File > Build Profiles**, install the required platform module if prompted, select the target, and choose **Build**. Test every platform-specific build on real hardware before publishing it.

## Screen sizes

The game adapts to any window or screen shape: resizable desktop windows, ultrawide monitors (up to 32:9), browser windows of any size, and phones or tablets in either orientation. The card and its text always stay fully on screen and clear of the resource icons, backgrounds fill the screen with no bars, and on phones the interface stays readable and easy to tap (the day counter moves under the resources on narrow screens).

## Building for the web

1. In Unity Hub, add the **WebGL Build Support** module to Unity `6000.0.49f1`.
2. In Unity, open **File > Build Profiles**, choose **Web**, and select **Build** (or run `BuildPlayer.BuildWebGL` with `-executeMethod`). The output goes to `Builds/Web`.
3. Upload the contents of `Builds/Web` to any static host, such as itch.io or GitHub Pages.

The page uses the `Responsive` template in `Assets/WebGLTemplates`, so the game fills the whole browser window, follows window resizes and phone rotation, and blocks page scrolling while you swipe. Decompression fallback is on, so it works on hosts that do not send compression headers. The Exit button is hidden in browsers and on phones because they cannot quit a game.

### Publishing on itch.io

1. Build for the web (above), then zip the contents of `Builds/Web` so `index.html` sits at the top of the zip:

   ```sh
   cd Builds/Web && zip -r -X ../SwipeSurvival-web.zip index.html Build TemplateData
   ```

2. On itch.io choose **Create new project**, set **Kind of project** to **HTML**, upload the zip and tick **This file will be played in the browser**.
3. Under **Embed options**, set the viewport to **960 x 600** (any size works; the game adapts to it), tick **Mobile friendly** (orientation: Default) and **Fullscreen button**, and leave **SharedArrayBuffer support** off.

Browsers keep the high score and volume for each upload, so players may start with a fresh high score after you upload a new build.

## Checking layouts (developers)

`BuildPlayer.BuildMacOSLayoutCapture` makes a separate development build that screenshots every scene at many window sizes. It uses its own app identifier, so it never touches the real game's saved settings or high score:

```sh
"Builds/LayoutCapture/Swipe Survival Layout Capture.app/Contents/MacOS/SwipeSurvival Layout Capture" -captureDir ~/Desktop/layouts
```

Add `-captureSizes 585x1266,1266x585 -capturePixelsPerPoint 1.5` to imitate a phone (here a 390 x 844 point iPhone), or `-logicCheck` to play through the swipe rules and write PASS/FAIL results to `logic-check.txt`.

## Version

Current build: `1.0.2`

## Feedback

Found a bug or have an idea? Open an issue on the **[Issues page](../../issues)** and include your operating system plus the steps needed to reproduce the problem.
