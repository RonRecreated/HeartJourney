# User Accounts and Profiles

Heart Journey uses Supabase for user authentication and user-owned data.

## Responsibilities

### Sanity

Sanity stores public content:

- Journeys
- Milestones
- Dimensions
- Reflection Prompts
- Answer Options
- Insights
- Action Steps
- Bible Verses

### Supabase

Supabase stores private user data:

- User profiles
- Onboarding answers
- Selected reflection answers
- Optional notes
- Journey progress
- Daily answer usage
- Insight summaries

### Blazor WebAssembly

Blazor controls the user experience:

- Sign in
- Onboarding
- Journey navigation
- Reflection answering
- Notes
- Insight display

## Key Design Principle

Sanity answers:

> What content should the user see?

Supabase answers:

> Who is the user, what have they answered, and how are they progressing?

Blazor answers:

> How should the journey feel?

## Privacy Principle

Heart Journey should only ask for profile information that helps personalize the journey.

Questions involving gender, age bracket, religious background, relationship season, or relationship history should be presented with care and should allow the user to skip nonessential questions.

## Evaluation Principle

The Reflection Prompt page should not immediately show internal status or concern labels.

The app may store internal values such as:

- healthy
- needsAttention
- actionNeeded
- none
- low
- moderate
- high
- critical

But these should be used later to build Insight summaries.

## Daily Reflection Limit

Users should be limited to approximately 15 new reflection answers per day.

Changing a previously selected answer should not count as a new answer for the day.

When the daily limit is reached, Heart Journey should encourage the user to pause, pray, and return later.