/**
 * Unit tests for the classic header navigation builder: which entries exist, what the Hosting
 * dropdown lists, when VPS and Servers appear, and how the current page is matched.
 */
import { describe, expect, it } from 'vitest'
import type { HeaderNavItem } from '~/types/headernavitem'
import type { PortalProduct } from '~/types/portalproduct'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import { ProductType } from '~/types/producttype'
import { buildHeaderNav, isHeaderItemActive, isHeaderPathActive } from '~/utils/headerNav'

/** Translator that echoes the key. */
const t = (key: string): string => key

/** Locale prefixer that adds `/en`. */
const localePath = (path: string): string => `/en${path}`

/**
 * Builds an active catalogue group holding one product.
 *
 * @param id - Group id.
 * @param name - Group name.
 * @returns The group.
 */
const group = (id: number, name: string): PortalProductGroup => ({ id, name, isActive: true, productCount: 1 })

/**
 * Builds a product of a group and type.
 *
 * @param groupId - The group it belongs to.
 * @param type - Its service type.
 * @returns The product.
 */
const product = (groupId: number, type: ProductType): PortalProduct => ({ id: groupId * 10, groupId, type }) as PortalProduct

/**
 * Builds the navigation for a catalogue.
 *
 * @param groups - The family groups.
 * @param products - Their products.
 * @returns The entries.
 */
const nav = (groups: PortalProductGroup[] = [], products: PortalProduct[] = []): HeaderNavItem[] =>
  buildHeaderNav({ t, localePath, groups, products })

describe('buildHeaderNav', () => {
  it('lists hosting, domains, products and support for an empty catalogue', () => {
    expect(nav().map((item) => item.key)).toEqual(['hosting', 'domains', 'products', 'support'])
  })

  it('makes Hosting a plain link while no other plan group exists', () => {
    const hosting = nav()[0]!

    expect(hosting.to).toBe('/en/hosting')
    expect(hosting.children).toBeUndefined()
  })

  it('turns Hosting into a dropdown of the catalogue plan groups', () => {
    const hosting = nav(
      [group(1, 'Web hosting'), group(7, 'Mailboxes'), group(8, 'Certificates'), group(9, 'Cloud servers')],
      [product(1, ProductType.SharedHosting), product(7, ProductType.Email), product(8, ProductType.Ssl), product(9, ProductType.Vps)],
    )[0]!

    expect(hosting.children?.map((child) => child.label)).toEqual(['nav.allHosting', 'Mailboxes', 'Certificates'])
    expect(hosting.children?.[1]?.to).toBe('/en/products/mailboxes')
  })

  it('adds VPS and Servers only when the catalogue sells them', () => {
    const items = nav(
      [group(1, 'Web hosting'), group(9, 'Cloud servers'), group(10, 'Metal')],
      [product(9, ProductType.Vps), product(10, ProductType.Dedicated)],
    )

    expect(items.map((item) => item.key)).toEqual(['hosting', 'domains', 'vps', 'servers', 'products', 'support'])
    expect(items.find((item) => item.key === 'vps')?.to).toBe('/en/products/cloud-servers')
    expect(items.find((item) => item.key === 'servers')?.to).toBe('/en/products/metal')
  })

  it('ignores a group that is inactive or empty', () => {
    const items = nav(
      [{ ...group(9, 'Cloud servers'), isActive: false }, { ...group(10, 'Metal'), productCount: 0 }],
      [product(9, ProductType.Vps), product(10, ProductType.Dedicated)],
    )

    expect(items.map((item) => item.key)).toEqual(['hosting', 'domains', 'products', 'support'])
  })

  it('groups help, FAQ and contact under Support', () => {
    const support = nav().find((item) => item.key === 'support')!

    expect(support.children?.map((child) => child.to)).toEqual(['/en/knowledgebase', '/en/faq', '/en/contact'])
  })
})

describe('isHeaderPathActive', () => {
  it('matches the path itself and the pages below it, not a longer sibling', () => {
    expect(isHeaderPathActive('/en/hosting', '/en/hosting')).toBe(true)
    expect(isHeaderPathActive('/en/hosting/plan', '/en/hosting')).toBe(true)
    expect(isHeaderPathActive('/en/hosting-extra', '/en/hosting')).toBe(false)
    expect(isHeaderPathActive('/en/hosting', undefined)).toBe(false)
  })
})

describe('isHeaderItemActive', () => {
  it('is active when a child of the group is the current page', () => {
    const support = nav().find((item) => item.key === 'support')!

    expect(isHeaderItemActive('/en/faq', support)).toBe(true)
    expect(isHeaderItemActive('/en/domains', support)).toBe(false)
  })
})
