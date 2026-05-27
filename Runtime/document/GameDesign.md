# Game Design Document: 2048

## 1. Overview

**Title:** 2048
**Genre:** Casual Puzzle
**Platform:** Web (HTML/CSS/JavaScript), Mobile (iOS, Android)
**Target Audience:** All ages (casual gamers, puzzle enthusiasts)
**Elevator Pitch:** Slide numbered tiles on a 4x4 grid to combine them and create the elusive 2048 tile—a deceptively simple number puzzle that is easy to learn but hard to master.

---

## 2. Gameplay & Mechanics

### 2.1 Core Game Loop

The player is presented with a 4x4 grid where numbered tiles appear. By swiping (or pressing arrow keys), they slide all tiles in one of four directions (up, down, left, right). When two tiles with the same number collide during the slide, they merge into a single tile with their sum. After each move, a new tile (usually 2, occasionally 4) spawns in a random empty cell. The game ends when no moves remain (i.e., the board is full and no adjacent tiles share the same number).

### 2.2 Rules

- **Grid:** 4x4 grid (16 cells).
- **Tile Values:** Powers of 2 (2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, ...).
- **Movement:** Swipe up/down/left/right. All tiles slide as far as possible in the chosen direction.
- **Merging:** When two equal-numbered tiles collide in the direction of movement, they merge into one (sum of values). Each tile can only merge once per move.
- **Spawning:** After each move, a new tile (90% = 2, 10% = 4) appears in a random empty cell.
- **Win Condition:** Create a tile with value 2048. (Players can continue beyond.)
- **Score:** Each merge adds its new value to the score. The total score begins at 0.

### 2.3 States

| State | Description |
|-------|-------------|
| IDLE | Waiting for player input. |
| ANIMATING | Tiles sliding/merging (if animated). |
| WIN | A 2048 tile is created. Player can continue or reset. |
| GAME_OVER | No empty cells and no adjacent equal tiles. Cannot move. |

### 2.4 Edge Cases & Constraints

- Move order: For merges, tiles closer to the wall merge first. If multiple equal tiles line up (e.g., 4 4 4 4), the resulting two tiles become 8 8 (merged from the side facing the movement direction).
- A single tile cannot merge twice in one move.
- If no tiles move (i.e., a direction with no space and no merges possible), no new tile spawns (the move is skipped).
- Grid is maintained as row-major 0-index: (row, col) with (0,0) top-left.

---

## 3. Visual & UI Design

### 3.1 Layout

- **Header:** Title, Score display, "New Game" button
- **Game Board:** 4x4 grid prominently centered, large tiles with bold numbers
- **Footer:** Instructions ("How to play: Use arrow keys or swipe to move tiles")

### 3.2 Visual Style

- Clean, minimalistic flat design
- Tile colors: Different background colors for each power-of-two value (2=#EEE4DA, 4=#EDE0C8, 8=#F2B179, ... , 2048=#EDC22E)
- Typography: Bold sans-serif for numbers; font size inversely proportional to the number of digits
- Subtle animation for tile movement (slide transition ~150ms) and merge (pop/scale effect)
- Color palette: Board background #BAAA9F, Cell background #CCC0B3, Text dark (#776E65) or white (#F9F6F2) for larger tiles

---

## 4. User Interaction

### 4.1 Controls

| Input | Action |
|-------|--------|
| Arrow Keys / WASD | Slide in direction |
| Swipe gesture (mobile) | Slide in direction |
| Touch (drag)  | Slide in direction |
| Button "New Game" | Reset board |

### 4.2 Feedback

- Slide animation (tiles slide smoothly)
- Merge animation (scale bounce)
- Score counter updates after merge
- Game-over overlay message with final score
- Win popup with option to "Continue" or "Restart"

---

## 5. Technical Notes

- The game must maintain game state (board, score), handle user input, and strictly follow merge logic.
- For web version, recommend plain HTML/CSS/JavaScript.
- Persistence via `localStorage` recommended to save game state across refreshes.
- Use CSS Grid or Flexbox for layout; CSS transitions/transforms for animations.

---

## 6. Development Roadmap (MVP)

1. Set up project structure (HTML/CSS/JS)
2. Implement board initialization (empty 4x4, spawn 2 tiles)
3. Implement swipe/movement logic and merge rules
4. Render board in DOM
5. Add animations (move and merge)
6. Add scoring
7. Game-over and win detection
8. Save game state to localStorage
9. Polish: Styling, responsiveness, mobile touch support
10. Playtest and adjust probabilities

---

This document serves as the definitive reference for the 2048 game design and covers all intended behavior, visual style, and technical requirements.