The remaining blockers, in priority order:
  What remains to resolve:

  1. Item support in version 1

  World contains item dictionaries but has no item allocator or creation operation. Choose either:

  - Implement world-owned item creation, IDs, placement, and restoration.
  - Exclude ItemSave and item placements from version 1.

  I recommend excluding items until their lifecycle exists.

 
  3. Restoration construction paths

  Normal commands are unsuitable for restoration because they allocate IDs and generate maps. Add controlled internal paths for:

  - Inserting a preconstructed map at a world coordinate.
  - Inserting agents/items with known IDs.
  - Restoring _nextAgentId.
  - Rebuilding address indexes from placements.
  - Constructing Game with a restored World and active coordinate without emitting events.

  These should be internal/private restoration APIs, not general public mutation.

  4. Movement consistency

  World.MoveAgent() removes the old placement before attempting the destination placement. If the destination is occupied, the agent becomes absent from maps while its address index still points to the old location. This is
  not specifically a serialization problem, but it can produce an inconsistent snapshot.

 
 
  5. Saving/loading is not viable yet.
      - Solitude.Game/Game.cs:8 stores state in private readonly fields, which default System.Text.Json will not serialize.
      - The save name is just "default" instead of an explicit user:// path.
      - Save.cs:26 does not check whether opening the file succeeded before dereferencing it.
      - Starting another game after "default" exists throws instead of presenting or loading it.

 
  7. Runtime verification is unavailable here.

     Godot 4.7.1 .NET is not installed, so scene parsing, node initialization, rendering, and input behavior cannot yet be exercised. The installed .NET SDK can still validate compilation once the code errors are fixed.
