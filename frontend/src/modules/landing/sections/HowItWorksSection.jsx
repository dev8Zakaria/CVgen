const steps = ["Create profile", "Paste job offer", "Generate tailored CV"];

export function HowItWorksSection() {
  return (
    <section className="bg-muted/50 px-6 py-20">
      <div className="container">
        <h2 className="font-display text-3xl font-bold">How it works</h2>
        <div className="mt-10 grid gap-4 md:grid-cols-3">
          {steps.map((step, index) => (
            <article key={step} className="rounded-2xl bg-background p-6 shadow-sm">
              <span className="text-sm font-semibold text-primary">0{index + 1}</span>
              <h3 className="mt-4 font-semibold">{step}</h3>
              <p className="mt-3 text-sm text-muted-foreground">
                This section is intentionally simple now and ready for GSAP/Three.js polish later.
              </p>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
