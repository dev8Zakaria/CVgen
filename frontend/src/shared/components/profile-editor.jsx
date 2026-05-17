import { Pencil, Plus, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";

import { Button } from "@/components/ui/button";
import { ConfirmDialog } from "@/shared/components/app-ui";

function getInitialForm(fields, initialValue) {
  return fields.reduce((accumulator, field) => {
    accumulator[field.name] = initialValue?.[field.name] ?? "";
    return accumulator;
  }, {});
}

export function CollectionEditorCard({ title, description, fields, items, onSave, onDelete, emptyCopy }) {
  const [draft, setDraft] = useState(null);
  const [pendingDelete, setPendingDelete] = useState(null);
  const [form, setForm] = useState({});

  useEffect(() => {
    setForm(getInitialForm(fields, draft));
  }, [draft]);

  const startCreate = () => {
    setDraft({ id: "" });
  };

  const startEdit = (item) => {
    setDraft(item);
  };

  const handleSave = async (event) => {
    event.preventDefault();
    try {
      await onSave({ ...draft, ...form, id: draft?.id });
      setDraft(null);
    } catch {
      return;
    }
  };

  return (
    <div className="paper-panel p-6">
      <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
        <div>
          <h3 className="font-display text-3xl tracking-[-0.04em] text-foreground">{title}</h3>
          <p className="mt-2 text-sm leading-7 text-muted-foreground">{description}</p>
        </div>
        <Button type="button" variant="outline" onClick={startCreate}>
          <Plus className="mr-2 h-4 w-4" />
          Add
        </Button>
      </div>

      <div className="mt-5 space-y-3">
        {items.length === 0 ? <p className="text-sm text-muted-foreground">{emptyCopy}</p> : null}
        {items.map((item) => (
          <div key={item.id} className="rounded-[24px] border border-border bg-white/60 p-4 dark:bg-white/[0.03]">
            <div className="flex items-start justify-between gap-3">
              <div className="space-y-1">
                <p className="font-semibold text-foreground">{item[fields[0].name]}</p>
                {fields.slice(1).map((field) => (
                  <p key={field.name} className="text-sm text-muted-foreground">
                    {item[field.name]}
                  </p>
                ))}
              </div>
              <div className="flex gap-2">
                <Button type="button" size="sm" variant="outline" onClick={() => startEdit(item)}>
                  <Pencil className="h-3.5 w-3.5" />
                </Button>
                <Button type="button" size="sm" variant="outline" onClick={() => setPendingDelete(item)}>
                  <Trash2 className="h-3.5 w-3.5" />
                </Button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {draft ? (
        <form onSubmit={handleSave} className="mt-5 grid gap-4 rounded-[24px] border border-border bg-white/80 p-5 dark:bg-white/[0.04]">
          {fields.map((field) => (
            <label key={field.name} className="space-y-2">
              <span className="text-sm font-semibold text-foreground">{field.label}</span>
              {field.multiline ? (
                <textarea
                  className="field"
                  rows={field.rows || 4}
                  value={form[field.name]}
                  onChange={(event) => setForm((current) => ({ ...current, [field.name]: event.target.value }))}
                />
              ) : (
                <input
                  className="field"
                  type={field.type || "text"}
                  value={form[field.name]}
                  onChange={(event) => setForm((current) => ({ ...current, [field.name]: event.target.value }))}
                />
              )}
            </label>
          ))}
          <div className="flex justify-end gap-3">
            <Button type="button" variant="outline" onClick={() => setDraft(null)}>
              Cancel
            </Button>
            <Button type="submit">Save item</Button>
          </div>
        </form>
      ) : null}

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        title={`Delete ${title.slice(0, -1) || "item"}?`}
        description="This entry will be removed from the profile prototype."
        confirmLabel="Delete"
        tone="danger"
        onCancel={() => setPendingDelete(null)}
        onConfirm={async () => {
          try {
            await onDelete(pendingDelete.id);
            setPendingDelete(null);
          } catch {
            return;
          }
        }}
      />
    </div>
  );
}
