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

  it('hides its decoration from assistive technology', () => {
    const wrapper = mountWithKit(UiPageHero, { props: { title: 'Contact' } })

    expect(wrapper.find('[data-testid="ui-backdrop"]').attributes('aria-hidden')).toBe('true')
    expect(wrapper.findAll('[aria-hidden="true"]').length).toBeGreaterThanOrEqual(3)
  })
})
