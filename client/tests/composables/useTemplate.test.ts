/**
 * Tests for `useTemplate`: the operator's template choice resolves a slot loader from the
 * registry, and an unknown name falls back to the default template.
 *
 * @module tests/composables/useTemplate.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { templates } from '~/templates/registry'
import { DEFAULT_TEMPLATE } from '~/templates/types'
import { TemplateName } from '~/types/templatename'
import { TemplateSlot } from '~/types/templateslot'
import { useTemplate } from '~/composables/useTemplate'

/**
 * Stubs the settings reader and `defineAsyncComponent`, which records the loader it was given.
 *
 * @param template The `portal.template` setting value.
 */
const stubRuntime = (template: string): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => (key === 'portal.template' ? template : '') }))
  vi.stubGlobal('defineAsyncComponent', (loader: unknown) => ({ loader }))
}

describe('useTemplate', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('names the template the operator chose', () => {
    stubRuntime(TemplateName.Nova)

    expect(useTemplate().name.value).toBe(TemplateName.Nova)
  })

  it.each(['', 'no-such-template'])('falls back to the default for %j', (value) => {
    stubRuntime(value)

    expect(useTemplate().name.value).toBe(DEFAULT_TEMPLATE)
  })

  it('resolves a slot through the active template loader', () => {
    stubRuntime(TemplateName.Nova)

    const component = useTemplate().slot(TemplateSlot.Header) as unknown as { loader: unknown }

    expect(component.loader).toBe(templates[TemplateName.Nova][TemplateSlot.Header])
  })

  it('resolves slots of the default template when the name is unknown', () => {
    stubRuntime('typo')

    const component = useTemplate().slot(TemplateSlot.Home) as unknown as { loader: unknown }

    expect(component.loader).toBe(templates[DEFAULT_TEMPLATE][TemplateSlot.Home])
  })
})
