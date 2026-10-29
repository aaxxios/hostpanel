/**
 * Unit tests for UiErrorState: the sentence in a danger alert, the prefixed test ids and the
 * retry event.
 */
import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'
import UiErrorState from '@/components/ui/UiErrorState.vue'
import { createTestI18n } from '../../helpers/i18n'

/**
 * Mounts the state with a sentence and a test id prefix.
 *
 * @returns The wrapper.
 */
const mountState = () =>
  mount(UiErrorState, {
    props: { message: 'No access.', testid: 'report-error' },
    global: { plugins: [createTestI18n()] },
  })

describe('UiErrorState', () => {
  it('shows the sentence alone in a danger alert', () => {
    const wrapper = mountState()

    const alert = wrapper.get('[data-testid="report-error"]')
    expect(alert.text()).toBe('No access.')
    expect(alert.attributes('role')).toBe('alert')
  })

  it('asks to load again from the retry button', async () => {
    const wrapper = mountState()

    const retry = wrapper.get('[data-testid="report-error-retry"]')
    expect(retry.text()).toBe('Retry')
    await retry.trigger('click')

    expect(wrapper.emitted('retry')).toHaveLength(1)
  })
})
