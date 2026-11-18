-- BlocoNaRua auth migration 001: auth columns on members
-- Source: .tmp/AUTH_MIGRATION_PLAN.md §2
-- Strategy: idempotent, schema-only (no data migration — existing Supabase users are dropped;
-- users re-register via Google).
--
-- Apply via psql or the Dokploy postgres SQL console:
--   psql "Host=<host>;Port=5432;Database=postgres;Username=<user>;Password=<pass>" -f sql/migrations/001_auth_columns.sql

ALTER TABLE members
  ADD COLUMN IF NOT EXISTS provider text,
  ADD COLUMN IF NOT EXISTS provider_id text,
  ADD COLUMN IF NOT EXISTS password_hash text,
  ADD COLUMN IF NOT EXISTS refresh_token text,
  ADD COLUMN IF NOT EXISTS refresh_token_expires_at timestamptz,
  ADD COLUMN email_confirmed boolean NOT NULL DEFAULT false;

CREATE UNIQUE INDEX IF NOT EXISTS ux_members_provider ON members (provider, provider_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_members_refresh_token ON members (refresh_token) WHERE refresh_token IS NOT NULL;
