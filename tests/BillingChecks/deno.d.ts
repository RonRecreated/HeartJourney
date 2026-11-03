// Minimal host declaration for local TypeScript checks; Supabase supplies Deno.
declare const Deno: {
    env: { get(name: string): string | undefined };
    serve(handler: (request: Request) => Promise<Response>): void;
};
