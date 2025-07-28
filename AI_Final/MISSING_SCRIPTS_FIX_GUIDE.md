# Missing Script Components Fix Guide

## Problem
You're seeing errors like:
- "The referenced script on this Behaviour (Game Object 'Player1') is missing!"
- "The referenced script on this Behaviour (Game Object 'Player2') is missing!"

## Quick Solution

I've created several scripts to fix this automatically. Here's how to use them:

### Method 1: Automatic Fix (Recommended)
1. Add the `StartupFixer` script to your GameManager GameObject
2. The script will automatically fix missing components when the game starts
3. Check the Console for fix confirmation messages

### Method 2: Manual Fix in Unity Editor
1. In Unity, go to the menu: **Quoridor → Fix Missing Scripts**
2. This will add all required components to Player1 and Player2
3. Save the scene after running the fix

### Method 3: Runtime Emergency Fix
1. Add the `EmergencyFixer` script to any GameObject in the scene
2. It will automatically run and fix components on Awake()
3. Remove the script after the fix is applied

### Method 4: Manual Component Addition
If the scripts don't work, manually add these components:

**Player1 GameObject needs:**
- ChessPlayer (with playerID = 1)
- WallPlacer (with playerID = 1)

**Player2 GameObject needs:**
- ChessPlayer (with playerID = 2)
- QuoridorAI (with playerID = 2)
- WallPlacer (with playerID = 2)

## Verification
After applying any fix:
1. Check that Player1 and Player2 GameObjects have the correct components
2. Verify playerID values are set correctly (1 for Player1, 2 for Player2)
3. Run the game - the missing script errors should be gone

## Current Game Status
Based on your console output, the Q-Learning system is working correctly:
- ✅ Q-table loaded with 14,657 trained states
- ✅ AI using exploitation mode (ε=0.1)
- ✅ Self-play training running successfully
- ❌ Missing script references on Player GameObjects (this fix will resolve it)

The game logic is functioning - you just need to fix the missing component references!
