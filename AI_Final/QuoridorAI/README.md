# Quoridor AI Game

## Overview
Quoridor is a strategic board game where players aim to reach the opposite side of the board while placing walls to hinder their opponent's movement. This project implements a digital version of Quoridor with AI capabilities using A* and Minimax algorithms.

## Features
- **Player Movement**: Players can move their pieces across the board.
- **Wall Placement**: Players can place walls to block opponents.
- **AI Opponents**: The game includes AI that uses A* for pathfinding and Minimax for decision-making.
- **Highlighting Squares**: Surrounding squares are highlighted when a player selects a piece.
- **Lift Pieces**: Players can lift their pieces for better visibility during gameplay.

## Project Structure
- **Assets**
  - **Materials**: Contains material settings for players, the board, and walls.
  - **Prefabs**: Prefabs for player pieces, squares, and walls.
  - **Scenes**: The main game scene where gameplay occurs.
  - **Scripts**: Contains all the game logic, including AI, core mechanics, player controls, and UI management.
  
- **Packages**: Lists the required packages and dependencies for the project.
  
- **ProjectSettings**: Contains project version information.

## Getting Started
1. Clone the repository or download the project files.
2. Open the project in Unity.
3. Load the `GameScene.unity` to start playing.
4. Use the mouse to select and move pieces or place walls.

## AI Implementation
- **AStarPathfinding.cs**: Implements the A* algorithm for efficient pathfinding.
- **MinimaxAI.cs**: Implements the Minimax algorithm for strategic decision-making.
- **AIController.cs**: Manages AI behavior and interactions with the game.

## Contribution
Feel free to contribute to the project by submitting issues or pull requests. Your feedback and suggestions are welcome!

## License
This project is open-source and available for personal and educational use.