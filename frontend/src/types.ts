export type Manufacturer = {
  id: number
  name: string
}

export type CategorySummary = {
  id: number
  name: string
  iconKey: string
}

export type Vehicle = {
  id: number
  ownerName: string
  manufacturer: Manufacturer
  yearOfManufacture: number
  weightKg: number
  category: CategorySummary
}

export type Category = CategorySummary & {
  startsAtKg: number
  endsBeforeKg: number | null
}

export type CategoryConfiguration = {
  version: number
  categories: Category[]
}

export type CategoryInput = {
  name: string
  iconKey: string
  startsAtKg: number
}

export type CategoryIcon = {
  key: string
  label: string
}

export type VehicleInput = {
  ownerName: string
  manufacturerId: number
  yearOfManufacture: number
  weightKg: number
}

export type VehicleSort = 'ownerName' | 'manufacturer' | 'yearOfManufacture' | 'weight'
export type SortDirection = 'asc' | 'desc'
