# Google Play billing deployment

The frontend and deployable backend files are implemented locally. No live Supabase,
Google Play, Stripe, or website configuration was changed.

Normal browser purchases use the existing Stripe checkout. Google Play purchases use
Digital Goods + Payment Request, then Supabase verifies and acknowledges them with
Google. The app's journey access service combines existing Stripe `journey_access`
records with the new read-only `google_play_journey_access` view. Google purchases are
stored separately so this deployment does not guess or alter existing Stripe column
constraints, access types, or webhook behavior.

## 1. Gather the values you will use

Record these values before deployment:

| Value | Where to get it |
| --- | --- |
| Application origin | The exact HTTPS origin of your website, without a trailing slash or path, e.g. `https://journey.example.com` |
| Android package ID | The package ID entered in PWABuilder and registered in Play Console |
| Journey slug | The actual slug in your site's `/journeys/SLUG` address |
| Google product ID | The one-time product you create in Play Console, e.g. `relationship_journey_unlock` |
| Google service-account JSON | The private server credential created in step 2 |
| Reconciliation secret | A randomly generated secret of at least 32 characters, saved in a password manager |

`relationship` in the examples below is an example, not a verified production slug.
The Google product ID is separate from the journey slug. No product or price belongs
in `wwwroot/appsettings.json`.

## 2. Configure Google before Supabase

1. In Play Console, register the application using your final Android package ID.
2. Set up the merchant/payments profile required to sell products.
3. Create an active **one-time, non-consumable** in-app product for the journey.
   Set the US base price to **USD 9.99** and review regional pricing. Use a normal
   permanent purchase, not a rental or multi-quantity product. The application never
   consumes this purchase; consuming it would permit repeat purchases.
4. In Google Cloud, enable the **Google Play Android Developer API**. Create a
   dedicated service account and a JSON key for it.
5. In Play Console's users/permissions settings, grant the service account access
   to this app and the permissions required to view financial/order information and
   manage orders (including purchase acknowledgement). Follow Google's API setup
   documentation if the interface labels differ.
6. Keep the JSON key in secure storage. You will paste its contents into a Supabase
   Edge Function secret, never into the Blazor app or this repository.
7. Set up an internal testing release and license testers. Product availability
   and permissions can take time to propagate.

References: [Google API access setup](https://developers.google.com/android-publisher/getting_started),
[TWA billing prerequisites](https://developer.chrome.com/docs/android/trusted-web-activity/receive-payments-play-billing).

## 3. Run the database migration in Supabase online

1. Open your Supabase project (the app currently points at project
   `yzpgbcwcumrhybyezeln`; confirm this is the project you intend to deploy).
2. Open **SQL Editor → New query**.
3. Copy the full contents of
   `supabase/migrations/202611020001_google_play_billing.sql` into the editor.
4. Run it **once**. The transaction creates:
   - `google_play_products`: the server-owned product-to-journey mapping;
   - `google_play_purchases`: private purchase records with unique tokens and
     immutable ownership;
   - `google_play_journey_access`: a view exposing only the signed-in user's
     completed, acknowledged entitlements;
   - `record_google_play_purchase`: an atomic function callable only by the service role.
5. Confirm all four objects exist. Do not add client insert/update policies on the
   Google tables. The view intentionally executes as its owner with an explicit
   `auth.uid()` filter; it exposes no purchase tokens. Supabase may flag owner-run
   views in its advisor. Do not remove that filter or change view execution mode
   without redesigning its permissions.
6. In a new query, insert your actual product mapping:

   ```sql
   insert into public.google_play_products (product_id, journey_slug)
   values ('YOUR_GOOGLE_PRODUCT_ID', 'YOUR_ACTUAL_JOURNEY_SLUG');
   ```

   Add one row for each separately sold journey. Do not change an existing mapping
   after purchases have been recorded. The current implementation supports one
   permanent Play product per journey.

Existing Stripe records, functions and policies are left intact. Google purchases
do **not** create rows in the existing `journey_access` table.

## 4. Add secrets in Supabase online

Open **Edge Functions → Secrets** and save these values:

| Secret name | Value |
| --- | --- |
| `GOOGLE_PLAY_PACKAGE_NAME` | Exact Android package ID |
| `GOOGLE_PLAY_SERVICE_ACCOUNT_JSON` | Entire service-account JSON, including the private key; paste as one secret value |
| `BILLING_ALLOWED_ORIGINS` | Exact production HTTPS origin; comma-separate additional approved origins if needed |
| `BILLING_SYNC_SECRET` | The random secret of at least 32 characters from step 1 |
| `GOOGLE_PLAY_ALLOW_TEST_PURCHASES` | `true` while testing; change to `false` before accepting production-only purchases |

Supabase supplies `SUPABASE_URL`, `SUPABASE_ANON_KEY`, and
`SUPABASE_SERVICE_ROLE_KEY` in the hosted runtime. These are read by the function;
do not try to create user secrets with the reserved `SUPABASE_` prefix. If your
project has disabled legacy API keys, adapt the server's key lookup to your injected
publishable/secret key dictionaries before deployment.

After switching test purchases off, existing test purchase rows should be canceled
in the private Google ledger using an administrator query if you no longer want
their accounts to retain access. Automatic reconciliation deliberately does not
change entitlements when verification fails.

Reference: [Supabase production secrets](https://supabase.com/docs/guides/functions/secrets).

## 5. Deploy the Edge Function in Supabase online

1. Open **Edge Functions → Deploy a new function → Via Editor** (the exact button
   labels may differ).
2. Name the function **`verify-google-play-purchase`** exactly.
3. Replace `index.ts` with
   `supabase/functions/verify-google-play-purchase/index.ts` from this repository.
4. Add a second file in the same function directory named **`google-play.ts`** and
   paste `supabase/functions/verify-google-play-purchase/google-play.ts` into it.
   Keep the relative import in `index.ts` unchanged.
5. Deploy the function.
6. In this function's settings, turn **Verify JWT / Enforce JWT verification**
   **off**. This is required for the scheduled reconciliation action and is also
   recorded in `supabase/config.toml` for CLI deployments. User actions still
   validate the supplied JWT against Supabase Auth inside the handler; the cron
   action requires the separate private secret. Do not disable these code checks.
7. Check the function logs for startup/deployment errors. A request without a valid
   user session must return 401; it must never grant access.

If the online editor cannot add the second file, deploy the same files with the
Supabase CLI from the repository root:

```powershell
supabase login
supabase functions deploy verify-google-play-purchase --project-ref YOUR_PROJECT_REF --no-verify-jwt
```

The dashboard's generic Test panel does not automatically represent a signed-in app
user. For a catalog test use a real test user's access JWT on `Authorization: Bearer
...` and this JSON body (do not paste tokens into chats or commit them):

```json
{ "action": "catalog", "journeySlug": "YOUR_ACTUAL_JOURNEY_SLUG" }
```

The response should be `{ "productId": "YOUR_GOOGLE_PRODUCT_ID" }`.

Reference: [Dashboard function deployment](https://supabase.com/docs/guides/functions/quickstart-dashboard).

## 6. Schedule reconciliation in Supabase online

This implementation uses periodic Google API checks rather than a Pub/Sub RTDN
webhook. Reconciliation completes pending purchases, retries interrupted
acknowledgements, and removes access when Google reports a canceled/revoked purchase.
It only knows tokens already received by the server; purchases interrupted before
the first verification call must be recovered with **Restore Google Play purchase**.

1. Enable **pg_cron**, **pg_net**, and **Vault** through the project's database
   extensions/integrations settings if they are not enabled.
2. In **Vault**, create:
   - `journey_billing_project_url`: `https://YOUR_PROJECT_REF.supabase.co`;
   - `journey_billing_sync_secret`: the exact same value as `BILLING_SYNC_SECRET`.
3. Open a new SQL Editor query and run the contents of
   `supabase/setup-google-play-cron.sql` once.
4. Confirm the `journey-google-play-reconcile` job appears in Cron.
5. To trigger a manual check, run this SQL (it reads the private credential from Vault):

   ```sql
   select net.http_post(
     url := (select decrypted_secret from vault.decrypted_secrets
             where name = 'journey_billing_project_url') || '/functions/v1/verify-google-play-purchase',
     headers := jsonb_build_object('Content-Type', 'application/json',
       'x-billing-sync-secret', (select decrypted_secret from vault.decrypted_secrets
                                where name = 'journey_billing_sync_secret')),
     body := '{"action":"reconcile"}'::jsonb,
     timeout_milliseconds := 120000
   );
   ```

6. Inspect the corresponding entry in `net._http_response` or the function's logs.
   A healthy JSON result has `failed: 0`. Cron successfully dispatching an HTTP
   request does not mean purchase reconciliation succeeded; check the HTTP result.

Each run checks up to 50 records, oldest check first. At 15-minute intervals,
more than 50 active records take multiple runs for full coverage. Monitor failures,
API quotas, timeouts and backlog as usage grows. Failed rows retain their last
successful verification time; their check timestamp advances so other records
still get a turn. Investigate persistent failures. Refund revocations are periodic,
not immediate. A refund that
does not revoke the Google entitlement can retain access, matching Google's state.

Reference: [Supabase scheduled Edge Functions](https://supabase.com/docs/guides/functions/schedule-functions).

## 7. Check any existing paid-data policies or backend guards

The frontend now checks both entitlement sources. Existing online database policies
and Edge Functions were unavailable for inspection and might still check only
`journey_access`. Find those policies in SQL Editor:

```sql
select schemaname, tablename, policyname, qual, with_check
from pg_policies
where coalesce(qual, '') ilike '%journey_access%'
   or coalesce(with_check, '') ilike '%journey_access%';
```

For any paid-data predicate that currently requires an active Stripe entitlement,
extend **that entitlement predicate** with an OR for a matching row in
`public.google_play_journey_access`. Keep the rest of the policy, including user
ownership restrictions, unchanged. For example, where `journey_slug` below is the
qualified journey column of the protected table:

```sql
(
  exists (select 1 from public.journey_access ja
          where ja.user_id = auth.uid() and ja.journey_slug = YOUR_TABLE.journey_slug
            and ja.is_active)
  or exists (select 1 from public.google_play_journey_access gp
             where gp.user_id = auth.uid() and gp.journey_slug = YOUR_TABLE.journey_slug)
)
```

This is a predicate example, not a complete runnable policy. Inspect the actual
policy definition before altering it. Also search existing Edge Function code for
`journey_access`; server guards running as the service role must query the private
Google ledger with the **verified user ID**, `purchase_state = 'PURCHASED'`, and
`acknowledged = true`. The user-filtered view returns no rows to a service-role JWT
without a user identity. Keep all existing ownership checks.

No Stripe checkout/webhook change is required just to add Google billing.

## 8. Publish the frontend and configure PWABuilder

1. Publish the updated Blazor app through your normal deployment process.
2. In PWABuilder's Android package settings enable **Google Play billing**.
3. Set the Android package's **Start URL** to **`/?billing=google_play`**.
   Leave the regular web manifest's start URL at `/`. This Android-specific hint
   prevents a misconfigured Play build from sending users to Stripe. It is not
   authentication and never grants access. It persists for the current tab session.
4. Set the package ID to the exact value in the function secret.
5. Deploy `/.well-known/assetlinks.json` on the exact website host. Use the Play
   **app-signing certificate** SHA-256 fingerprint from Play Console, not just your
   upload-key fingerprint. Confirm the URL serves JSON rather than the Blazor SPA.
6. Upload the generated AAB to your internal testing track and install through
   Google Play using the configured test account.
7. Confirm the generated Android package meets the current Play target SDK and
   Billing Library requirements shown by Play Console before submission.

The detector returns `stripe` in normal browsers, `google_play` when the Play service
works, and `unavailable` when a known Play launch cannot use billing. Browser PWA
installation alone is not Google Play billing.

Reference: [PWABuilder Android setup](https://github.com/pwa-builder/PWABuilder/blob/main/docs/builder/android.md).

## 9. Verify the full flow before production

- Browser visitor: fourth-card button opens the existing Stripe checkout.
- Existing Stripe owner in the Play app: continues without another purchase.
- Unpaid Play visitor: signs in, returns to card four, sees Google's localized price,
  and opens Google's purchase sheet.
- Cancel: remains on card four without access or milestone progress.
- Purchase: private ledger records the signed-in user, acknowledged purchase, and
  purchased state; the button changes to Continue. Milestone progress changes only
  after the owner continues.
- Pending payment: no access until Google reports completed and acknowledgement
  succeeds. Retry using Restore or the scheduled job.
- Network interruption/reinstall: sign in with the same Journey Recreated account
  and the purchasing Google account; use Restore before attempting another payment.
- Different app account: an already-linked token cannot be transferred. The initial
  successful verification links a token to the signed-in app account; this TWA flow
  does not assume support for native obfuscated-account billing parameters.
- Refund with revoke: after reconciliation, the Google entitlement disappears.
  Any independent active Stripe purchase still grants access.
- Signed-out or other-account requests cannot read the private Google purchase
  tables, mutate purchases, call the record RPC, or see another user's view rows.
- Misconfigured Play billing: shows retry setup and never opens Stripe.

Use this administrator query to inspect state without displaying tokens:

```sql
select id, user_id, product_id, journey_slug, purchase_state, acknowledged,
       granted_at, last_verified_at, last_checked_at
from public.google_play_purchases
order by granted_at desc;
```

Local checks (Node 22.18+ or 24 for native TypeScript stripping):

```powershell
dotnet build HeartJourney.slnx --no-restore
dotnet run --project tests/PaywallPreviewChecks/PaywallPreviewChecks.csproj --no-restore
node --test tests/BillingChecks/billing-js.test.mjs tests/BillingChecks/google-play.test.mjs tests/BillingChecks/edge-function.test.mjs
```

The automated checks use fake store/backend responses. They do not charge money,
deploy SQL, or substitute for a real Google Play internal-track purchase test.
