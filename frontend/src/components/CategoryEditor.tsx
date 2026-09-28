import { useState } from 'react'
import { ApiError, api } from '../api'
import type { CategoryConfiguration, CategoryIcon, CategoryInput } from '../types'
import { VehicleIcon } from './VehicleIcon'

type Props = {
  configuration: CategoryConfiguration
  icons: CategoryIcon[]
  onSaved: (configuration: CategoryConfiguration, message: string) => Promise<void>
  onReload: () => Promise<void>
}

export function CategoryEditor({ configuration, icons, onSaved, onReload }: Props) {
  const [categories, setCategories] = useState<CategoryInput[]>(() =>
    configuration.categories.map(({ name, iconKey, startsAtKg }) => ({ name, iconKey, startsAtKg })))
  const [errors, setErrors] = useState<string[]>([])
  const [saving, setSaving] = useState(false)

  function update(index: number, values: Partial<CategoryInput>) {
    setCategories((current) => current.map((category, position) =>
      position === index ? { ...category, ...values } : category))
  }

  function addCategory() {
    const lastStart = categories.at(-1)?.startsAtKg ?? 0.01
    setCategories((current) => [
      ...current,
      { name: 'New category', iconKey: icons[0]?.key ?? 'car', startsAtKg: Math.round((lastStart + 500) * 100) / 100 },
    ])
  }

  async function save() {
    setSaving(true)
    setErrors([])
    try {
      const updated = await api.updateCategories(configuration.version, categories)
      await onSaved(updated, 'Category configuration saved. Vehicle categories are now up to date.')
    } catch (error) {
      if (error instanceof ApiError) {
        if (error.status === 409) setErrors(['This configuration was changed elsewhere. Reload it before saving.'])
        else setErrors(Object.values(error.errors).flat().length ? Object.values(error.errors).flat() : [error.message])
      } else setErrors(['Could not connect to the API. Please try again.'])
    } finally {
      setSaving(false)
    }
  }

  const sorted = [...categories].sort((a, b) => a.startsAtKg - b.startsAtKg)

  return (
    <section className="panel category-panel" aria-labelledby="category-title">
      <div className="panel-heading category-heading">
        <div>
          <p className="eyebrow">Weight rules · version {configuration.version}</p>
          <h2 id="category-title">Category configuration</h2>
          <p className="section-description">Ranges include their starting weight and continue up to the next boundary.</p>
        </div>
        <button className="button button--secondary" onClick={addCategory}>+ Add category</button>
      </div>

      {errors.length > 0 && (
        <div className="message message--error" role="alert">
          {errors.map((error) => <div key={error}>{error}</div>)}
          {errors.some((error) => error.includes('Reload')) && (
            <button className="text-button" onClick={() => void onReload()}>Reload latest configuration</button>
          )}
        </div>
      )}

      <div className="category-list">
        {categories.map((category, index) => {
          const sortedIndex = sorted.findIndex((item) => item === category)
          const nextStart = sorted[sortedIndex + 1]?.startsAtKg
          return (
            <article className="category-row" key={index}>
              <div className="category-symbol">
                <VehicleIcon iconKey={category.iconKey} label={category.name} size="large" />
              </div>
              <label>
                Category name
                <input value={category.name} maxLength={100} onChange={(event) => update(index, { name: event.target.value })} />
              </label>
              <label>
                Starts at (kg)
                <input type="number" min="0.01" step="0.01" value={category.startsAtKg}
                  onChange={(event) => update(index, { startsAtKg: Number(event.target.value) })} />
                <span className="range-note">{nextStart == null ? 'No upper limit' : `Up to ${nextStart.toLocaleString()} kg`}</span>
              </label>
              <label>
                Icon
                <select value={category.iconKey} onChange={(event) => update(index, { iconKey: event.target.value })}>
                  {icons.map((icon) => <option key={icon.key} value={icon.key}>{icon.label}</option>)}
                </select>
              </label>
              <button className="icon-button" aria-label={`Delete ${category.name}`} title="Delete category"
                onClick={() => setCategories((current) => current.filter((_, position) => position !== index))}>×</button>
            </article>
          )
        })}
      </div>

      <div className="category-actions">
        <p>Saving updates every vehicle's displayed category immediately.</p>
        <button className="button button--primary" onClick={() => void save()} disabled={saving}>
          {saving ? 'Saving configuration…' : 'Save configuration'}
        </button>
      </div>
    </section>
  )
}
