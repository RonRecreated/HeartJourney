begin;

-- This migration does not change the existing journey_access table or Stripe policies.
create table public.google_play_products (
    product_id text primary key check (length(product_id) between 1 and 200),
    journey_slug text not null unique check (length(journey_slug) between 1 and 200),
    is_active boolean not null default true
);

create table public.google_play_purchases (
    id uuid primary key default gen_random_uuid(),
    purchase_token text not null unique,
    user_id uuid not null references auth.users(id) on delete cascade,
    product_id text not null references public.google_play_products(product_id),
    journey_slug text not null,
    package_name text not null,
    purchase_state text not null check (purchase_state in ('PURCHASED', 'PENDING', 'CANCELLED')),
    acknowledged boolean not null default false,
    order_id text,
    granted_at timestamptz not null default now(),
    last_verified_at timestamptz not null default now(),
    last_checked_at timestamptz not null default now()
);
create index google_play_purchases_reconciliation on public.google_play_purchases(last_checked_at)
    where purchase_state <> 'CANCELLED';
create index google_play_purchases_user on public.google_play_purchases(user_id, journey_slug);

alter table public.google_play_products enable row level security;
alter table public.google_play_purchases enable row level security;
revoke all on public.google_play_products, public.google_play_purchases from anon, authenticated;
grant all on public.google_play_products, public.google_play_purchases to service_role;

-- Owner-executed view intentionally filters on the verified JWT user ID. No tokens
-- or order IDs are exposed. Do not replace this filter with a client parameter.
create view public.google_play_journey_access with (security_barrier = true) as
select id, user_id, journey_slug, 'google_play'::text as access_type,
       granted_at, true as is_active
from public.google_play_purchases
where user_id = (select auth.uid()) and purchase_state = 'PURCHASED' and acknowledged;
revoke all on public.google_play_journey_access from public, anon, authenticated;
grant select on public.google_play_journey_access to authenticated;

-- Transactional ownership lock: retries by the same user are idempotent;
-- a token already linked to another account can never be transferred by a client.
create function public.record_google_play_purchase(
    p_token text, p_user uuid, p_product text, p_journey text, p_package text,
    p_state text, p_acknowledged boolean, p_order text
) returns uuid language plpgsql security definer set search_path = '' as $$
declare purchase_id uuid;
begin
    if not exists (select 1 from public.google_play_products
        where product_id = p_product and journey_slug = p_journey) then
        raise exception 'Unknown Google Play product';
    end if;
    insert into public.google_play_purchases as purchase
        (purchase_token, user_id, product_id, journey_slug, package_name, purchase_state, acknowledged, order_id)
    values (p_token, p_user, p_product, p_journey, p_package, p_state, p_acknowledged, p_order)
    on conflict (purchase_token) do update set
        -- Cancellations are terminal for a token, including concurrent stale lookups.
        purchase_state = case when purchase.purchase_state = 'CANCELLED' then 'CANCELLED'
                              else excluded.purchase_state end,
        acknowledged = excluded.acknowledged,
        order_id = excluded.order_id,
        last_verified_at = now(),
        last_checked_at = now()
    where purchase.user_id = excluded.user_id
        and purchase.product_id = excluded.product_id
        and purchase.journey_slug = excluded.journey_slug
        and purchase.package_name = excluded.package_name
    returning id into purchase_id;
    if purchase_id is null then raise exception 'Purchase ownership conflict' using errcode = '23505'; end if;
    return purchase_id;
end;
$$;
revoke all on function public.record_google_play_purchase(text, uuid, text, text, text, text, boolean, text)
    from public, anon, authenticated;
grant execute on function public.record_google_play_purchase(text, uuid, text, text, text, text, boolean, text)
    to service_role;

commit;
