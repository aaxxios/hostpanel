/**
 * Structure of the "how we work" steps on the public pages: identity, icon and order. The copy
 * (title, description) lives in the `process` locale files under `process.steps.<id>`.
 *
 * @module constants/processStep
 */

import type { ProcessStepDefinition } from '~/types/processstepdefinition'
import { ProcessStepIcon } from '~/types/processstepicon'
import { ProcessStepId } from '~/types/processstepid'

/** The process steps in display order. */
export const PROCESS_STEPS: readonly ProcessStepDefinition[] = [
  { id: ProcessStepId.Discovery, order: 1, icon: ProcessStepIcon.Discovery },
  { id: ProcessStepId.Design, order: 2, icon: ProcessStepIcon.Design },
  { id: ProcessStepId.Development, order: 3, icon: ProcessStepIcon.Development },
  { id: ProcessStepId.Launch, order: 4, icon: ProcessStepIcon.Launch },
]
