/** The price of one TLD action for one registration period, as the TLD price table sends it. */
export interface TldPeriodPrice {
  /** The period in whole years. */
  years: number
  /** The price for that period in the table's currency, already rounded to its decimals. */
  amount: number
}
