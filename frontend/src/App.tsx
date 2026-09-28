import { useEffect, useState } from 'react'
import { ApiError, api } from './api'
import { CategoryEditor } from './components/CategoryEditor'
import { VehicleForm } from './components/VehicleForm'
import { VehicleList } from './components/VehicleList'
import type { CategoryConfiguration, CategoryIcon, Manufacturer, SortDirection, Vehicle, VehicleSort } from './types'
import './App.css'

type View = 'vehicles' | 'categories'

function App() {
  const [view, setView] = useState<View>('vehicles')
  const [manufacturers, setManufacturers] = useState<Manufacturer[]>([])
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [configuration, setConfiguration] = useState<CategoryConfiguration | null>(null)
  const [icons, setIcons] = useState<CategoryIcon[]>([])
  const [sortBy, setSortBy] = useState<VehicleSort>('ownerName')
  const [direction, setDirection] = useState<SortDirection>('asc')
  const [loading, setLoading] = useState(true)
  const [vehicleLoading, setVehicleLoading] = useState(true)
  const [pageError, setPageError] = useState('')
  const [notice, setNotice] = useState('')

  async function loadVehicles(requestSort = sortBy, requestDirection = direction) {
    setVehicleLoading(true)
    try {
      setVehicles(await api.getVehicles(requestSort, requestDirection))
    } catch {
      setPageError('Could not load vehicles. Check that the API and database are running.')
    } finally {
      setVehicleLoading(false)
    }
  }

  async function loadConfiguration() {
    setConfiguration(await api.getCategories())
  }

  useEffect(() => {
    let active = true
    Promise.all([api.getManufacturers(), api.getCategories(), api.getCategoryIcons(), api.getVehicles('ownerName', 'asc')])
      .then(([manufacturerData, categoryData, iconData, vehicleData]) => {
        if (!active) return
        setManufacturers(manufacturerData)
        setConfiguration(categoryData)
        setIcons(iconData)
        setVehicles(vehicleData)
      })
      .catch((error: unknown) => {
        if (!active) return
        const message = error instanceof ApiError ? error.message : 'Could not connect to the API.'
        setPageError(`${message} Check that the API and database are running.`)
      })
      .finally(() => {
        if (active) {
          setLoading(false)
          setVehicleLoading(false)
        }
      })
    return () => { active = false }
  }, [])

  function showNotice(message: string) {
    setNotice(message)
    window.setTimeout(() => setNotice(''), 4500)
  }

  function handleSort(field: VehicleSort) {
    const nextDirection = field === sortBy && direction === 'asc' ? 'desc' : 'asc'
    setSortBy(field)
    setDirection(nextDirection)
    void loadVehicles(field, nextDirection)
  }

  async function handleVehicleCreated(message: string) {
    await loadVehicles()
    showNotice(message)
  }

  async function handleCategoriesSaved(updated: CategoryConfiguration, message: string) {
    setConfiguration(updated)
    await loadVehicles()
    showNotice(message)
  }

  async function handleReloadConfiguration() {
    try {
      await loadConfiguration()
      showNotice('Latest category configuration loaded.')
    } catch {
      setPageError('Could not reload category configuration.')
    }
  }

  return (
    <div className="app-shell">
      <header className="site-header">
        <a className="brand" href="#top" aria-label="CreditWorks vehicle manager home">
          <span className="brand-mark">CW</span><span>CreditWorks</span>
        </a>
        <nav aria-label="Primary navigation">
          <button className={view === 'vehicles' ? 'active' : ''} onClick={() => setView('vehicles')}>Vehicles</button>
          <button className={view === 'categories' ? 'active' : ''} onClick={() => setView('categories')}>Weight categories</button>
        </nav>
      </header>

      <main id="top">
        <section className="page-intro">
          <div>
            <p className="eyebrow">Vehicle administration</p>
            <h1>{view === 'vehicles' ? 'Know your fleet.' : 'Define every weight.'}</h1>
            <p>{view === 'vehicles'
              ? 'Add vehicles, keep the register clear, and let weight rules handle every category.'
              : 'Maintain complete, continuous ranges. Every saved change is reflected across the fleet.'}</p>
          </div>
          <div className="status-card" aria-label="System status">
            <span className="status-dot" />
            <div><strong>Classification live</strong><span>{configuration?.categories.length ?? 0} active categories</span></div>
          </div>
        </section>

        {pageError && <div className="message message--error page-message" role="alert">{pageError}<button className="text-button" onClick={() => window.location.reload()}>Try again</button></div>}
        {notice && <div className="toast" role="status">✓ {notice}</div>}

        {loading ? <div className="loading-card">Loading vehicle management…</div> : view === 'vehicles' ? (
          <div className="vehicle-layout">
            <VehicleForm manufacturers={manufacturers} onCreated={handleVehicleCreated} />
            <VehicleList vehicles={vehicles} loading={vehicleLoading} sortBy={sortBy} direction={direction} onSort={handleSort} />
          </div>
        ) : configuration ? (
          <CategoryEditor key={configuration.version} configuration={configuration} icons={icons} onSaved={handleCategoriesSaved} onReload={handleReloadConfiguration} />
        ) : null}
      </main>

      <footer><span>CreditWorks vehicle manager</span><span>Weights in kilograms · Categories calculated live</span></footer>
    </div>
  )
}

export default App
