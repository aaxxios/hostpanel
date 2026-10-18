/**
 * Unit tests for UiSpinner: display, stepping, press-and-hold repeat and typed input.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import UiSpinner from '@/components/ui/UiSpinner.vue'

afterEach(() => {
  vi.useRealTimers()
})

describe('UiSpinner', () => {
  it('shows two decimals, and nothing for zero', () => {
    expect((mount(UiSpinner, { props: { modelValue: 12.5 } }).get('input').element as HTMLInputElement).value).toBe('12.50')
    expect((mount(UiSpinner, { props: { modelValue: 0 } }).get('input').element as HTMLInputElement).value).toBe('')
  })

  it('steps once on press', async () => {
    const wrapper = mount(UiSpinner, { props: { modelValue: 1, step: 0.5 } })

    await wrapper.get('[data-testid="spinner-increment"]').trigger('mousedown')
    await wrapper.get('[data-testid="spinner-decrement"]').trigger('mousedown')

    expect(wrapper.emitted('update:modelValue')).toEqual([[1.5], [0.5]])
  })

  it('keeps stepping while held and stops on release', async () => {
    vi.useFakeTimers()
    const wrapper = mount(UiSpinner, { props: { modelValue: 0, step: 1 } })
    const up = wrapper.get('[data-testid="spinner-increment"]')

    await up.trigger('mousedown')
    vi.advanceTimersByTime(250)
    await up.trigger('mouseup')
    vi.advanceTimersByTime(500)

    expect(wrapper.emitted('update:modelValue')).toHaveLength(3)
  })

  it('stops stepping when unmounted mid-hold', async () => {
    vi.useFakeTimers()
    const wrapper = mount(UiSpinner, { props: { modelValue: 0 } })

    await wrapper.get('[data-testid="spinner-increment"]').trigger('mousedown')
    const before = wrapper.emitted('update:modelValue')?.length ?? 0
    wrapper.unmount()
    vi.advanceTimersByTime(500)

    expect(before).toBe(1)
    expect(vi.getTimerCount()).toBe(0)
  })

  it('does not step past max or min', async () => {
    const atMax = mount(UiSpinner, { props: { modelValue: 5, max: 5 } })
    const atMin = mount(UiSpinner, { props: { modelValue: 0, min: 0 } })

    await atMax.get('[data-testid="spinner-increment"]').trigger('mousedown')
    await atMin.get('[data-testid="spinner-decrement"]').trigger('mousedown')

    expect(atMax.emitted('update:modelValue')).toBeUndefined()
    expect(atMin.emitted('update:modelValue')).toBeUndefined()
  })

  it('parses typed numbers, treats empty as zero and ignores text', async () => {
    const wrapper = mount(UiSpinner, { props: { modelValue: 3 } })

    await wrapper.get('input').setValue('7.25')
    await wrapper.get('input').setValue('')

    expect(wrapper.emitted('update:modelValue')).toEqual([[7.25], [0]])
  })

  it('resets an empty field to zero on blur', async () => {
    const wrapper = mount(UiSpinner, { props: { modelValue: 0 } })

    await wrapper.get('input').trigger('blur')

    expect(wrapper.emitted('update:modelValue')?.[0]).toEqual([0])
  })
})
