# Milestone intro paywall change

Visitors can now explore the journey overview, choose a milestone, and read all four intro cards without an account. The fourth prayer card includes the purchase offer. Buying continues to unlock the whole journey. Existing owners continue into Growth Areas using the current progress rules.

Search the application source for `PAYWALL PREVIEW` to find comments explaining the changes. The full application/test patch is saved as `docs/paywall-preview.patch`.

## Reverting

Original source files were copied before editing to `docs/paywall-preview-backup/`. The manifest records original and implemented SHA-256 hashes. The restore script checks every file first and stops if subsequent edits would be lost. It does not change database records, payments, Git commits, or existing generated build files.

From `D:\heart-journey`, preview the restoration:

```powershell
.\docs\Revert-PaywallPreview.ps1 -WhatIf
```

Restore the original application and remove the helper/component/tests added for this change:

```powershell
.\docs\Revert-PaywallPreview.ps1
dotnet build HeartJourney.slnx --no-restore
```

Keep the backup folder until you are satisfied with the change. Backups, this document, the patch, and the restore script remain after restoration. If the script reports later edits, merge those edits manually with the original backups instead of forcing a restore.

## Checkout integration requiring deployment verification

The Supabase `create-checkout` Edge Function, payment webhook implementation, and database policies are outside this repository. No backend function, policy, price, or live payment was changed.

The checkout POST body now includes `journeySlug`, `successUrl`, and `cancelUrl`. For example:

```json
{
  "journeySlug": "relationship",
  "successUrl": "https://YOUR-APP/journeys/relationship/milestones/single/intro?step=4&checkout=success",
  "cancelUrl": "https://YOUR-APP/journeys/relationship/milestones/single/intro?step=4&checkout=cancel"
}
```

To support explicit return destinations, update the existing function to validate both URLs against the configured application origin and an allowed milestone intro path, then pass them to Stripe Checkout as `success_url` and `cancel_url`. Retain its existing authenticated user checks, journey/price lookup, and checkout metadata. Never trust a caller-supplied price, user ID, or return URL origin. Retain the verified payment webhook as the mechanism granting active `journey_access`.

If the existing function ignores the added URL fields, the app also saves a 30-minute browser navigation hint. On a full return to `/`, `/journeys`, or the matching journey overview, it restores the chosen fourth card. This works in the same browser and origin when local storage is available. An unknown checkout callback route, another browser/device, or another application origin cannot use that fallback. Closing checkout without returning leaves the hint until it expires; a later generic-page visit within that window may restore card four.

The fourth card verifies real journey ownership; `checkout=success` and browser hints never grant access. It checks up to five times over about eight seconds for webhook processing and offers a manual retry. A confirmed owner must click Continue before an intro visit is counted. Unpaid visits and checkout cancellation do not create or increment milestone progress.

The price is shown by the existing checkout deployment. The intro offer says the visitor can review price and payment terms before paying; no amount or purchase terms were invented.

The sign-up confirmation redirect now carries the local return destination. Verify that Supabase's allowed redirect URLs accept `/auth/confirmed` with its query string on the deployed origin. New users carry that destination through Welcome and profile setup.

## Validation

Run the dependency-free checks:

```powershell
dotnet run --project tests/PaywallPreviewChecks/PaywallPreviewChecks.csproj --no-restore
```

The checks exercise the explicit preview-page allowlist, guarded paid-page types, local redirect validation (including encoded attacks), and the actual checkout service using a fake HTTP handler. They confirm journey-wide purchase context, success/cancel destinations, authorization headers, and refusal to create checkout without a token. They do not contact Stripe or Supabase.

Before publishing, use a test account and Stripe test mode to verify new-user email confirmation/onboarding, existing-owner continuation, successful checkout and delayed webhook access, cancellation, and checkout failures. Test direct links to Growth Areas, reflection pages, and insights for signed-out and signed-in unpaid users. Confirm no intro progress is counted when payment is canceled.

The browser route guard preserves the application's payment boundary. Sanity content visibility and Supabase row-level access policies still need a backend audit if payment must restrict direct API access to paid content; those settings cannot be verified from this repository.
