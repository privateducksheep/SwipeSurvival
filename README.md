# Swipe Survival

Swipe Survival is a choice-driven zombie survival game. Drag each card left or right to make a decision, then live with its effect on your food, water, ammunition, morale, and medicine. Keep every resource above zero and see how many days your group can survive.

## Download and play

Download the newest build from the **[Releases page](../../releases/latest)**. You do not need Unity to play.

| Device | Download | How to start | Status |
| --- | --- | --- | --- |
| Windows 10/11 (64-bit) | `SwipeSurvival-v1.0.2-Windows-x64.zip` | Extract the entire ZIP, then run `SwipeSurvival.exe`. Keep the `.exe`, `_Data` folder, and DLL files together. | Ready |
| macOS (Intel or Apple Silicon) | `SwipeSurvival-v1.0.2-macOS-universal.zip` | Extract the ZIP, then open `Swipe Survival.app`. | Ready |
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
2. Click or press a card and drag it left or right.
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

## Version

Current build: `1.0.2`

## Feedback

Found a bug or have an idea? Open an issue on the **[Issues page](../../issues)** and include your operating system plus the steps needed to reproduce the problem.
