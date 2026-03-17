CREATE TABLE IF NOT EXISTS fee_operation (
    id TEXT NOT NULL PRIMARY KEY,
    transfer_id TEXT NOT NULL UNIQUE,
    account_id TEXT NOT NULL,
    amount REAL NOT NULL,
    created_at_utc TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_fee_account_id
    ON fee_operation(account_id);