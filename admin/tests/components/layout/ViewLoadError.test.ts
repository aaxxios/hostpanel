/**
 * Mount tests for ViewLoadError: the failure sentence and a button that reloads the page.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import ViewLoadError from '@/components/layout/ViewLoadError.vue'
import { mountWithApp } from '../../helpers/mount'

describe('ViewLoadError', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('tells the operator the page could not be loaded', () => {
    const wrapper = mountWithApp(ViewLoadError)

    expect(wrapper.get('[data-testid="view-load-error"]').text()).toContain('could not be loaded')
  })

  it('reloads the page from the button', async () => {
    const reload = vi.fn()
    vi.stubGlobal('location', { ...window.location, reload })
    const wrapper = mountWithApp(ViewLoadError)

    await wrapper.get('[data-testid="view-load-error-reload"]').trigger('click')

    expect(reload).toHaveBeenCalledTimes(1)
  })
})
