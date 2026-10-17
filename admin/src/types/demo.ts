/**
 * Wire shape for `GET /api/public/demo`.
 *
 * The endpoint is anonymous: the login screen needs to know whether the deployment is a
 * public demo, and which sign-in details it publishes, before anybody has a session.
 */

/** Role a published demo account signs in with. */
export enum DemoAccountRole {
  /** Back-office operator account. */
  Admin = 'Admin',
  /** Customer account. */
  Client = 'Client',
}

/** One published demo sign-in. */
export interface DemoAccount {
  /** Role the account holds. */
  role: DemoAccountRole
  /** Sign-in email. */
  email: string
  /** Sign-in password, published on purpose for the demo. */
  password: string
}

/** Body of `GET /api/public/demo`. */
export interface DemoInfo {
  /** True when this deployment runs as a public demo. */
  enabled: boolean
  /** ISO timestamp of the next data reset, or null when none is scheduled. */
  nextResetAt: string | null
  /** Published demo accounts; empty when demo mode is off. */
  accounts: DemoAccount[]
}
