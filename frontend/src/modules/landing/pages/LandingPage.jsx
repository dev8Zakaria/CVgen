import { CTASection } from "@/modules/landing/sections/CTASection";
import { FeaturesSection } from "@/modules/landing/sections/FeaturesSection";
import { HeroSection } from "@/modules/landing/sections/HeroSection";
import { HowItWorksSection } from "@/modules/landing/sections/HowItWorksSection";

export function LandingPage() {
  return (
    <main>
      <HeroSection />
      <FeaturesSection />
      <HowItWorksSection />
      <CTASection />
    </main>
  );
}
