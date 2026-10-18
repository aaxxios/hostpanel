/**
 * Helper for screen tests: drives the kit's themed select (a button that opens a listbox) the way
 * a reader does — open it, then press the option.
 */

import type { DOMWrapper, VueWrapper } from '@vue/test-utils'

/**
 * Opens the select behind a test id and picks the option with the given visible label.
 *
 * @param wrapper - The mounted screen.
 * @param testId - The `data-testid` placed on the select.
 * @param label - Visible label of the option to pick.
 * @returns Resolves once the pick has been dispatched.
 */
export const pickOption = async (wrapper: VueWrapper, testId: string, label: string): Promise<void> => {
  await wrapper.get(`[data-testid="${testId}"] button`).trigger('click')
  const options: DOMWrapper<Element>[] = wrapper.findAll(`[data-testid="${testId}"] [role="option"]`)
  const option = options.find((candidate) => candidate.text() === label)
  if (!option) {
    throw new Error(`No option "${label}" in ${testId}`)
  }
  await option.trigger('mousedown')
}

/**
 * The text a select currently shows on its trigger.
 *
 * @param wrapper - The mounted screen.
 * @param testId - The `data-testid` placed on the select.
 * @returns The trigger text.
 */
export const selectText = (wrapper: VueWrapper, testId: string): string =>
  wrapper.get(`[data-testid="${testId}"] button`).text()
