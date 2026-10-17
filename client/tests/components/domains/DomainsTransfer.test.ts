/**
 * Unit tests for DomainsTransfer: the two fields are v-models, the button reports the submit and the
 * tabs link between registering and transferring.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import DomainsTransfer from '~/components/domains/DomainsTransfer.vue'

/**
 * Mounts the screen.
 *
 * @param domain The domain field's value.
 * @param epp The authorization code field's value.
 * @returns The wrapper.
 */
const mountTransfer = (domain = '', epp = '') => mountWithKit(DomainsTransfer, { props: { domain, epp } })

describe('DomainsTransfer', () => {
  beforeEach(() => {
    stubPublicPageRuntime()
  })

  it('shows the two fields with their values and placeholders', () => {
    const wrapper = mountTransfer('example.com', 'secret')

    const domain = wrapper.find('[data-testid="domains-transfer-domain"]')
    const epp = wrapper.find('[data-testid="domains-transfer-epp"]')
    expect((domain.element as HTMLInputElement).value).toBe('example.com')
    expect(domain.attributes('placeholder')).toBe('domains.transferDomainPlaceholder')
    expect((epp.element as HTMLInputElement).value).toBe('secret')
    expect(epp.attributes('placeholder')).toBe('domains.transferEppPlaceholder')
  })

  it('reports what the visitor types into each field', async () => {
    const wrapper = mountTransfer()

    await wrapper.find('[data-testid="domains-transfer-domain"]').setValue('shop.test')
    await wrapper.find('[data-testid="domains-transfer-epp"]').setValue('abc123')

    expect(wrapper.emitted('update:domain')?.[0]).toEqual(['shop.test'])
    expect(wrapper.emitted('update:epp')?.[0]).toEqual(['abc123'])
  })

  it('reports the submit', async () => {
    const wrapper = mountTransfer('example.com')

    await wrapper.find('[data-testid="domains-transfer-submit"]').trigger('click')

    expect(wrapper.emitted('submit')).toHaveLength(1)
  })

  it('links the register tab and the search card back to the domains page', () => {
    const wrapper = mountTransfer()

    expect(wrapper.find('[data-testid="domains-tab-register"]').attributes('href')).toBe('/en/domains')
    expect(wrapper.find('[data-testid="domains-transfer-search"]').attributes('href')).toBe('/en/domains')
    expect(wrapper.find('[data-testid="domains-transfer-crumb"]').attributes('href')).toBe('/en/domains')
  })

  it('explains the authorization code in a tooltip', () => {
    expect(mountTransfer().find('[data-testid="domains-transfer-help"]').exists()).toBe(true)
  })
})
