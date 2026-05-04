import { AlertCircle, LoaderCircle, Mail, MapPin, Phone, RefreshCw, ShieldCheck, Sparkles, UserRound } from "lucide-react";

import { Button } from "@/components/ui/button";
import { useAuth } from "@/modules/auth/AuthProvider";
import { useProfile } from "@/modules/profile/hooks/useProfile";

function InfoCard({ icon: Icon, label, value, subtle = false }) {
  return (
    <div className="rounded-3xl border bg-card p-5 shadow-sm shadow-slate-900/5">
      <div className="flex items-center gap-3 text-sm font-semibold uppercase tracking-[0.18em] text-muted-foreground">
        <Icon className="h-4 w-4" />
        <span>{label}</span>
      </div>
      <p className={`mt-4 text-base ${subtle ? "text-muted-foreground" : "font-medium text-foreground"}`}>{value}</p>
    </div>
  );
}

export function ProfilePage() {
  const { user } = useAuth();
  const { profile, loading, error, reload } = useProfile();

  const displayName =
    profile?.fullName ||
    user?.name ||
    [user?.given_name, user?.family_name].filter(Boolean).join(" ") ||
    user?.preferred_username ||
    "Profile";

  if (loading) {
    return (
      <section className="rounded-[2rem] border bg-card p-10 text-center shadow-sm shadow-slate-900/5">
        <LoaderCircle className="mx-auto h-10 w-10 animate-spin text-primary" />
        <h1 className="mt-5 font-display text-3xl font-bold">Loading your profile</h1>
        <p className="mt-3 text-muted-foreground">We are fetching your backend profile and syncing it with your Keycloak identity.</p>
      </section>
    );
  }

  if (error) {
    return (
      <section className="rounded-[2rem] border border-destructive/20 bg-destructive/5 p-10 shadow-sm shadow-slate-900/5">
        <div className="flex items-start gap-4">
          <AlertCircle className="mt-1 h-6 w-6 text-destructive" />
          <div className="space-y-4">
            <div>
              <p className="text-sm font-semibold uppercase tracking-[0.25em] text-destructive">Profile</p>
              <h1 className="mt-2 font-display text-3xl font-bold">We could not load the profile API</h1>
            </div>
            <p className="max-w-2xl text-muted-foreground">
              The frontend is connected to the profile endpoint, but this request failed. Check the backend container or token state, then retry.
            </p>
            <Button type="button" className="rounded-full" onClick={reload}>
              <RefreshCw className="mr-2 h-4 w-4" />
              Retry request
            </Button>
          </div>
        </div>
      </section>
    );
  }

  return (
    <section className="space-y-8">
      <div className="rounded-[2rem] border bg-[radial-gradient(circle_at_top_right,_rgba(191,219,254,0.65),_transparent_35%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(248,250,252,0.9))] p-8 shadow-sm shadow-slate-900/5">
        <div className="flex flex-col gap-8 lg:flex-row lg:items-end lg:justify-between">
          <div className="space-y-4">
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-primary">Authenticated profile</p>
            <h1 className="font-display text-4xl font-bold tracking-tight">{displayName}</h1>
            <p className="max-w-2xl text-muted-foreground">
              This page is connected to `GET /api/profile/me`. Your first successful request auto-created the backend profile tied to your Keycloak account.
            </p>
          </div>
          <div className="rounded-3xl border bg-white/80 px-5 py-4 shadow-sm backdrop-blur">
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-muted-foreground">Current role</p>
            <p className="mt-2 text-lg font-semibold text-foreground">{profile?.role ?? "User"}</p>
          </div>
        </div>
      </div>

      <div className="grid gap-5 lg:grid-cols-[1.25fr_0.75fr]">
        <div className="space-y-5">
          <div className="grid gap-5 md:grid-cols-2">
            <InfoCard icon={Mail} label="Email" value={profile?.email ?? "No email returned"} />
            <InfoCard icon={UserRound} label="Username" value={user?.preferred_username ?? "No username returned"} />
            <InfoCard icon={Phone} label="Phone" value={profile?.phone || "Not filled yet"} subtle={!profile?.phone} />
            <InfoCard icon={MapPin} label="Location" value={profile?.location || "Not filled yet"} subtle={!profile?.location} />
          </div>

          <div className="rounded-[2rem] border bg-card p-6 shadow-sm shadow-slate-900/5">
            <div className="flex items-center gap-3">
              <Sparkles className="h-5 w-5 text-primary" />
              <h2 className="font-display text-2xl font-bold">Profile summary</h2>
            </div>
            <p className={`mt-4 leading-7 ${profile?.summary ? "text-foreground" : "text-muted-foreground"}`}>
              {profile?.summary || "The backend created this profile successfully, but the summary is still empty."}
            </p>
          </div>
        </div>

        <div className="space-y-5">
          <InfoCard icon={ShieldCheck} label="Keycloak subject" value={user?.sub ?? "Missing token subject"} subtle={!user?.sub} />
          <InfoCard icon={UserRound} label="Profile title" value={profile?.title || "Not filled yet"} subtle={!profile?.title} />
          <div className="rounded-[2rem] border bg-card p-6 shadow-sm shadow-slate-900/5">
            <h2 className="font-display text-2xl font-bold">What is wired now</h2>
            <ul className="mt-4 space-y-3 text-sm text-muted-foreground">
              <li>React is connected to Keycloak for login, sign up, and logout.</li>
              <li>Protected routes now sit behind the authenticated dashboard.</li>
              <li>The profile page calls the backend with your bearer token.</li>
            </ul>
            <Button type="button" variant="outline" className="mt-6 w-full rounded-full" onClick={reload}>
              <RefreshCw className="mr-2 h-4 w-4" />
              Refresh profile data
            </Button>
          </div>
        </div>
      </div>
    </section>
  );
}
