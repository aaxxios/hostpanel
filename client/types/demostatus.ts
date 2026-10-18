/**
 * The demo status type, as the public demo endpoint sends it.
 *
 * @module types/demostatus
 */

import type { DemoAccount } from '~/types/demoaccount'

/**
 * Whether this deployment runs as a public demo, as `GET /api/public/demo` sends it.
 */
export interface DemoStatus {
  /** True when the deployment is a demo; false for a normal installation. */
  enabled: boolean
  /** ISO timestamp of the next data reset, or null when none is scheduled. */
  nextResetAt: string | null
  /** The published demo accounts; empty when the demo is off. */
  accounts: DemoAccount[]
}
