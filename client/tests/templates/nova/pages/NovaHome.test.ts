/**
 * Unit tests for NovaHome: the page root the header's skip link points at, and its sections.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaHome from '~/templates/nova/pages/NovaHome.vue'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

describe('NovaHome', () => {
  it('is the skip-link target and carries the nova palette', () => {
    stubTemplateGlobals()
    const wrapper = mountWithKit(NovaHome)

    expect(wrapper.attributes('id')).toBe('nova-main')
    expect(wrapper.attributes('data-testid')).toBe('nova-home')
    expect(wrapper.classes()).toEqual(expect.arrayContaining(['tpl-nova', 'bg-nova-bg']))
  })

  it('owns exactly one h1, in the hero', () => {
    stubTemplateGlobals()

    expect(mountWithKit(NovaHome).findAll('h1')).toHaveLength(1)
  })

  it('shows the empty plans state when no plans arrive', () => {
    stubTemplateGlobals()

    expect(mountWithKit(NovaHome).find('[data-testid="nova-plans-empty"]').exists()).toBe(true)
  })

  it('passes the billing choice up unchanged', async () => {
    stubTemplateGlobals()
    const wrapper = mountWithKit(NovaHome)

    await wrapper.find('[data-testid="nova-billing-yearly"]').trigger('click')

    expect(wrapper.emitted('update:yearly')).toEqual([[true]])
  })

  it('does not leak the declared domain-search props onto the root as attributes', () => {
    stubTemplateGlobals()
    const wrapper = mountWithKit(NovaHome, { props: { domainPending: true, hasZones: false } })

    expect(wrapper.attributes('domainpending')).toBeUndefined()
    expect(wrapper.attributes('haszones')).toBeUndefined()
  })
})
