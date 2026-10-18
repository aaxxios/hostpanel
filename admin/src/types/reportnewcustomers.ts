/** Sign-ups and orders of one month. */
export interface ReportNewCustomersRow {
  /** Month caption. */
  month: string
  /** New client accounts. */
  newSignups: number
  /** Orders placed. */
  ordersPlaced: number
  /** Orders completed. */
  ordersCompleted: number
  /** Share of sign-ups that ordered. */
  conversionRate: number
}
