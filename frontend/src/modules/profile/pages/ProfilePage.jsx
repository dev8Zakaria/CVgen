import { Save } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { InlineTagEditor, SectionHeading, SkeletonBlock, StatCard } from "@/shared/components/app-ui";
import { CollectionEditorCard } from "@/shared/components/profile-editor";
import { usePrototypeApp } from "@/shared/providers/PrototypeAppProvider";
import { useToast } from "@/shared/providers/ToastProvider";

export function ProfilePage() {
  const { hydrated, profile, updateProfile, saveCollectionItem, removeCollectionItem, addSkill, removeSkill, usingBackendData } = usePrototypeApp();
  const toast = useToast();
  const [form, setForm] = useState(null);
  const [saving, setSaving] = useState(false);

  if (!hydrated) {
    return (
      <div className="grid gap-5">
        <SkeletonBlock className="h-40" />
        <SkeletonBlock className="h-72" />
        <SkeletonBlock className="h-72" />
      </div>
    );
  }

  const safeForm = form ?? profile;

  const saveProfile = async () => {
    try {
      setSaving(true);
      await updateProfile({
        phone: safeForm.phone,
        address: safeForm.address,
        professionalTitle: safeForm.professionalTitle,
        summary: safeForm.summary,
      });
      toast.success("Profile updated", "Your real backend profile has been updated.");
    } catch {
      toast.error("Profile update failed", "We could not save the profile changes to the backend.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="My Profile"
        title="Build the master profile your AI generator will refine."
        description="This is the heart of the product. The richer and cleaner your profile is, the stronger every generated CV becomes."
        action={
          <Button type="button" onClick={saveProfile}>
            <Save className="mr-2 h-4 w-4" />
            {saving ? "Saving..." : "Save profile"}
          </Button>
        }
      />

      <div className="grid gap-5 lg:grid-cols-3">
        <StatCard label="Experience Entries" value={profile.experience.length} meta="Your most relevant roles should be tight, evidence-based, and recent." />
        <StatCard label="Core Skills" value={profile.skills.length} meta="Skills become matched and missing signals inside job analysis." />
        <StatCard label="Projects & Proof" value={profile.projects.length} meta="Projects strengthen credibility and expand the narrative beyond titles." />
      </div>

      <div className="grid gap-5 xl:grid-cols-[1.2fr_0.8fr]">
        <div className="paper-panel p-6">
          <SectionHeading
            eyebrow="Profile Core"
            title="Personal and professional essentials"
            description="These fields appear across the CV preview and are reused as AI input."
            className="md:items-start"
          />

          <div className="mt-6 grid gap-4 md:grid-cols-2">
            <label className="space-y-2">
              <span className="text-sm font-semibold text-foreground">Full name</span>
              <input className="field opacity-70" value={safeForm.fullName} readOnly />
            </label>
            <label className="space-y-2">
              <span className="text-sm font-semibold text-foreground">Email</span>
              <input className="field opacity-70" value={safeForm.email} readOnly />
            </label>
            <label className="space-y-2">
              <span className="text-sm font-semibold text-foreground">Phone</span>
              <input className="field" value={safeForm.phone} onChange={(event) => setForm((current) => ({ ...(current ?? profile), phone: event.target.value }))} />
            </label>
            <label className="space-y-2">
              <span className="text-sm font-semibold text-foreground">Address</span>
              <input className="field" value={safeForm.address} onChange={(event) => setForm((current) => ({ ...(current ?? profile), address: event.target.value }))} />
            </label>
            <label className="space-y-2 md:col-span-2">
              <span className="text-sm font-semibold text-foreground">Professional title</span>
              <input
                className="field"
                value={safeForm.professionalTitle}
                onChange={(event) => setForm((current) => ({ ...(current ?? profile), professionalTitle: event.target.value }))}
              />
            </label>
            <label className="space-y-2 md:col-span-2">
              <span className="text-sm font-semibold text-foreground">Summary</span>
              <textarea
                className="field min-h-[180px]"
                value={safeForm.summary}
                onChange={(event) => setForm((current) => ({ ...(current ?? profile), summary: event.target.value }))}
              />
            </label>
          </div>
          {usingBackendData ? (
            <p className="mt-4 text-xs leading-6 text-muted-foreground">
              Full name and email are sourced from your authenticated identity/backend profile. Title, summary, phone, and address are saved to the backend.
            </p>
          ) : null}
        </div>

        <div className="paper-panel p-6">
          <SectionHeading
            eyebrow="Skills"
            title="Tag your strongest capabilities"
            description="Add or remove skill tags inline. These are reused in job matching and the CV preview."
            className="md:items-start"
          />
          <div className="mt-6">
            <InlineTagEditor
              items={profile.skills}
              onAdd={(skill) => {
                addSkill(skill);
                toast.success("Skill added", `"${skill}" is now part of your profile skill set.`);
              }}
              onRemove={(skill) => {
                removeSkill(skill);
                toast.success("Skill removed", `"${skill}" was removed from the profile.`);
              }}
              placeholder="Add skill, e.g. Prompt Design"
            />
          </div>
        </div>
      </div>

      <CollectionEditorCard
        title="Education"
        description="Degrees, programs, and formal learning that strengthen the narrative."
        items={profile.education}
        onSave={(item) => {
          saveCollectionItem("education", item);
          toast.success("Education updated", "Your education section has been refreshed.");
        }}
        onDelete={(id) => {
          removeCollectionItem("education", id);
          toast.success("Education removed");
        }}
        emptyCopy="No education entries yet."
        fields={[
          { name: "school", label: "School" },
          { name: "degree", label: "Degree" },
          { name: "period", label: "Period" },
          { name: "location", label: "Location" },
        ]}
      />

      <CollectionEditorCard
        title="Work Experience"
        description="Keep each role sharp, concrete, and outcome-driven."
        items={profile.experience.map((entry) => ({ ...entry, bullets: entry.bullets.join(" • ") }))}
        onSave={(item) => {
          saveCollectionItem("experience", {
            ...item,
            bullets: String(item.bullets || "")
              .split("•")
              .map((bullet) => bullet.trim())
              .filter(Boolean),
          });
          toast.success("Experience updated");
        }}
        onDelete={(id) => {
          removeCollectionItem("experience", id);
          toast.success("Experience removed");
        }}
        emptyCopy="No work experience entries yet."
        fields={[
          { name: "company", label: "Company" },
          { name: "role", label: "Role" },
          { name: "period", label: "Period" },
          { name: "location", label: "Location" },
          { name: "bullets", label: "Bullet points", multiline: true, rows: 5 },
        ]}
      />

      <CollectionEditorCard
        title="Projects"
        description="Add proof-of-work that gives recruiters and the AI engine more depth."
        items={profile.projects}
        onSave={(item) => {
          saveCollectionItem("projects", item);
          toast.success("Project updated");
        }}
        onDelete={(id) => {
          removeCollectionItem("projects", id);
          toast.success("Project removed");
        }}
        emptyCopy="No projects yet."
        fields={[
          { name: "name", label: "Project name" },
          { name: "role", label: "Role" },
          { name: "description", label: "Description", multiline: true, rows: 4 },
        ]}
      />

      <div className="grid gap-5 xl:grid-cols-2">
        <CollectionEditorCard
          title="Languages"
          description="Show linguistic flexibility for international roles."
          items={profile.languages}
          onSave={(item) => {
            saveCollectionItem("languages", item);
            toast.success("Language updated");
          }}
          onDelete={(id) => {
            removeCollectionItem("languages", id);
            toast.success("Language removed");
          }}
          emptyCopy="No language entries yet."
          fields={[
            { name: "name", label: "Language" },
            { name: "level", label: "Level" },
          ]}
        />

        <CollectionEditorCard
          title="Certifications"
          description="Capture signals of depth, rigor, and current practice."
          items={profile.certifications}
          onSave={(item) => {
            saveCollectionItem("certifications", item);
            toast.success("Certification updated");
          }}
          onDelete={(id) => {
            removeCollectionItem("certifications", id);
            toast.success("Certification removed");
          }}
          emptyCopy="No certifications yet."
          fields={[
            { name: "title", label: "Certification" },
            { name: "issuer", label: "Issuer" },
            { name: "year", label: "Year" },
          ]}
        />
      </div>
    </div>
  );
}
