/** VAT figures of one country and rate. */
export interface ReportVatRow {
  /** Country name. */
  countryName: string
  /** ISO-2 country code. */
  countryCode: string
  /** VAT rate in percent. */
  vatRate: number
  /** Invoices counted. */
  numberOfInvoices: number
  /** Value invoiced without VAT. */
  totalExclVat: number
  /** VAT collected. */
  totalVatCollected: number
  /** Currency of the figures. */
  currency: string
}

/** The VAT MOSS report. */
export interface ReportVat {
  /** Caption of the quarter. */
  periodLabel: string
  /** One row per country and rate. */
  rows: ReportVatRow[]
}
