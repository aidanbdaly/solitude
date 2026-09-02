-- sqlite
PRAGMA foreign_keys = ON;

CREATE TABLE
    entity (
        id INTEGER PRIMARY KEY AUTOINCREMENT
    );

CREATE TABLE
    entity_agent (
        entity_id INTEGER PRIMARY KEY,
        name TEXT NOT NULL CHECK (length (trim(name)) > 0),
        race INTEGER NOT NULL CHECK (race = 0),
        drive INTEGER NOT NULL CHECK (drive IN (0, 1, 2)),
        size INTEGER NOT NULL CHECK (size IN (0, 1, 2)),
        tiredness REAL NOT NULL CHECK (tiredness >= 0.0),
        hunger REAL NOT NULL CHECK (hunger >= 0.0),
        FOREIGN KEY (entity_id) REFERENCES entity (id) ON DELETE CASCADE
    );

CREATE TABLE
    entity_item (
        entity_id INTEGER PRIMARY KEY,
        type INTEGER NOT NULL CHECK (type IN (0, 1)),
        quantity INTEGER NOT NULL CHECK (quantity >= 0),
        FOREIGN KEY (entity_id) REFERENCES entity (id) ON DELETE CASCADE
    );

CREATE TABLE
    entity_feature (
        entity_id INTEGER PRIMARY KEY,
        type INTEGER NOT NULL CHECK (type IN (0, 1, 2)),
        FOREIGN KEY (entity_id) REFERENCES entity (id) ON DELETE CASCADE
    );

CREATE TABLE
    entity_inventory (
        entity_id INTEGER PRIMARY KEY,
        FOREIGN KEY (entity_id) REFERENCES entity (id) ON DELETE CASCADE
    );

CREATE TABLE
    entity_inventory_item (
        entity_id INTEGER NOT NULL,
        item_type INTEGER NOT NULL CHECK (item_type IN (0, 1)),
        quantity INTEGER NOT NULL CHECK (quantity >= 0),
        PRIMARY KEY (entity_id, item_type),
        FOREIGN KEY (entity_id) REFERENCES entity_inventory (entity_id) ON DELETE CASCADE
    );

CREATE TABLE
    map (
        x INTEGER NOT NULL CHECK (x >= 0),
        y INTEGER NOT NULL CHECK (y >= 0),
        elapsed INTEGER NOT NULL CHECK (elapsed >= 0),
        width INTEGER NOT NULL CHECK (width > 0),
        height INTEGER NOT NULL CHECK (height > 0),
        PRIMARY KEY (x, y)
    );

CREATE TABLE
    map_tile (
        map_x INTEGER NOT NULL,
        map_y INTEGER NOT NULL,
        x INTEGER NOT NULL CHECK (x >= 0),
        y INTEGER NOT NULL CHECK (y >= 0),
        type INTEGER NOT NULL CHECK (type IN (0, 1, 2)),
        PRIMARY KEY (map_x, map_y, x, y),
        FOREIGN KEY (map_x, map_y) REFERENCES map (x, y) ON DELETE CASCADE
    );

CREATE TABLE
    entity_position (
        entity_id INTEGER PRIMARY KEY,
        map_x INTEGER NOT NULL,
        map_y INTEGER NOT NULL,
        x INTEGER NOT NULL CHECK (x >= 0),
        y INTEGER NOT NULL CHECK (y >= 0),
        UNIQUE (entity_id, map_x, map_y, x, y),
        FOREIGN KEY (entity_id) REFERENCES entity (id) ON DELETE CASCADE,
        FOREIGN KEY (map_x, map_y, x, y) REFERENCES map_tile (map_x, map_y, x, y) ON DELETE RESTRICT
    );

CREATE INDEX entity_position_by_coordinate
ON entity_position (map_x, map_y, x, y);

CREATE TABLE
    entity_collider (
        entity_id INTEGER PRIMARY KEY,
        map_x INTEGER NOT NULL,
        map_y INTEGER NOT NULL,
        x INTEGER NOT NULL CHECK (x >= 0),
        y INTEGER NOT NULL CHECK (y >= 0),
        UNIQUE (map_x, map_y, x, y),
        FOREIGN KEY (entity_id, map_x, map_y, x, y) REFERENCES entity_position (entity_id, map_x, map_y, x, y) ON UPDATE CASCADE ON DELETE CASCADE
    );

CREATE TABLE
    game (
        singleton INTEGER PRIMARY KEY CHECK (singleton = 1),
        data_version INTEGER NOT NULL,
        active_map_x INTEGER,
        active_map_y INTEGER,
        FOREIGN KEY (active_map_x, active_map_y) REFERENCES map (x, y),
        CHECK (
            (
                active_map_x IS NULL
                AND active_map_y IS NULL
            )
            OR (
                active_map_x IS NOT NULL
                AND active_map_y IS NOT NULL
            )
        )
    );
