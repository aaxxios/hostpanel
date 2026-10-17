/**
 * Unit tests for the overlay helpers: the re-entrant scroll lock and the open/Escape wiring.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { defineComponent, h, nextTick, ref } from 'vue'
import { mount } from '@vue/test-utils'
import { lockBodyScroll, unlockBodyScroll, useOverlay } from '~/utils/ui/overlay'

describe('body scroll lock', () => {
  afterEach(() => {
    unlockBodyScroll()
    unlockBodyScroll()
  })

  it('locks on the first call and unlocks on the last release', () => {
    lockBodyScroll()
    lockBodyScroll()
    expect(document.body.classList.contains('overflow-hidden')).toBe(true)

    unlockBodyScroll()
    expect(document.body.classList.contains('overflow-hidden')).toBe(true)

    unlockBodyScroll()
    expect(document.body.classList.contains('overflow-hidden')).toBe(false)
  })

  it('ignores a release with no lock held', () => {
    unlockBodyScroll()

    expect(document.body.classList.contains('overflow-hidden')).toBe(false)
  })
})

describe('useOverlay', () => {
  /**
   * Mounts a host component that wires an overlay to the given state.
   *
   * @param open Reactive open flag.
   * @param close Close callback.
   * @returns The wrapper.
   */
  const host = (open: { value: boolean }, close: () => void) =>
    mount(defineComponent({
      setup() {
        useOverlay(() => open.value, close)
        return () => h('div')
      },
    }))

  it('locks scrolling while open and releases it when closed', async () => {
    const open = ref(false)
    const wrapper = host(open, vi.fn())
    expect(document.body.classList.contains('overflow-hidden')).toBe(false)

    open.value = true
    await nextTick()
    expect(document.body.classList.contains('overflow-hidden')).toBe(true)

    open.value = false
    await nextTick()
    expect(document.body.classList.contains('overflow-hidden')).toBe(false)
    wrapper.unmount()
  })

  it('calls close on Escape only while open', async () => {
    const open = ref(false)
    const close = vi.fn()
    const wrapper = host(open, close)

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }))
    expect(close).not.toHaveBeenCalled()

    open.value = true
    await nextTick()
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }))
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }))

    expect(close).toHaveBeenCalledTimes(1)
    wrapper.unmount()
  })

  it('releases the lock and the listener on unmount', async () => {
    const open = ref(true)
    const close = vi.fn()
    const wrapper = host(open, close)
    expect(document.body.classList.contains('overflow-hidden')).toBe(true)

    wrapper.unmount()
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }))

    expect(document.body.classList.contains('overflow-hidden')).toBe(false)
    expect(close).not.toHaveBeenCalled()
  })

  it('locks immediately when mounted already open', () => {
    const open = ref(true)
    const wrapper = host(open, vi.fn())

    expect(document.body.classList.contains('overflow-hidden')).toBe(true)
    wrapper.unmount()
  })
})

describe('useOverlay focus and nesting', () => {
  it('returns focus to the trigger on close and lets only the topmost overlay react to Escape', async () => {
    const trigger = document.createElement('button')
    document.body.appendChild(trigger)
    trigger.focus()
    const outerOpen = ref(true)
    const innerOpen = ref(true)
    const outerClose = vi.fn()
    const innerClose = vi.fn()
    const mountOverlay = (open: { value: boolean }, close: () => void) => mount(defineComponent({
      setup() {
        useOverlay(() => open.value, close)
        return () => h('div')
      },
    }))
    const outer = mountOverlay(outerOpen, outerClose)
    const inner = mountOverlay(innerOpen, innerClose)

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }))
    expect(innerClose).toHaveBeenCalledTimes(1)
    expect(outerClose).not.toHaveBeenCalled()

    inner.unmount()
    outer.unmount()
    await nextTick()
    expect(document.activeElement).toBe(trigger)
    trigger.remove()
  })
})
