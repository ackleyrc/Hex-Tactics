# MISSION DEBRIEFING

Unattended mechs across the sector have been hacked and gone rogue. The now autonomous mechs are wreaking havoc, and it's your mission to scuttle the drones and keep the malware from spreading. The injected override routines can be outsmarted if you're clever, but the mechs will do real damage, so be careful out there!

![image](https://github.com/user-attachments/assets/970f47ca-73dd-4902-b43a-f265f5e225fb)


**Hex Tactics** is a demo developed over about 5 weeks. The project is built in Unity. While the code was written by Robert Ackley, all audio and visual assets were obtained from online marketplaces and forums.

Development has advanced in phases, each with a different area of focus:

1. Rapid Prototype [14 Days]
2. AI Extension [6 Days]
3. Polish Phase [6 Days]
4. Procgen Iteration [10 Days]

The procedural generation phase was developed as a submission for [ProcJam 2020](https://itch.io/jam/procjam/rate/829370).

# Development Diary

## [Rapid Prototype](https://ackleyrc.itch.io/hex-tactics/devlog/208108/introducing-hex-tactics) [14 Days]

* Generate hex tiles from map definition scriptable object
* Implement basic mech controls for selection, movement, attack
* Add health bar UI, functions to inflict damage, death animation
* Add cycling through unit turns plus unit turn UI
* Add action phase handling plus action panel UI
* Implement initial AI for enemy turn management
* Extend AI to reposition when no target is available
* Add UI to render lines representing LOS to enemies
* Implement weighted pathfinding based on terrain types
* Implement LOS blocking via certain terrain types
* Add Hex Tile UI panel with info on terrain effects
* Update mech movement to run or walk based on terrain type
* Add initial Title Menu and Pause Menu screens
* Expand map, add Camera controls for pan & zoom
* Implement win condition handling and End Game UI
* Implement Restart option, add to Pause and End Game menus
* Add mech movement limit and UI to convey mech range

## [Extended AI](https://ackleyrc.itch.io/hex-tactics/devlog/220323/hex-tactics-ai-extension) [6 Days]

* Introduce terrain based defense bonuses
* Extend hex grid code to produce distance map for multiple destinations
* Calculate defensive bonus utility factor for nearby hexes
* Calculate exposure avoidance & target opportunity utility factors
* Choose best location to move based on combined weighted utility factors
* Provide UI to select from AI difficulty options at start of each game
* Evaluate game state and render dialogue to convey AI decision making

## Polish Phase [6 Days]

* Add Title Screen and Gameplay music with track cycling
* Add musical stingers for win and loss conditions
* Add ambient looping wind sound effect
* Add SFX for laser attacks, mech locomotion, and unit death
* Add SFX for hex hover over, confirm action, new unit turn, and AI dialogue
* Add SFX for hovering over and clicking on UI buttons
* Increase mech movement and animation speeds by 30%
* Add Credits screen accessible from the Title Screen
* Display shield icon indicating terrain defense bonus in hex highlight
* Display shield icon indicating terrain defense bonus next to mech health bar


## Procgen Iteration [10 Days]

* Introduce basic procedural generation of maps and starting positions
* Add option to reset current scenario to original starting positions
* Add option to reset current scenario to same map with new positions
* Add option to start a new scenario on a new map
* Add Customize menu with sliders controlling procgen map parameters
* Refactor map generation to accommodate multiple biomes
* Refactor Customize menu to accommodate multiple biomes
* Add Desert & Shrubland biomes (in addition to original Tropical biome)
* Refine map gen parameters to achieve more distinct gameplay per biome
* Add mech pilot speech lines for end turn, move, attack, and teammate down
* Randomize mech pilot names and avatars for each new paythrough
* Introduce randomized character traits with Character Profile UI panel
* Extend speech lines and filter on speaking units' characters traits
