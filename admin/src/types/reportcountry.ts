/** Active clients of one country. */
export interface ReportCountryRow {
  /** ISO-2 country code, or `Unknown`. */
  country: string
  /** Number of active clients. */
  clientCount: number
  /** Revenue from the country clients. */
  totalRevenue: number
}

/** Active clients of one city, for the map markers. */
export interface ReportCityRow {
  /** City name. */
  city: string
  /** ISO-2 country code. */
  country: string
  /** Number of active clients. */
  clientCount: number
}
