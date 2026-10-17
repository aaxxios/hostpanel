/** One configurable option chosen at order time, as the hosting-service detail lists it. */
export interface ServiceConfigOption {
  /** Option id. */
  id: number
  /** Option name. */
  option: string
  /** Option type. */
  type: string
  /** The chosen value. */
  value: string
}
