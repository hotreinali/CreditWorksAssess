import { useState, type FormEvent } from 'react'
import { ApiError, api } from '../api'
import type { Manufacturer } from '../types'

type Props = {
  manufacturers: Manufacturer[]
  onCreated: (message: string) => Promise<void>
}

const currentYear = new Date().getFullYear()

export function VehicleForm({ manufacturers, onCreated }: Props) {
  const [ownerName, setOwnerName] = useState('')
  const [manufacturerId, setManufacturerId] = useState('')
  const [year, setYear] = useState('')
  const [weight, setWeight] = useState('')
  const [errors, setErrors] = useState<Record<string, string[]>>({})
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitting(true)
    setErrors({})
    try {
      await api.createVehicle({
        ownerName,
        manufacturerId: Number(manufacturerId),
        yearOfManufacture: Number(year),
        weightKg: Number(weight),
      })
      setOwnerName('')
      setManufacturerId('')
      setYear('')
      setWeight('')
      await onCreated('Vehicle added successfully.')
    } catch (error) {
      if (error instanceof ApiError) setErrors(error.errors)
      else setErrors({ form: ['Could not connect to the API. Please try again.'] })
    } finally {
      setSubmitting(false)
    }
  }

  const errorFor = (name: string) => errors[name]?.[0]

  return (
    <section className="panel vehicle-form-panel" aria-labelledby="add-vehicle-title">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">New record</p>
          <h2 id="add-vehicle-title">Add a vehicle</h2>
        </div>
        <span className="step-number">01</span>
      </div>

      <form onSubmit={handleSubmit}>
        {errors.form && <div className="message message--error">{errors.form[0]}</div>}
        <label>
          Owner's name
          <input
            value={ownerName}
            onChange={(event) => setOwnerName(event.target.value)}
            maxLength={200}
            autoComplete="name"
            required
            aria-invalid={Boolean(errorFor('OwnerName'))}
          />
          {errorFor('OwnerName') && <span className="field-error">{errorFor('OwnerName')}</span>}
        </label>

        <div className="form-row">
          <label>
            Manufacturer
            <select
              value={manufacturerId}
              onChange={(event) => setManufacturerId(event.target.value)}
              required
              aria-invalid={Boolean(errorFor('ManufacturerId'))}
            >
              <option value="">Select manufacturer</option>
              {manufacturers.map((manufacturer) => (
                <option key={manufacturer.id} value={manufacturer.id}>{manufacturer.name}</option>
              ))}
            </select>
            {errorFor('ManufacturerId') && <span className="field-error">{errorFor('ManufacturerId')}</span>}
          </label>

          <label>
            Year
            <input
              type="number"
              value={year}
              onChange={(event) => setYear(event.target.value)}
              min="1886"
              max={currentYear + 1}
              placeholder={String(currentYear)}
              required
              aria-invalid={Boolean(errorFor('YearOfManufacture'))}
            />
            {errorFor('YearOfManufacture') && <span className="field-error">{errorFor('YearOfManufacture')}</span>}
          </label>
        </div>

        <label>
          Weight <span className="unit">kilograms</span>
          <input
            type="number"
            value={weight}
            onChange={(event) => setWeight(event.target.value)}
            min="0.01"
            step="0.01"
            placeholder="1850.75"
            required
            aria-invalid={Boolean(errorFor('WeightKg'))}
          />
          {errorFor('WeightKg') && <span className="field-error">{errorFor('WeightKg')}</span>}
        </label>

        <button className="button button--primary button--full" disabled={submitting}>
          {submitting ? 'Adding vehicle…' : 'Add vehicle'}
        </button>
      </form>
    </section>
  )
}
