/**
 * The availability answer for one domain from the public domain check.
 *
 * @module types/domaincheckresult
 */

import type { DomainAvailabilityStatus } from '~/types/domainavailabilitystatus'

/** What `POST /api/portal/public/domain-check` answers for one domain. */
export interface DomainCheckResult {
  /** The domain that was checked. */
  domain: string
  /** True when nobody has registered it. */
  available: boolean
  /** The same outcome as a closed value. */
  status: DomainAvailabilityStatus
}
