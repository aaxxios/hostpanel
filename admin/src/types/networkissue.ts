/**
 * A network issue, its payload and filter, plus the closed value sets the API speaks for it
 * (type, priority, status, list tab).
 */

/** Kind of network issue. */
export enum NetworkIssueType {
  /** Server-related issue. */
  Server = 'Server',
  /** Any other kind of issue. */
  Other = 'Other',
}

/** Priority of a network issue. */
export enum NetworkIssuePriority {
  /** Minimal impact. */
  Low = 'Low',
  /** Moderate impact. */
  Medium = 'Medium',
  /** Significant impact. */
  High = 'High',
  /** Service-threatening impact. */
  Critical = 'Critical',
}

/** Lifecycle status of a network issue. */
export enum NetworkIssueStatus {
  /** Reported, not yet looked at. */
  Reported = 'Reported',
  /** Being investigated. */
  Investigating = 'Investigating',
  /** Maintenance or fix scheduled. */
  Scheduled = 'Scheduled',
  /** Resolved. */
  Resolved = 'Resolved',
}

/** The tabs of the network issues list: the status groups the API can filter by. */
export enum NetworkIssueTab {
  /** Issues still open (reported or investigating). */
  Open = 'Open',
  /** Issues with a scheduled fix. */
  Scheduled = 'Scheduled',
  /** Resolved issues. */
  Resolved = 'Resolved',
}

/** The statuses each list tab asks the API for. */
export const NETWORK_ISSUE_TAB_STATUSES: Readonly<Record<NetworkIssueTab, readonly NetworkIssueStatus[]>> = {
  [NetworkIssueTab.Open]: [NetworkIssueStatus.Reported, NetworkIssueStatus.Investigating],
  [NetworkIssueTab.Scheduled]: [NetworkIssueStatus.Scheduled],
  [NetworkIssueTab.Resolved]: [NetworkIssueStatus.Resolved],
}

/** DTO representing a network issue. */
export interface NetworkIssue {
  /** Network issue primary key. */
  id: number
  /** Issue title. */
  title: string
  /** Issue type. */
  type: NetworkIssueType
  /** Server name, if applicable. */
  server: string | null
  /** Priority level. */
  priority: NetworkIssuePriority
  /** Lifecycle status. */
  status: NetworkIssueStatus
  /** ISO 8601 start date. */
  startDate: string
  /** ISO 8601 end date, if any. */
  endDate: string | null
  /** Description body. */
  description: string
  /** ISO 8601 creation timestamp. */
  createdAt: string
}

/** Body that creates or replaces a network issue. */
export interface NetworkIssuePayload {
  /** Issue title. */
  title: string
  /** Issue type. */
  type: NetworkIssueType
  /** Server name, if applicable. */
  server: string | null
  /** Priority level. */
  priority: NetworkIssuePriority
  /** Lifecycle status. */
  status: NetworkIssueStatus
  /** ISO 8601 start date. */
  startDate: string
  /** ISO 8601 end date, if any. */
  endDate: string | null
  /** Description body. */
  description: string
}

/** Filter and paging of the network issue list. */
export interface NetworkIssueFilter {
  /** Page, 1-based. */
  page: number
  /** Items per page. */
  pageSize: number
  /** Tab to keep; null or omitted for all. */
  status?: NetworkIssueTab | null
}
