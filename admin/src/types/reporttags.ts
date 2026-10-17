/** Usage of one ticket tag. */
export interface ReportTagRow {
  /** The tag. */
  tag: string
  /** Tickets carrying it. */
  count: number
}

/** The ticket tags report. */
export interface ReportTags {
  /** One row per tag. */
  rows: ReportTagRow[]
}
