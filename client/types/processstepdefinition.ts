/**
 * The structure of one process step.
 *
 * @module types/processstepdefinition
 */

import type { ProcessStepIcon } from '~/types/processstepicon'
import type { ProcessStepId } from '~/types/processstepid'

/** The structure of one process step. */
export interface ProcessStepDefinition {
  /** Step identifier, also the i18n key segment. */
  id: ProcessStepId
  /** Icon shown above the step. */
  icon: ProcessStepIcon
  /** One-based position in the process. */
  order: number
}
