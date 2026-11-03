-- First enable pg_cron, pg_net and Vault in the Dashboard, and create these
-- Vault entries in the Dashboard (do not paste credentials into this file):
-- journey_billing_project_url = https://YOUR_PROJECT_REF.supabase.co
-- journey_billing_sync_secret = exactly the BILLING_SYNC_SECRET Edge Function secret
--
-- Run once. To replace a previous job, unschedule it first using its job ID.
select cron.schedule(
    'journey-google-play-reconcile',
    '*/15 * * * *',
    $job$
    select net.http_post(
        url := (select decrypted_secret from vault.decrypted_secrets
                where name = 'journey_billing_project_url') || '/functions/v1/verify-google-play-purchase',
        headers := jsonb_build_object(
            'Content-Type', 'application/json',
            'x-billing-sync-secret', (select decrypted_secret from vault.decrypted_secrets
                                     where name = 'journey_billing_sync_secret')
        ),
        body := '{"action":"reconcile"}'::jsonb,
        timeout_milliseconds := 120000
    );
    $job$
);
