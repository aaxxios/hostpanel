/**
 * Unit tests for AuroraHome: every section is composed in, the optional ones can be switched off,
 * and the search and billing events travel up to the page that owns the data.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraHome from '~/templates/aurora/pages/AuroraHome.vue'
import { stubAuroraGlobals, PLANS } from '../auroraGlobals'

describe('AuroraHome', () => {
  it('composes the hero, search, trust bar, plans, services, process, testimonials, FAQ and call to action', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHome, { props: { plans: PLANS } })

    expect(wrapper.attributes('data-testid')).toBe('aurora-home')
    expect(wrapper.find('[data-testid="hero-title"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="home-domain-search"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="trust-bar"]').exists()).toBe(true)
    expect(wrapper.find('#hosting').exists()).toBe(true)
    expect(wrapper.find('#services').exists()).toBe(true)
    expect(wrapper.find('#process').exists()).toBe(true)
    expect(wrapper.find('#faq').exists()).toBe(true)
    expect(wrapper.find('[data-testid="aurora-cta"]').exists()).toBe(true)
  })

  it('leaves out the process and FAQ sections when they are switched off', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHome, { props: { showProcess: false, showFaq: false } })

    expect(wrapper.find('#process').exists()).toBe(false)
    expect(wrapper.find('#faq').exists()).toBe(false)
  })

  it('passes the search term of the card up to the page', async () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHome)

    await wrapper.find('[data-testid="home-domain-search-input"]').setValue('acme')
    await wrapper.find('[data-testid="home-domain-search"] form').trigger('submit')

    expect(wrapper.emitted('search')).toEqual([['acme']])
  })

  it('passes the billing period up to the page', async () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHome, { props: { plans: PLANS } })

    await wrapper.find('[data-testid="billing-yearly"]').trigger('click')

    expect(wrapper.emitted('update:yearly')).toEqual([[true]])
  })

  it('spans the full width with the aurora backdrop and keeps the content in the shared page container', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraHome)

    expect(wrapper.classes()).not.toContain('container-custom')
    expect(wrapper.find('.container-custom').exists()).toBe(true)
    expect(wrapper.find('[data-testid="ui-backdrop"]').attributes('data-variant')).toBe('aurora')
  })
})
