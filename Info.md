• That is a sound approach. It is essentially a command buffer plus a unit-of-work transaction:

  Current frame state
      ↓ queries/decisions
  Buffered mutation commands
      ↓ Next()
  One SQLite transaction
      ├── all succeed → commit
      └── any fails   → rollback

  SQLite transactions naturally restore the database to the state before Next() if any mutation fails.

  The main issues to design explicitly are:

  - Reads during the frame won’t see buffered mutations.
  - Command ordering matters.
  - Generated entity IDs aren’t available until commands execute.
  - A rollback only covers database state, not external side effects.
  - Failed commands must not remain in the buffer accidentally.
  - Buffered arguments must not be mutable references that change before execution.

  For example:

  simulation.Entities.SetPosition(position);
  simulation.Entities.SetAgent(agent);

  // Neither change is visible yet.
  simulation.Next();

  // Both changes become visible atomically.

  That’s perfectly good if your semantics are “every system reads frame N and collectively produces frame N+1.” It can actually make simulation behavior more deterministic.

  Use immutable command values rather than delegates:

  internal interface ISimulationCommand
  {
      void Execute(
          SqliteConnection connection,
          SqliteTransaction transaction);
  }

  internal sealed record SetPositionCommand(
      EntityPosition Position) : ISimulationCommand;

  A simplified flush:

  public void Next()
  {
      using var transaction = _connection.BeginTransaction();

      try
      {
          foreach (var command in _commands)
          {
              command.Execute(_connection, transaction);
          }

          transaction.Commit();
      }
      finally
      {
          _commands.Clear();
      }
  }

  Important design decisions:

  1. Visibility

  Systems within one frame observe the previous committed frame. If a later system must see an earlier system’s changes, you need explicit phases:

  Decide();
  FlushMovement();
  ResolveCollisions();
  FlushResults();

  Those phases could use savepoints or separate transactions, but then the whole tick is no longer automatically atomic unless wrapped in a parent transaction.

  2. Entity creation

  This API is awkward:

  long id = mutations.Create(); // Nothing has executed yet

  You could reserve IDs when enqueueing, use application-generated IDs, or return a temporary entity handle resolved during Next().

  3. Failure handling

  Rollback gives you the previous database frame, but decide whether failure means:

  - Discard that tick and report the error
  - Fix/remove the invalid command and retry
  - Treat it as a fatal simulation bug

  Blindly retrying the identical command buffer will generally produce the identical failure.

  4. Non-database effects

  Events, animations, sounds, file writes, and network messages should only be emitted after the transaction commits. Otherwise, the database can roll back while the outside world has already observed the failed frame.

  5. Command lifetime

  Copy immutable records into the buffer. Don’t capture a mutable object:

  _commands.Add(() => SetPosition(position));
  position.X = 20; // Would unexpectedly change the queued operation

  Overall, I like the architecture for this simulator. It provides atomic ticks, deterministic snapshot-style reads, centralized transaction handling, and an obvious place for validation and post-commit events. The biggest question is whether “mutations are invisible until Next()” matches the simulation semantics you want.