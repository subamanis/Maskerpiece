# Maskerpiece - Global Game Jam 2026

## Concept
Dress-up game (My Style Rocks style). Theme: MASKS.

A girl stands in a dressing room. You are a fashion designer creating her outfit by composing a collage of masks. Masks = building blocks for clothes (skirts, tops, etc).

## Gameplay Flow

### 1. Start Screen
- Background: runway/catwalk
- Girl facing camera with default basic clothes
- "Start Game" button

### 2. Walking Away Sequence (~3 sec)
- Girl turns around (back view)
- Walks toward end of runway
- Alternate between left/right foot forward sprites for walk cycle
- Scale decreases as she walks into distance

### 3. Curtain Transition
- Curtains close effect
- Curtains open to reveal dressing room

### 4. Dressing Room
- Girl in base state (to be dressed)
- UI drawer with mask templates
- Touch controls to place/move/rotate/scale masks
- Masks become children of girl
- "Finalize" button

### 5. Runway Reveal
- Curtain close/open transition
- Girl appears on runway with new mask outfit
- (Loop back or end?)

## Girl Model Variations
1. **Front + basic clothes** - start screen, facing camera
2. **Back + basic clothes (left foot)** - walking away frame 1
3. **Back + basic clothes (right foot)** - walking away frame 2
4. **Dressing room base** - same model used for final runway reveal

## Core Mechanics

### Mask Spawning
- UI drawer panel with ~10 mask templates
- Tap mask in drawer → instantiate in scene
- Masks become child objects of the girl (for cutscene movement)

### Touch Controls (Android)
- Move: drag
- Rotate: two-finger twist
- Scale: pinch zoom
- Selection: tap to select/deselect

## Tech Stack
- Unity (2D)
- Android touch input
- Target: Mobile

## Architecture Notes
- Masks are child GameObjects of the girl character
- SelectionManager handles tap/drag/multitouch
- TouchMovableRotatable component for mask manipulation
