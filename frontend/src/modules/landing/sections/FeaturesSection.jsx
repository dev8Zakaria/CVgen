const features = [
  "Profile management",
  "Opportunity analysis",
  "AI-assisted CV generation",
  "PDF export preparation",
];

export function FeaturesSection() {
  return (
    <section className="bg-background px-6 py-20">
      <div className="container">
        <div className="max-w-2xl">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-primary">
            Modules
          </p>
          <h2 className="mt-3 font-display text-3xl font-bold">
            A frontend shaped like the backend.
          </h2>
        </div>
        <div className="mt-10 grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {features.map((feature) => (
            <article key={feature} className="rounded-2xl border bg-card p-5 text-card-foreground shadow-sm">
              <h3 className="font-semibold">{feature}</h3>
              <p className="mt-3 text-sm text-muted-foreground">
                Placeholder module ready for API integration and feature-specific UI.
              </p>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
