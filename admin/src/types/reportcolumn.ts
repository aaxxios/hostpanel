/**
 * Column description for the shared report table.
 */
import type { UiAlign } from '@/utils/ui/table'

/** Closed set of ways a report table cell formats its raw value. */
export const ReportCellFormat = {
  Text: 'text',
  Money: 'money',
  Percent: 'percent',
  Integer: 'integer',
  Date: 'date',
  Status: 'status',
  Id: 'id',
  Years: 'years',
} as const

/** One of the cell formats. */
export type ReportCellFormat = (typeof ReportCellFormat)[keyof typeof ReportCellFormat]

/** One column of a report table. */
export interface ReportColumn {
  /** Property of the row the column reads, or a dot path into a nested record (`dates.created`); also the suffix of its `cell-<key>` slot. */
  key: string
  /** Header caption (already translated). */
  label: string
  /** Horizontal alignment of the header and the cells; numbers are usually `right`. */
  align?: UiAlign
  /** How the raw value is turned into text; `text` when omitted. */
  format?: ReportCellFormat
  /**
   * Property of the row (or dot path into a nested record) holding the ISO 4217 code a `money` cell is in. When set, the cell is
   * written in that row's own currency instead of through the table's `formatMoney`.
   */
  currencyKey?: string
}
