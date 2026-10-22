/**
 * Unit tests for LayoutFooter: the call-to-action band, the five link columns (hosting groups
 * from the catalogue), the brand block with the operator's contact details and trust marks, and
 * the bottom row with the pickers and theme toggle.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { defineComponent, h, ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import LayoutFooter from '~/components/layout/LayoutFooter.vue'
import { useCatalogStore } from '~/stores/catalog'
import type { PortalProductGroup } from '~/types/portalproductgroup'

vi.mock('~/components/AppLogoLockup.vue', () => ({
  default: defineComponent({
    name: 'AppLogoLockup',
    setup: () => () => h('span', { 'data-testid': 'app-logo-stub' }),
  }),
}))

/** Builds a stand-in for a picker the footer owns no behaviour of. */
const stubFor = (name: string, testid: string) => defineComponent({
  name,
  setup: () => () => h('div', { 'data-testid': testid }),
})

/** Contact details the stood-in `useContactDetails` answers with. */
let contact = { email: '', phone: '', phoneLink: '', address: '' }

/**
 * Mounts the footer with the given catalogue groups already in the store.
 *
 * @param groups The plan groups the catalogue holds.
 * @returns The wrapper.
 */
const mountFooter = (groups: PortalProductGroup[] = []) => {
  const pinia = createPinia()
  setActivePinia(pinia)
  const catalog = useCatalogStore(pinia)
  // Seed before mounting and keep the mount-time load off the network: the footer reads store state.
  catalog.productGroups = groups
  vi.spyOn(catalog, 'loadProductGroups').mockResolvedValue(groups)
  return mountWithKit(LayoutFooter, {
    pinia,
    components: {
      UiLanguageSwitcher: stubFor('UiLanguageSwitcher', 'footer-language'),
      UiCurrencySwitcher: stubFor('UiCurrencySwitcher', 'footer-currency'),
      UiThemeToggle: stubFor('UiThemeToggle', 'footer-theme'),
      SectionsDomainSearch: stubFor('SectionsDomainSearch', 'footer-domain-search'),
    },
  })
}

describe('LayoutFooter', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
    contact = { email: 'hello@portal.test', phone: '+1 555 010 200', phoneLink: '+1555010200', address: '' }
    vi.stubGlobal('useContactDetails', () => ({
      email: ref(contact.email),
      phone: ref(contact.phone),
      phoneLink: ref(contact.phoneLink),
      address: ref(contact.address),
    }))
  })

  it('sits on the classic footer surface with a visible top border', () => {
    const classes = mountFooter().find('footer').classes()

    expect(classes).toEqual(expect.arrayContaining(['bg-classic-footer', 'border-t', 'border-classic-border']))
  })

  it('drops the old decor: no dot headings, blobs, pill bar or back-to-top button', () => {
    const wrapper = mountFooter()

    expect(wrapper.find('[data-testid="footer-back-to-top"]').exists()).toBe(false)
    expect(wrapper.html()).not.toContain('blur-[120px]')
    expect(wrapper.html()).not.toContain('rounded-[2rem]')
    expect(wrapper.text()).not.toContain('footer.bottom.secure')
  })

  it('puts the shared domain search beside a get started link in the call to action', () => {
    const wrapper = mountFooter()

    expect(wrapper.find('[data-testid="footer-cta"] [data-testid="footer-domain-search"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="footer-cta-plans"]').attributes('href')).toBe('/en/products')
  })

  it('renders the five link columns in order', () => {
    const wrapper = mountFooter()

    const titles = wrapper.findAll('h3').map((title) => title.text())
    expect(titles).toEqual([
      'footer.columns.hosting',
      'footer.columns.domains',
      'footer.columns.company',
      'footer.columns.support',
      'footer.columns.legal',
    ])
  })

  it('lists every catalogue plan group in the hosting column, skipping empty or inactive ones', () => {
    const wrapper = mountFooter([
      { id: 1, name: 'Shared hosting', isActive: true, productCount: 3 },
      { id: 7, name: 'Cloud servers', isActive: true, productCount: 2 },
      { id: 9, name: 'Retired', isActive: false, productCount: 4 },
      { id: 10, name: 'Empty', isActive: true, productCount: 0 },
    ])

    const column = wrapper.find('[data-testid="footer-column-hosting"]')
    const labels = column.findAll('a').map((link) => link.text())
    expect(labels).toContain('Shared hosting')
    expect(labels).toContain('Cloud servers')
    expect(labels).not.toContain('Retired')
    expect(labels).not.toContain('Empty')
    expect(column.find('[data-testid="footer-hosting-all"]').attributes('href')).toBe('/en/products')
  })

  it('links the domain, company, support and legal pages', () => {
    const wrapper = mountFooter()

    /** Reads the destination of the footer link with the given test id. */
    const href = (testid: string): string | undefined => wrapper.find(`[data-testid="${testid}"]`).attributes('href')
    expect(href('footer-domains-register')).toBe('/en/domains')
    expect(href('footer-domains-transfer')).toBe('/en/domains/transfer')
    expect(href('footer-company-about')).toBe('/en/about')
    expect(href('footer-support-help')).toBe('/en/knowledgebase')
    expect(href('footer-support-faq')).toBe('/en/faq')
    expect(href('footer-legal-privacy')).toBe('/en/privacy')
    expect(href('footer-legal-cookies')).toBe('/en/cookie-policy')
    expect(href('footer-legal-terms')).toBe('/en/terms')
    expect(href('footer-legal-refund')).toBe('/en/refund-policy')
  })

  it('shows the logo lockup and the tagline in the brand block', () => {
    const wrapper = mountFooter()

    expect(wrapper.find('[data-testid="app-logo-stub"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="footer-brand"]').text()).toContain('footer.tagline')
  })

  it('offers the contact email and phone as mailto and tel text links', () => {
    const wrapper = mountFooter()

    expect(wrapper.find('[data-testid="footer-email"]').attributes('href')).toBe('mailto:hello@portal.test')
    expect(wrapper.find('[data-testid="footer-phone"]').attributes('href')).toBe('tel:+1555010200')
  })

  it('hides the contact rows that are not configured', () => {
    contact = { email: '', phone: '', phoneLink: '', address: '' }
    const wrapper = mountFooter()

    expect(wrapper.find('[data-testid="footer-email"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="footer-phone"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="footer-address"]').exists()).toBe(false)
  })

  it('shows the address when one is configured', () => {
    contact = { ...contact, address: '1 Example Street' }

    expect(mountFooter().find('[data-testid="footer-address"]').text()).toBe('1 Example Street')
  })

  it('draws generic trust marks as icons with plain labels and no images', () => {
    const trust = mountFooter().find('[data-testid="footer-trust"]')

    expect(trust.findAll('li')).toHaveLength(3)
    expect(trust.findAll('svg')).toHaveLength(3)
    expect(trust.find('img').exists()).toBe(false)
  })

  it('puts the copyright, both pickers and the theme toggle in the bottom row', () => {
    const bottom = mountFooter().find('[data-testid="footer-bottom"]')

    expect(bottom.text()).toContain(String(new Date().getFullYear()))
    expect(bottom.text()).toContain('footer.bottom.rights')
    expect(bottom.find('[data-testid="footer-language"]').exists()).toBe(true)
    expect(bottom.find('[data-testid="footer-currency"]').exists()).toBe(true)
    expect(bottom.find('[data-testid="footer-theme"]').exists()).toBe(true)
  })

  it('stacks the columns on a phone and spreads them from the small breakpoint', () => {
    const grid = mountFooter().find('[data-testid="footer-brand"]').element.parentElement!

    expect(grid.className).toContain('grid-cols-1')
    expect(grid.className).toContain('sm:grid-cols-3')
  })

  it('adds no top margin, so no gap is stacked above the footer', () => {
    const wrapper = mountFooter()

    expect(wrapper.find('footer').classes().filter((name) => /^-?m[ty]?-/.test(name))).toEqual([])
  })
})
