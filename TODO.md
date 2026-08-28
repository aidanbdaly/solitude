The remaining blockers, in priority order:

 
 
  5. Saving/loading is not viable yet.
      - Solitude.Game/Game.cs:8 stores state in private readonly fields, which default System.Text.Json will not serialize.
      - The save name is just "default" instead of an explicit user:// path.
      - Save.cs:26 does not check whether opening the file succeeded before dereferencing it.
      - Starting another game after "default" exists throws instead of presenting or loading it.

 
  7. Runtime verification is unavailable here.

     Godot 4.7.1 .NET is not installed, so scene parsing, node initialization, rendering, and input behavior cannot yet be exercised. The installed .NET SDK can still validate compilation once the code errors are fixed.
