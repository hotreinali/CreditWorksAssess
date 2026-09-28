import type {
  CategoryConfiguration,
  CategoryIcon,
  CategoryInput,
  Manufacturer,
  SortDirection,
  Vehicle,
  VehicleInput,
  VehicleSort,
} from './types'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

type ProblemDetails = {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  readonly status: number
  readonly errors: Record<string, string[]>

  constructor(
    message: string,
    status: number,
    errors: Record<string, string[]> = {},
  ) {
    super(message)
    this.status = status
    this.errors = errors
  }
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...options,
    headers: {
      ...(options?.body ? { 'Content-Type': 'application/json' } : {}),
      ...options?.headers,
    },
  })

  if (!response.ok) {
    const problem = (await response.json().catch(() => ({}))) as ProblemDetails
    throw new ApiError(
      problem.detail ?? problem.title ?? 'The request could not be completed.',
      response.status,
      problem.errors,
    )
  }

  return response.json() as Promise<T>
}

export const api = {
  getManufacturers: () => request<Manufacturer[]>('/api/manufacturers'),
  getCategoryIcons: () => request<CategoryIcon[]>('/api/category-icons'),
  getCategories: () => request<CategoryConfiguration>('/api/categories'),
  getVehicles: (sortBy: VehicleSort, direction: SortDirection) =>
    request<Vehicle[]>(`/api/vehicles?sortBy=${sortBy}&direction=${direction}`),
  createVehicle: (vehicle: VehicleInput) =>
    request<Vehicle>('/api/vehicles', {
      method: 'POST',
      body: JSON.stringify(vehicle),
    }),
  updateCategories: (version: number, categories: CategoryInput[]) =>
    request<CategoryConfiguration>('/api/categories', {
      method: 'PUT',
      body: JSON.stringify({ version, categories }),
    }),
}
