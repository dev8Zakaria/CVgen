import { Slot } from "@radix-ui/react-slot";
import { cva } from "class-variance-authority";

import { cn } from "@/shared/utils/cn";

const buttonVariants = cva(
  "inline-flex items-center justify-center whitespace-nowrap rounded-2xl text-sm font-semibold transition duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/30 disabled:pointer-events-none disabled:opacity-50",
  {
    variants: {
      variant: {
        default:
          "bg-primary text-primary-foreground shadow-[0_18px_44px_rgba(37,99,235,0.28)] hover:-translate-y-0.5 hover:bg-blue-500 hover:shadow-[0_22px_52px_rgba(37,99,235,0.34)]",
        outline:
          "border border-border bg-white/72 text-foreground shadow-sm hover:border-primary/25 hover:bg-white hover:text-foreground dark:bg-slate-950/45 dark:hover:bg-slate-900/80",
        ghost: "text-muted-foreground hover:bg-slate-900/[0.04] hover:text-foreground dark:hover:bg-white/[0.06]",
        accent:
          "bg-accent text-accent-foreground shadow-[0_18px_44px_rgba(6,182,212,0.24)] hover:-translate-y-0.5 hover:bg-cyan-400 hover:shadow-[0_22px_52px_rgba(6,182,212,0.3)]",
        destructive: "bg-destructive text-destructive-foreground shadow-sm hover:brightness-95",
      },
      size: {
        default: "h-11 px-5",
        sm: "h-9 px-3 text-xs",
        lg: "h-12 px-6 text-sm",
        icon: "h-11 w-11",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  },
);

export function Button({ className, variant, size, asChild = false, ...props }) {
  const Comp = asChild ? Slot : "button";
  return <Comp className={cn(buttonVariants({ variant, size, className }))} {...props} />;
}

export { buttonVariants };
