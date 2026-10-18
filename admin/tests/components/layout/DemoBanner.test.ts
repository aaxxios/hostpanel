/**
 * Mount tests for DemoBanner: shown only in demo mode, with the label, the countdown and the reset note.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { mount } from '@vue/test-utils'
import DemoBanner from '@/components/layout/DemoBanner.vue'
import { useDemoStore } from '@/stores/demo'
import { createTestI18n } from '../../helpers/i18n'

afterEach(() => {
  vi.useRealTimers()
})

/**
 * Mounts the banner with the given demo state.
 *
 * @param enabled - Whether demo mode is on.
 * @param nextResetAt - The next reset timestamp, or null.
 * @returns The wrapper.
 */
const mountBanner = (enabled: boolean, nextResetAt: string | null) => {
  const pinia = createPinia()
  setActivePinia(pinia)
  const store = useDemoStore()
  store.enabled = enabled
  store.nextResetAt = nextResetAt
  return mount(DemoBanner, { global: { plugins: [createTestI18n(), pinia] } })
}

describe('DemoBanner', () => {
  it('stays hidden when demo mode is off', () => {
    expect(mountBanner(false, null).find('[data-testid="demo-banner"]').exists()).toBe(false)
  })

  it('shows the label, the countdown and the reset note', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2030-01-01T00:00:00Z'))
    const wrapper = mountBanner(true, '2030-01-01T01:05:00Z')

    const text = wrapper.get('[data-testid="demo-banner"]').text()
    expect(text).toContain('Demo mode')
    expect(wrapper.get('[data-testid="demo-banner-countdown"]').text()).toContain('1h 5m')
    expect(text).toContain('reset on a schedule')
  })

  it('omits the countdown when no reset is scheduled', () => {
    const wrapper = mountBanner(true, null)

    expect(wrapper.find('[data-testid="demo-banner-countdown"]').exists()).toBe(false)
  })

  it('reloads the demo description when the countdown reaches zero', async () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2030-01-01T00:00:00Z'))
    const wrapper = mountBanner(true, '2030-01-01T00:01:00Z')
    const load = vi.spyOn(useDemoStore(), 'load').mockResolvedValue()

    await vi.advanceTimersByTimeAsync(60_000)
    expect(load).toHaveBeenCalledWith(true)
    wrapper.unmount()
  })
})
