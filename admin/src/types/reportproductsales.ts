/** Units and income of one product in the income by product report. */
export interface ReportIncomeProduct {
  /** Product id. */
  productId: number
  /** Product name. */
  productName: string
  /** Units sold. */
  unitsSold: number
  /** Income from the product. */
  totalIncome: number
}

/** A product group of the income by product report. */
export interface ReportIncomeGroup {
  /** Group name. */
  groupName: string
  /** Products of the group. */
  products: ReportIncomeProduct[]
  /** Units sold in the group. */
  groupUnitsSold: number
  /** Income of the group. */
  groupIncome: number
}

/** The income by product report. */
export interface ReportIncomeByProduct {
  /** Month number, 1 to 12. */
  month: number
  /** Year. */
  year: number
  /** Product groups. */
  groups: ReportIncomeGroup[]
  /** Units sold overall. */
  totalUnitsSold: number
  /** Income overall. */
  totalIncome: number
  /** ISO 4217 code of the base currency every income figure is converted to. */
  currency: string
}

/** Units and value of one product (monthly orders report; the income report maps onto it). */
export interface ReportSalesProduct {
  /** Product id. */
  productId: number
  /** Product name. */
  productName: string
  /** Units ordered: one per order line. */
  unitsSold: number
  /** Value of the units sold. */
  value: number
}

/** A product group of the monthly orders report. */
export interface ReportSalesGroup {
  /** Group name. */
  groupName: string
  /** Products of the group. */
  products: ReportSalesProduct[]
  /** Units sold in the group. */
  groupUnitsSold: number
  /** Value of the group. */
  groupValue: number
}

/** The monthly orders report. */
export interface ReportMonthlyOrders {
  /** Month number, 1 to 12. */
  month: number
  /** Year. */
  year: number
  /** Product groups. */
  groups: ReportSalesGroup[]
  /** Units sold overall. */
  totalUnitsSold: number
  /** Value overall. */
  totalValue: number
  /** ISO 4217 code of the base currency every value is converted to. */
  currency: string
}
