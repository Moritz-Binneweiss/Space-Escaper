# Space Escaper

An endless runner set in space, built for Android. Swipe to steer your ship across
three lanes, dodge asteroids and space debris, collect coins and chase your high
score. In the hangar you buy new ships and unlock skins.

Originally released on the Google Play Store in 2020, currently being reworked for
a re-release.

🎬 **[Release Trailer](https://youtu.be/XtdYrUWEQlk)** &nbsp;·&nbsp;
📺 **[ANIMO Games on YouTube](https://www.youtube.com/@animogames9579)** &nbsp;·&nbsp;
📷 **[@space_escaper on Instagram](https://www.instagram.com/space_escaper/)**

---

> [!NOTE]
> **Work in Progress** - as of October 2026
>
> This project is still actively being developed and may change frequently.

**Version:** 1.4.5 &nbsp;·&nbsp; **Unity:** 6000.6.0f1 &nbsp;·&nbsp; **Render Pipeline:** URP 17.6.0 &nbsp;·&nbsp; **Platform:** Android

---

## 🛠️ **Setup**

This repository uses **Git LFS** for textures, audio and models. Without LFS you
only get pointer files when cloning and the project will not open correctly.

```bash
git lfs install
git clone https://github.com/Moritz-Binneweiss/Space-Escaper.git
```

Then open the `Space-Escaper/` folder in **Unity 6000.6.0f1** and load the scene
`Assets/Scenes/Game.unity` - menu, hangar and gameplay all live in that one scene.

---

## 👥 **Contributors**

### **Current Team**

- **Moritz Binneweiß** - Developer, Designer, Sound Designer

### **Previous Contributors**

- **Anian Geist** - Lead Developer (v1.3.0)
- **Moritz Binneweiß** - Lead Designer (v1.3.0)
- **Julian Daniel Görner** - Main Menu Instrumental
- **Hayden Folker** - "Cloud Nine" In-Game Instrumental

---

## 📜 **Version History & Roadmap**

### **v2.0.0** | Re-Release _(October 15, 2027)_

- Re-Release to Google Play Store
- Featuring all updates from 1.2.0 through 2.0.0
- [ ] New store listing with a new package name
- [ ] Release signing with a dedicated keystore instead of the debug key
- [ ] Android App Bundle (.aab) and target API level 36
- [ ] Android version code increased with every release
- [ ] Optional: Google Play Games integration, e.g. for the leaderboard

#### **v1.9.2** | Revive Mechanic Reworked _(------- --, 2026)_

<small>

- [ ] Revive Mechanic working again
- [ ] Re-check ship prices after the revive rework

</small>

#### **v1.9.1** | Marketing Material _(------- --, 2026)_

<small>

- [ ] New trailer
- [ ] New marketing materials

</small>

### **v1.9.0** | New Gameplay Features _(------- --, 2026)_

- [ ] Double tap shield mechanic
- [ ] Verticality in level design
- [ ] Vertical swipe controls
- [ ] Mini tutorial system

### **v1.8.0** | Achievements & Collectibles _(------- --, 2026)_

- [ ] Achievements System (Ingame)
- [ ] Collectibles System
- [ ] 5 collectible logbooks
- [ ] 2 collectible plants
- [ ] 2 collectible star-system cards

### **v1.7.0** | Art, Visuals & Sound _(------- --, 2026)_

- [ ] Remaster all game objects
- [ ] Remaster all spaceships
- [ ] Ship tilts into lane changes
- [ ] Remaster obstacles
- [ ] Remaster collectible coins
- [ ] Remaster structures
- [ ] Remaster effects
- [ ] Restore the explosion distortion effect lost in the URP switch
- [ ] Icon remaster
- [ ] New fonts implementation
- 🔊 New sounds for:
  - [ ] Explosion
  - [ ] Button Press
  - [ ] Coin collected
  - [ ] Select Spaceship
  - [ ] Deselect Spaceship
  - [ ] Buy new Ship
  - [ ] Engine

### **v1.6.0** | Localization _(------- --, 2026)_

- [ ] German language support
- [ ] English language support

#### **v1.5.2** | Credits _(------- --, 2026)_

<small>

- [ ] Added Credits Screen

</small>

#### **v1.5.1** | UI Overhaul _(------- --, 2026)_

<small>
 
- [ ] UI overhaul
- [ ] Switch all UI text to TextMeshPro (needed for localization in v1.6.0)
- [ ] Safe area support for notches and camera cutouts
- [ ] Clean up unused UI images
- [ ] Decide what the leaderboard button should do (currently without function)

</small>

### **v1.5.0** | Refactoring _(------- --, 2026)_

- [ ] Reworked and Refactored all Game Engine and Development related things

#### **v1.4.6** | Swipe Controls _(------- --, 2026)_

<small>

- [x] Lane changes take the same time at any frame rate
- [x] Removed the unused ship animator and its tilt clips
- [ ] Swipe distance in millimeters, the same on every screen
- [x] Only mostly sideways swipes change lanes
- [ ] Input prepared for double taps and vertical swipes

</small>

#### **v1.4.5** | Pause, Code Style & 60 FPS _(October 7, 2026)_

<small>

- ⏸️ One shared pause state that only pauses during a run
- ⌨️ Removed the unused Escape key binding from the UI input
- 🔧 All scripts follow Unity's C# style guide
- 🏷️ Consistent names for scene objects and project files
- 📁 Project folders sorted by asset type
- 🗂️ Scene hierarchy grouped by role, unused components and tags removed
- 📱 Auto-pause when the app goes to the background
- ⚡ 60 FPS on Android instead of 30
- 🐛 Fixed the Unity logo showing as the app icon on Android
- 🏷️ App name is now "Space Escaper"

</small>

#### **v1.4.4** | Bug Fixes & Music Pause _(October 6, 2026)_

<small>

- 🐛 Fixed revive crashing the ship again right away
- 🐛 Fixed the blurry "Use new Sound" label in the settings
- 🎵 In-game music now pauses on death and continues after a revive

</small>

#### **v1.4.3** | Project Cleanup _(October 6, 2026)_

<small>

- 🗑️ Removed leftover Google Play Games files and the unused Google package registry
- 🗑️ Removed an unused duplicate settings component
- 🔧 Removed debug logs, empty update methods and unused imports
- 🏷️ Replaced the leftover tutorial app ID for desktop builds

</small>

#### **v1.4.2** | Input System & Engine Flame Fix _(October 5, 2026)_

<small>

- 🎮 Switched from the legacy Input Manager to the new Input System
- ⌨️ Added arrow key controls for testing in Play Mode
- 🐛 Fixed the missing engine flame on the Freeter

</small>

#### **v1.4.1** | Bug Fixes & Shop Prices _(October 5, 2026)_

<small>

- 🐛 Fixed purchased ships not being saved
- 🐛 Fixed invisible ship and crash on first launch
- 🐛 Fixed hangar showing the wrong ship and engine flame
- 🐛 Fixed highscore differing from the displayed score
- 🐛 Fixed coins being counted twice after a revive
- 🐛 Fixed missing click sounds on most buttons
- 💰 Rebalanced ship prices to the actual coin income (250 / 750 / 1250)

</small>

### **v1.4.0** | Audio System Rework _(September 10, 2026)_

- 🔊 Reworked the whole Audio System
- 🔀 Added audio toggle to switch between classic and new music/SFX
- 🎚️ Added master volume slider to the settings menu
- 🎵 New main menu music: **"Nerous Spaceport"**
- 🎵 New in-game music: **"Striving Through Space"**
- ⬆️ Switched Unity Version from **6000.2.6f2** to **6000.6.0f1**
- 🖼️ Switched Render Pipeline from **Built-in** to **URP 17.6.0**

#### **v1.3.2** | Refactoring _(January 18, 2026)_

<small>

- 🔧 Project refactoring and cleanup
- 🗑️ Removed Google Play Games integration
- 🗑️ Removed Unity Ads integration
- 🐛 Fixed MobileInput touch handling bug
- 🐛 Minor bug fixes and code improvements
- 📦 Cleaned up unused packages

</small>

#### **v1.3.1** | Unity Update _(January 17, 2026)_

<small>

- ⬆️ Updated project to Unity 6000.2.6f2
- 🔄 Migrated from Unity 2019.4.41f1
- 🛠️ Fixed deprecated API calls
- ✅ Ensured Unity 6 compatibility

</small>

### **v1.3.0** | UI & Vagor _(February 14, 2021)_

- 🎨 Full UI overhaul with improved layout and readability
- 🎨 Refined color palette for visual consistency
- 🚀 New playable spaceship: **Vagor**
  - 3 unique skins
  - Available in in-game shop
- 🛠️ Internal UI refactoring for future expansions

### **v1.2.0** | Leaderboard & Balancing _(November 13, 2020)_

- 🏆 Added global highscore leaderboard
- 💰 Rebalanced in-game shop prices
- ⚙️ Backend adjustments for leaderboard data

### **v1.1.0** | Content & Performance _(November 12, 2020)_

- 🧱 Added 10 new obstacles
- ☄️ Added 11 new asteroid chunks
- ⚡ Performance optimizations
- 🐛 Bug fixes and stability improvements

### **v1.0.0** | Initial Release _(October 15, 2020)_

- 🎉 First public release
- 🚀 Endless runner gameplay in space
- 🏆 Highscore system
- ☄️ Dodge asteroids and space debris
- 🪙 Collect coins during runs
- 🚀 Buy ships and unlock skins
- 🎮 Core controls and UI implemented

#### **v0.0.1** | Concept _(July 21, 2020)_

<small>

- 💡 Initial idea and concept development

</small>
