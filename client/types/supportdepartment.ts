/** One department a ticket can be routed to, as `GET /api/portal/public/departments` lists it. */
export interface SupportDepartment {
  /** Department id: what the new-ticket form sends back. */
  id: number
  /** Display name. */
  name: string
}
