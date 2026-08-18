# Solitude

Solitude is the Godot-native continuation of Domain. It retains the original
pixel art, audio, colony simulation, and gameplay concepts without carrying
Unreal's source, content, controller, or component conventions into Godot.

## Play

Open `project.godot` in Godot 4.7.1 .NET and run the project. `Main.tscn` is the
application entry point; it opens the menu and swaps in the self-contained game
scene when play begins.

## Controls

- WASD or arrow keys: pan the camera
- Mouse wheel: zoom
- Middle mouse drag: pan
- Left click: use the active tool
- Right click: move the selected colonist in Command mode
- 1-3: Command, Construct, Damage
- Space: pause
- Plus/minus: simulation speed

Destroyed flora drops wood, destroyed rock drops stone, and wall construction sites require
16 wood. Colonists claim orders, navigate to their targets, collect construction
materials, and complete construction autonomously.

## Structure

- `Main.tscn` and `Main.cs`: visible application entry point and screen routing
- `Nodes/Game`: playable game scene and its Godot nodes
- `Nodes/Menu`: menu scene and its Godot node
- `Domain/Game`: state boundary, simulation orchestration, and simulation events
- `Domain/Game/World`: world state and domain mutations, split into focused partial implementations
- `Domain/Game/Map`: grid, terrain, and spatial-math primitives
- `Domain/Game/Agents`: agent state, GOAP planning, linear action plans, navigation, and simulation processes
- `Domain/Game/Construction`: construction-site state and progress
- `Domain/Game/Orders`: player and simulation orders, assignment state, and assignment lifecycle
- `Domain/Game/Items`: inventory, item stacks, and reservations
- `Domain/Game/Objects`: placed flora, rock, and walls
- `Domain/Game/Commands`: player-facing gameplay commands
- `Domain/Game/Generation`: world generation and exact Unreal-compatible Perlin noise
- `Domain/Game/Clock.cs`: simulation time
- `Assets`: imported textures and audio, grouped by owning feature

New-game generation produces a complete `State` before the game scene is
initialized. A future save loader can supply the same boundary object without
changing `GameNode` or `Simulation`. Stored state contains no runtime services
or presentation events; simulation events are delivered synchronously to the
game node, while Godot signals remain local to scene-tree communication.

Input bindings are defined in Godot's Input Map in `project.godot`. Scene and
resource dependencies are assigned declaratively in `.tscn` and `.tres` files.
