/**
 * Unit tests for UiPageHero: the title is the page's only h1, the optional parts appear only when
 * given, and the slots land under the lead sentence and above the badge.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import UiPageHero from '~/components/ui/UiPageHero.vue'

describe('UiPageHero', () => {
  it('renders the title as the h1', () => {
    const wrapper = mountWithKit(UiPageHero, { props: { title: 'Contact' } })

    const heading = wrapper.find('h1')
    expect(heading.exists()).toBe(true)
    expect(heading.text()).toBe('Contact')
  })

  it('leaves out the badge, highlight and subtitle when they are not given', () => {
    const wrapper = mountWithKit(UiPageHero, { props: { title: 'Contact' } })

    expect(wrapper.find('[data-testid="ui-page-hero-badge"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="ui-gradient-text"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="ui-page-hero-subtitle"]').exists()).toBe(false)
  })

  it('shows the badge, the highlighted word and the subtitle', () => {
    const wrapper = mountWithKit(UiPageHero, {
      props: { title: 'Build', highlight: 'Amazing', badge: 'Hello', subtitle: 'Tell us more' },
    })

    expect(wrapper.find('[data-testid="ui-page-hero-badge"]').text()).toBe('Hello')
    expect(wrapper.find('h1 [data-testid="ui-gradient-text"]').text()).toBe('Amazing')
    expect(wrapper.find('[data-testid="ui-page-hero-subtitle"]').text()).toBe('Tell us more')
  })

  it('renders the default slot after the subtitle', () => {
    const wrapper = mountWithKit(UiPageHero, {
      props: { title: 'Contact', subtitle: 'Lead' },
      slots: { default: '<span data-testid="extra">Links</span>' },
    })

    const html = wrapper.html()
    expect(wrapper.find('[data-testid="extra"]').exists()).toBe(true)
    expect(html.indexOf('Lead')).toBeLessThan(html.indexOf('Links'))
  })

  it('renders the breadcrumbs slot before the title', () => {
    const wrapper = mountWithKit(UiPageHero, {
      props: { title: 'Contact' },
      slots: { breadcrumbs: '<nav data-testid="trail">Home</nav>' },
    })

    const html = wrapper.html()
    expect(html.indexOf('Home')).toBeLessThan(html.indexOf('Contact'))
  })

  it('carries the classic-scoped overrides that flatten the band', () => {
    const wrapper = mountWithKit(UiPageHero, { props: { title: 'Contact', badge: 'Hi', highlight: 'Now' } })

    expect(wrapper.find('[data-testid="ui-page-hero"]').classes()).toContain('[.tpl-classic_&]:bg-classic-surface-2')
    expect(wrapper.find('[data-testid="ui-backdrop"]').classes()).toContain('[.tpl-classic_&]:hidden')
    expect(wrapper.find('[data-testid="ui-page-hero-badge"]').classes()).toContain('[.tpl-classic_&]:bg-classic-brand-soft')
    expect(wrapper.find('[data-testid="ui-gradient-text"]').classes()).toContain('[.tpl-classic_&]:bg-none')
  })

  it('hides its decoration from assistive technology', () => {
    const wrapper = mountWithKit(UiPageHero, { props: { title: 'Contact' } })

    expect(wrapper.find('[data-testid="ui-backdrop"]').attributes('aria-hidden')).toBe('true')
    expect(wrapper.findAll('[aria-hidden="true"]').length).toBeGreaterThanOrEqual(3)
  })

  it('never fades the header in from transparent: it slides only, and classic does not animate it', () => {
    const wrapper = mountWithKit(UiPageHero, { props: { title: 'Contact', subtitle: 'Lead' } })

    const block = wrapper.find('h1').element.parentElement!
    expect(block.className).not.toContain('animate-reveal-up')
    expect(block.className).toContain('animate-slide-up')
    expect(block.className).toContain('[.tpl-classic_&]:animate-none')
    expect(block.className).toContain('motion-reduce:animate-none')
    expect(block.getAttribute('style') ?? '').not.toContain('opacity')
  })

  it('keeps the lead sentence to a reading measure and the title and badge uncapped', () => {
    const wrapper = mountWithKit(UiPageHero, { props: { title: 'Contact', badge: 'Hi', subtitle: 'Lead' } })

    expect(wrapper.find('[data-testid="ui-page-hero-subtitle"]').classes()).toContain('max-w-[75ch]')
    expect(wrapper.find('h1').classes().some(name => name.startsWith('max-w-'))).toBe(false)
  })
})
