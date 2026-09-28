import type { SortDirection, Vehicle, VehicleSort } from '../types'
import { VehicleIcon } from './VehicleIcon'

type Props = {
  vehicles: Vehicle[]
  loading: boolean
  sortBy: VehicleSort
  direction: SortDirection
  onSort: (field: VehicleSort) => void
}

const columns: { field: VehicleSort; label: string }[] = [
  { field: 'ownerName', label: 'Owner' },
  { field: 'manufacturer', label: 'Manufacturer' },
  { field: 'yearOfManufacture', label: 'Year' },
  { field: 'weight', label: 'Weight' },
]

export function VehicleList({ vehicles, loading, sortBy, direction, onSort }: Props) {
  const sortLabel: 'ascending' | 'descending' = direction === 'asc' ? 'ascending' : 'descending'

  return (
    <section className="panel fleet-panel" aria-labelledby="fleet-title">
      <div className="panel-heading fleet-heading">
        <div>
          <p className="eyebrow">Fleet register</p>
          <h2 id="fleet-title">Vehicles</h2>
        </div>
        <span className="record-count">{vehicles.length} {vehicles.length === 1 ? 'vehicle' : 'vehicles'}</span>
      </div>

      {loading ? (
        <div className="empty-state">Loading vehicles…</div>
      ) : vehicles.length === 0 ? (
        <div className="empty-state">
          <span className="empty-mark">CW</span>
          <h3>No vehicles yet</h3>
          <p>Add the first vehicle using the form.</p>
        </div>
      ) : (
        <div className="table-scroll">
          <table>
            <thead>
              <tr>
                {columns.map(({ field, label }) => (
                  <th key={field} aria-sort={sortBy === field ? sortLabel : 'none'}>
                    <button className="sort-button" onClick={() => onSort(field)}>
                      {label}
                      <span aria-hidden="true">{sortBy === field ? (direction === 'asc' ? '↑' : '↓') : '↕'}</span>
                    </button>
                  </th>
                ))}
                <th>Category</th>
              </tr>
            </thead>
            <tbody>
              {vehicles.map((vehicle) => (
                <tr key={vehicle.id}>
                  <td data-label="Owner"><strong>{vehicle.ownerName}</strong></td>
                  <td data-label="Manufacturer">{vehicle.manufacturer.name}</td>
                  <td data-label="Year">{vehicle.yearOfManufacture}</td>
                  <td data-label="Weight">{vehicle.weightKg.toLocaleString(undefined, { maximumFractionDigits: 2 })} kg</td>
                  <td data-label="Category">
                    <span className="category-badge">
                      <VehicleIcon iconKey={vehicle.category.iconKey} label={vehicle.category.name} />
                      {vehicle.category.name}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
