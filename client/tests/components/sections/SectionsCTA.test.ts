/**
 * Unit tests for SectionsCTA: the headline, the e-mail field and the simulated submit with its
 * confirmation.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsCTA from '~/components/sections/SectionsCTA.vue'

describe('SectionsCTA', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('shows the headline and the badge', () => {
    const wrapper = mountWithKit(SectionsCTA)

    expect(wrapper.find('h2').text()).toBe('cta.title')
    expect(wrapper.text()).toContain('cta.badge')
    expect(wrapper.text()).toContain('cta.subtitle')
  })

  it('gives the e-mail field a placeholder and an e-mail type', () => {
    const input = mountWithKit(SectionsCTA).find('[data-testid="cta-email"]')

    expect(input.attributes('placeholder')).toBe('cta.placeholder')
    expect(input.attributes('type')).toBe('email')
  })

  it('does nothing when the field is empty', async () => {
    const wrapper = mountWithKit(SectionsCTA)

    await wrapper.find('[data-testid="cta-form"]').trigger('submit')
    await vi.advanceTimersByTimeAsync(1500)

    expect(wrapper.find('[data-testid="cta-success"]').exists()).toBe(false)
  })

  it('confirms after a moment, clears the field and hides the confirmation again', async () => {
    const wrapper = mountWithKit(SectionsCTA)
    await wrapper.find('[data-testid="cta-email"]').setValue('visitor@example.test')

    await wrapper.find('[data-testid="cta-form"]').trigger('submit')
    expect(wrapper.find('[data-testid="cta-success"]').exists()).toBe(false)

    await vi.advanceTimersByTimeAsync(1000)
    expect(wrapper.find('[data-testid="cta-success"]').text()).toContain('cta.success')
    expect((wrapper.find('[data-testid="cta-email"]').element as HTMLInputElement).value).toBe('')

    await vi.advanceTimersByTimeAsync(3000)
    expect(wrapper.find('[data-testid="cta-success"]').exists()).toBe(false)
  })
})
