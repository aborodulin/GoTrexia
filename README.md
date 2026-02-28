# GoTrexia

**GoTrexia** is a customizable GPS-based treasure quest engine built with .NET MAUI.

Create your own location-based adventure games, define stages, hints, scoring rules, and share them as portable game packs.

Turn the real world into a playground.

---

## 🌍 What is GoTrexia?

GoTrexia is a mobile application for Android that allows players to load custom adventure games and complete GPS-based stages in the real world.

Each game consists of multiple stages.  
Each stage has:

- A description
- A target GPS location
- Optional hints
- A scoring system

Game creators can design their own quests using a simple JSON-based format.

---

## ✨ Features

- 📂 Load custom game packs from device storage
- 🗺 Interactive map with real-time GPS tracking
- 🎯 Location-based stage completion detection
- 💡 Smart hint system with dynamic search area
- 🏆 Scoring system (full score, half score, or skip)
- 🎨 Custom backgrounds per stage
- 🧩 Modular game engine architecture
- 🔌 Offline-capable gameplay

---

## 🎮 Gameplay Flow

1. Load a game file
2. View stage list
3. Read stage description
4. Navigate using the map
5. Use hints (optional)
6. Reach the target location
7. Complete all stages
8. See final score

---

## 🧠 Hint Logic

- Hint button becomes available after a configurable delay
- Hint shows a search radius (not the exact location)
- If the player takes too long, the search area shrinks
- Using a hint reduces stage score by 50%
- Skipping a stage results in 0 points

---

## 📦 Game Pack Format

GoTrexia uses a portable game pack format:
```
.gamepack (zip archive)
├── game.json
├── start.jpg
├── end.jpg
├── stage1.jpg
├── stage2.jpg
```


### Example `game.json` structure:

```json
{
  "gameInfo": {
    "title": "City Mystery Quest",
    "author": "Game Creator",
    "startBackground": "start.jpg"
  },
  "endScreen": {
    "description": "Congratulations! You completed the adventure!",
    "backgroundImage": "end.jpg"
  },
  "stages": [
    {
      "id": 1,
      "title": "The Old Bridge",
      "description": "Find the old stone bridge near the river.",
      "backgroundImage": "stage1.jpg",
      "finalLocation": {
        "latitude": 49.283,
        "longitude": -123.120
      },
      "hintDelaySeconds": 60,
      "hintRadiusMeters": 200,
      "hintPrecisionDelaySeconds": 180,
      "points": 100
    }
  ]
}
```

## 🏗 Architecture

GoTrexia is built with clean and modular architecture:

/Models
/GameEngine
/Services
/ViewModels
/Views

Core components:
GameFileLoader
GameStateManager
LocationService
HintService
ScoreService

## 🛠 Technology Stack

.NET MAUI
C#
Android
Google Maps integration
JSON-based configuration

## 🚀 Roadmap
```
[] MVP: Load game + map navigation
[] Dynamic hint visualization
[] Score engine refinement
[] Game pack validator
[] UI polish & animations
[] Publish on Google Play
```
## 📌 Vision

GoTrexia aims to become a flexible GPS quest engine that allows anyone to create real-world adventure games without writing code.
