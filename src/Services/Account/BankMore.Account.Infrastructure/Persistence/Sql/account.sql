CREATE TABLE IF NOT EXISTS current_account (
    id TEXT NOT NULL PRIMARY KEY,
    account_number TEXT NOT NULL UNIQUE,
    name TEXT NOT NULL,
    cpf TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    is_active INTEGER NOT NULL,
    created_at_utc TEXT NOT NULL,
    deactivated_at_utc TEXT NULL
);

CREATE TABLE IF NOT EXISTS movement (
    id TEXT NOT NULL PRIMARY KEY,
    current_account_id TEXT NOT NULL,
    request_id TEXT NOT NULL,
    type INTEGER NOT NULL,
    amount REAL NOT NULL,
    created_at_utc TEXT NOT NULL,
    CONSTRAINT fk_movement_current_account
        FOREIGN KEY (current_account_id) REFERENCES current_account(id)
);

CREATE INDEX IF NOT EXISTS ix_movement_current_account_id
    ON movement(current_account_id);

CREATE TABLE IF NOT EXISTS idempotency (
    id TEXT NOT NULL PRIMARY KEY,
    scope TEXT NOT NULL,
    request_id TEXT NOT NULL,
    created_at_utc TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_idempotency_scope_request
    ON idempotency(scope, request_id);