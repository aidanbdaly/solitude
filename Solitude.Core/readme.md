 Use Dapper over Microsoft.Data.Sqlite.

  Strictly speaking, Dapper is a micro-ORM rather than a full ORM. That is a good fit here because your SQL schema is the domain model: you want explicit joins, transactions, indexes, and constraints without a framework trying to reconstruct an object graph and track its mutations.

  ## Recommended stack

  Microsoft.Data.Sqlite
      SQLite connection, commands and transactions

  Dapper
      Parameters and mapping query results to records

  Numbered SQL migrations
      Schema creation and upgrades

  GameContext
      Domain operations and transaction boundaries

  Packages:

  <PackageReference Include="Microsoft.Data.Sqlite" Version="..." />
  <PackageReference Include="Dapper" Version="..." />

  Select stable versions compatible with the project’s net8.0 target.

  ## Why Dapper fits

  Your operations will naturally be SQL-centric:

  SELECT position.*, agent.*
  FROM entity_position AS position
  JOIN entity_agent AS agent
      ON agent.entity_id = position.entity_id
  WHERE position.map_x = $mapX
    AND position.map_y = $mapY;

  Dapper maps that result without hiding the query or introducing change tracking:

  public sealed record PositionedAgent(
      long EntityId,
      int MapX,
      int MapY,
      int X,
      int Y,
      string Name,
      AgentType Race,
      AgentDrive Drive,
      AgentStature Size,
      double Tiredness,
      double Hunger);

  var agents = connection.Query<PositionedAgent>(
      sql,
      new { mapX, mapY },
      transaction);

  Dapper supports SQLite through the standard ADO.NET provider and supports explicit transaction arguments. Dapper repository

  ## Why I would not choose EF Core here

  EF Core could map these tables, but its main strengths work against the current design:

  - Its change tracker expects object-oriented entity graphs.
  - The relational ECS deliberately has optional, independently queried components.
  - Systems will use explicit joins based on component combinations.
  - Movement and collision need carefully controlled SQL transactions.
  - The hand-authored constraints are important and should remain visible.
  - SQLite migrations often require table rebuilds, especially for constraint changes. EF Core SQLite limitations

  EF Core would be reasonable if the goal were LINQ-driven CRUD over conventional aggregate objects. It is less compelling for a schema-first ECS.

  ## Connection ownership

  Let one GameContext own one open SqliteConnection for its active save:

  public sealed class GameContext : IDisposable
  {
      private readonly SqliteConnection _connection;

      public GameContext(string path)
      {
          _connection = new($"Data Source={path}");
          _connection.Open();

          _connection.Execute("PRAGMA foreign_keys = ON;");
      }

      public void Dispose() => _connection.Dispose();
  }

  Do not let component records or Godot nodes access the connection. They should call GameContext.

  A single long-lived connection is appropriate because:

  - each game context targets one save database;
  - temporary in-memory SQLite databases disappear when their connection closes;
  - foreign-key configuration is per connection;
  - SQLite permits only one active writer anyway.

  SQLite transactions provide atomicity and can also improve batches of related writes. Microsoft.Data.Sqlite transactions

  ## Transactional domain operations

  Dapper does not automatically associate commands with the current transaction. Pass it explicitly:

  public long CreateAgent(CreateAgentRequest request)
  {
      using var transaction = _connection.BeginTransaction();

      var entityId = _connection.QuerySingle<long>(
          """
          INSERT INTO entity DEFAULT VALUES
          RETURNING id;
          """,
          transaction: transaction);

      _connection.Execute(
          """
          INSERT INTO entity_agent (
              entity_id,
              name,
              race,
              drive,
              size,
              tiredness,
              hunger
          )
          VALUES (
              @EntityId,
              @Name,
              @Race,
              @Drive,
              @Size,
              @Tiredness,
              @Hunger
          );
          """,
          new
          {
              EntityId = entityId,
              request.Name,
              Race = (int)request.Race,
              Drive = (int)request.Drive,
              Size = (int)request.Size,
              request.Tiredness,
              request.Hunger
          },
          transaction);

      _connection.Execute(
          "INSERT INTO entity_inventory (entity_id) VALUES (@entityId);",
          new { entityId },
          transaction);

      transaction.Commit();
      return entityId;
  }

  If any component insertion fails, disposing the uncommitted transaction rolls everything back.

  ## Mapping convention

  The schema uses snake_case, while C# uses PascalCase. Prefer explicit aliases in projection queries:

  SELECT
      entity_id  AS EntityId,
      map_x      AS MapX,
      map_y      AS MapY,
      x          AS X,
      y          AS Y
  FROM entity_position;

  That keeps mappings obvious. Dapper can globally enable underscore matching, but explicit aliases avoid hidden global behaviour and make projection contracts clearer.

  For inserts and internal commands, SQL parameter names can remain PascalCase independently of table-column names.

  ## Do not build generic repositories

  Avoid an abstraction such as:

  IRepository<TEntity>

  ECS operations are not generic CRUD. Expose domain queries and commands:

  CreateAgent(...)
  MoveEntity(...)
  AddCollider(...)
  RemoveCollider(...)
  GetPositionedAgents(...)
  GetEntitiesAt(...)
  TransferItemToInventory(...)

  Internally, split large implementations into focused query/system classes if GameContext grows:

  GameContext
  ├── EntityCommands
  ├── PositionQueries
  ├── MovementSystem
  ├── InventorySystem
  └── MapQueries

  They can all share the same connection and accept explicit transactions.

  ## Migrations

  Keep migrations as handwritten SQL:

  Sql/
  ├── 001_initial.sql
  ├── 002_add_work.sql
  └── 003_add_requirements.sql

  Use PRAGMA user_version to record the applied version:

  PRAGMA user_version = 1;

  Run each pending migration inside a transaction. Do not introduce EF solely for its migrations; it would create a second schema definition alongside map.sql.

  ## Testing

  Use one open in-memory connection per test:

  using var connection =
      new SqliteConnection("Data Source=:memory:");

  connection.Open();
  connection.Execute("PRAGMA foreign_keys = ON;");
  connection.Execute(schemaSql);

  Test GameContext against the real SQLite engine rather than mocking Dapper or the connection. The database constraints are part of the domain behaviour, so mocks would omit much of what needs testing.

  My recommendation is therefore:

  Dapper + Microsoft.Data.Sqlite + handwritten SQL migrations, with all access encapsulated by GameContext.