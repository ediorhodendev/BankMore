CREATE TABLE IF NOT EXISTS transfer_operation (
    id TEXT NOT NULL PRIMARY KEY,
    request_id TEXT NOT NULL UNIQUE,
    source_account_id TEXT NOT NULL,
    destination_account_id TEXT NOT NULL,
    amount REAL NOT NULL,
    created_at_utc TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_transfer_source_account_id
    ON transfer_operation(source_account_id);

CREATE INDEX IF NOT EXISTS ix_transfer_destination_account_id
    ON transfer_operation(destination_account_id);

CREATE TABLE IF NOT EXISTS idempotency (
    id TEXT NOT NULL PRIMARY KEY,
    scope TEXT NOT NULL,
    request_id TEXT NOT NULL,
    created_at_utc TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_transfer_idempotency_scope_request
    ON idempotency(scope, request_id);