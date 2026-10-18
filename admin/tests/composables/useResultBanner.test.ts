/**
 * Tests for the result banner: it shows a sentence, hides it after the delay, replaces an earlier
 * one without the old timer cutting the new one short, and cancels its timer on unmount.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, h } from 'vue'
import { mount } from '@vue/test-utils'
import { RESULT_BANNER_MS, useResultBanner } from '@/composables/useResultBanner'
import type { ResultBanner } from '@/composables/useResultBanner'

/**
 * Mounts a throwaway component that owns a banner, so the unmount hook is real.
 *
 * @returns The banner and the wrapper.
 */
const mountBanner = () => {
  let banner: ResultBanner | undefined
  const wrapper = mount(defineComponent({
    setup: () => {
      banner = useResultBanner()
      return () => h('div')
    },
  }))
  return { banner: banner as ResultBanner, wrapper }
}

describe('useResultBanner', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('shows a sentence with its error flag and hides it after the delay', () => {
    const { banner } = mountBanner()

    banner.show('Rates updated', false)
    expect(banner.message.value).toBe('Rates updated')
    expect(banner.isError.value).toBe(false)

    vi.advanceTimersByTime(RESULT_BANNER_MS)
    expect(banner.message.value).toBeNull()
  })

  it('flags a failure', () => {
    const { banner } = mountBanner()

    banner.show('Could not reach the server', true)

    expect(banner.isError.value).toBe(true)
  })

  it('a newer sentence replaces the older one and restarts the delay', () => {
    const { banner } = mountBanner()

    banner.show('first', false)
    vi.advanceTimersByTime(RESULT_BANNER_MS - 1000)
    banner.show('second', true)
    vi.advanceTimersByTime(RESULT_BANNER_MS - 1000)

    expect(banner.message.value).toBe('second')
    vi.advanceTimersByTime(1000)
    expect(banner.message.value).toBeNull()
  })

  it('clear hides the banner at once', () => {
    const { banner } = mountBanner()

    banner.show('x', true)
    banner.clear()

    expect(banner.message.value).toBeNull()
    expect(banner.isError.value).toBe(false)
  })

  it('drops its pending timer when the screen unmounts', () => {
    const { banner, wrapper } = mountBanner()
    banner.show('pending', false)
    expect(vi.getTimerCount()).toBe(1)

    wrapper.unmount()

    expect(vi.getTimerCount()).toBe(0)
  })
})
