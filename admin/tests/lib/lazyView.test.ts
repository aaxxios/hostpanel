/**
 * Tests for lazy screens: a view switches in at once with a skeleton and swaps to the real screen
 * when its chunk arrives, a failed download is repeated, and the loader stays reachable for prefetching.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { defineComponent, h, nextTick } from 'vue'
import { LOAD_RETRIES, RETRY_STEP_MS, lazyView, originalLoader, viewLoader, withRetry } from '@/lib/lazyView'
import { createTestI18n } from '../helpers/i18n'

/** The real screen the loader eventually delivers. */
const Real = defineComponent({ render: () => h('p', { 'data-testid': 'real' }, 'real screen') })

/**
 * Wraps a component the way a bundler's dynamic `import()` does: a module namespace whose default export it is.
 *
 * Vue only unwraps `default` from something that announces itself as a module.
 *
 * @param component - The component the module exports.
 * @returns A module-shaped object.
 */
const moduleOf = (component: typeof Real): { default: typeof Real } =>
  Object.assign(Object.create(null) as object, { [Symbol.toStringTag]: 'Module', default: component }) as { default: typeof Real }

/** Lets pending promises and the render they trigger run under fake timers. */
const settle = async (): Promise<void> => {
  await vi.advanceTimersByTimeAsync(0)
  await nextTick()
}

/**
 * Mounts a lazy view with the translations its stand-ins need.
 *
 * @param component - The async component under test.
 * @returns The mounted wrapper.
 */
const mountLazy = (component: ReturnType<typeof lazyView>) => mount(
  // Inside a host element: as the root, the wrapper would keep pointing at the stand-in it replaced.
  defineComponent({ render: () => h('div', [h(component)]) }),
  { global: { plugins: [createTestI18n()] } },
)

describe('lazyView', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
    vi.restoreAllMocks()
  })

  it('shows the skeleton straight away and the screen once its chunk arrives', async () => {
    let arrive: (module: { default: typeof Real }) => void = () => undefined
    const view = lazyView(() => new Promise((resolve) => { arrive = resolve }))

    const wrapper = mountLazy(view)
    expect(wrapper.find('[data-testid="view-loading"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="real"]').exists()).toBe(false)

    arrive(moduleOf(Real))
    await settle()

    expect(wrapper.find('[data-testid="real"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="view-loading"]').exists()).toBe(false)
  })

  it('repeats a failed download and recovers', async () => {
    const loader = vi.fn()
      .mockRejectedValueOnce(new Error('504'))
      .mockResolvedValue(moduleOf(Real))
    const wrapper = mountLazy(lazyView(loader))

    await vi.advanceTimersByTimeAsync(RETRY_STEP_MS)
    await settle()

    expect(loader).toHaveBeenCalledTimes(2)
    expect(wrapper.find('[data-testid="real"]').exists()).toBe(true)
  })

  it('shows the error state when every attempt fails', async () => {
    // Vue reports the failed load to the console even when an error component handles it.
    vi.spyOn(console, 'warn').mockImplementation(() => undefined)
    const loader = vi.fn().mockRejectedValue(new Error('offline'))
    const wrapper = mountLazy(lazyView(loader))

    await vi.advanceTimersByTimeAsync(RETRY_STEP_MS * (LOAD_RETRIES * (LOAD_RETRIES + 1)) / 2)
    await settle()

    expect(loader).toHaveBeenCalledTimes(LOAD_RETRIES + 1)
    expect(wrapper.find('[data-testid="view-load-error"]').exists()).toBe(true)
  })

  it('keeps the plain import reachable for prefetching', () => {
    const loader = vi.fn(() => Promise.resolve(moduleOf(Real)))
    const view = lazyView(loader)

    expect(viewLoader(view)).toBe(loader)
    expect(viewLoader(Real)).toBeUndefined()
  })
})

describe('withRetry', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('waits one step longer before each repeat and then gives up with the last error', async () => {
    const loader = vi.fn().mockRejectedValue(new Error('gone'))
    const result = withRetry(loader)()
    const settled = result.catch((error: Error) => error.message)

    await vi.advanceTimersByTimeAsync(RETRY_STEP_MS - 1)
    expect(loader).toHaveBeenCalledTimes(1)
    await vi.advanceTimersByTimeAsync(1)
    expect(loader).toHaveBeenCalledTimes(2)
    await vi.advanceTimersByTimeAsync(RETRY_STEP_MS * 2)

    expect(await settled).toBe('gone')
    expect(loader).toHaveBeenCalledTimes(LOAD_RETRIES + 1)
  })

  it('returns the result of a first-time success without waiting', async () => {
    const loader = vi.fn().mockResolvedValue('ok')

    expect(await withRetry(loader)()).toBe('ok')
    expect(loader).toHaveBeenCalledTimes(1)
  })

  it('exposes the wrapped import, and passes any other loader through', () => {
    const loader = vi.fn(() => Promise.resolve('x'))

    expect(originalLoader(withRetry(loader))).toBe(loader)
    expect(originalLoader(loader)).toBe(loader)
  })
})
