/**
 * A selectable contact entry for the "Choose Contact" dropdown on a domain's contact tab.
 *
 * @module types/domaincontactchoice
 */

import type { DomainContactFields } from '~/types/domaincontactfields'

/** A selectable contact entry for the "Choose Contact" dropdown on a domain's contact tab. */
export interface DomainContactChoice {
  /** Contact id. */
  id: string
  /** Name shown in the dropdown. */
  label: string
  /** The fields the choice pre-fills. */
  fields: Partial<DomainContactFields>
}
