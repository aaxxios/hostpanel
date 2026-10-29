import { describe, expect, it } from 'vitest'
import { HOSTING_GROUP_ID } from '~/constants/catalogGroup'
import type { PortalProduct } from '~/types/portalproduct'
import { ProductType } from '~/types/producttype'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import {
  FAMILY_COLORS,
  FAMILY_ICON,
  HOSTING_FAMILY_KEY,
  familyColor,
  familyConfig,
  dominantProductType,
  familyKey,
  isDomainGroup,
  isFamilyGroup,
  nameToKey,
  parseDescription
} from '~/utils/catalogGroups'

/**
 * Builds a catalogue group.
 *
 * @param overrides Fields that differ from the defaults.
 * @returns The group.
 */
const group = (overrides: Partial<PortalProductGroup> = {}): PortalProductGroup => ({
  id: 3,
  name: 'Alpha Suite',
  description: null,
  productCount: 2,
  ...overrides
})


/**
 * The exact shape a operator-authored description arrives in when it was edited as
 * HTML: prose, then check-marked items separated by `<br />`. Printed straight into
 * a card it showed its own markup: "✔ 20 GB Disk Space <br /> ✔ 200 GB Bandwidth".
 */
const HTML_DESCRIPTION =
  'The Business Hosting plan provides the optimal performance for medium-sized '
  + 'enterprises and online stores. It includes everything you need to run a fast, '
  + 'secure, and professional website. <br /> ✔ 20 GB Disk Space <br /> ✔ 200 GB '
  + 'Bandwidth <br /> ✔ 50 Email Accounts <br /> ✔ 20 MySQL Databases <br /> '
  + '✔ 20 Subdomains <br /> ✔ 20 FTP Accounts <br /> ✔ Free SSL Certificate <br />'

describe('parseDescription', () => {
  it('separates the prose from the check-marked items in an HTML description', () => {
    const { summary, features } = parseDescription(HTML_DESCRIPTION)

    expect(summary).toBe(
      'The Business Hosting plan provides the optimal performance for medium-sized '
      + 'enterprises and online stores. It includes everything you need to run a fast, '
      + 'secure, and professional website.')
    expect(features).toEqual([
      '20 GB Disk Space',
      '200 GB Bandwidth',
      '50 Email Accounts',
      '20 MySQL Databases',
      '20 Subdomains',
      '20 FTP Accounts',
      'Free SSL Certificate',
    ])
  })

  it('leaves no markup anywhere in the result', () => {
    const { summary, features } = parseDescription(HTML_DESCRIPTION)

    for (const text of [summary, ...features]) {
      expect(text).not.toMatch(/<[^>]+>/)
      expect(text).not.toContain('&')
    }
  })

  it('handles a description that is nothing but items, with no prose', () => {
    const { summary, features } = parseDescription(
      '✔ 600 MB Disk Space <br /> ✔ 5 GB Bandwidth <br /> ✔ 2 Email Accounts')

    expect(summary).toBe('')
    expect(features).toEqual(['600 MB Disk Space', '5 GB Bandwidth', '2 Email Accounts'])
  })

  it('still reads a plain-text description, which the catalogue also holds', () => {
    const { summary, features } = parseDescription(
      'A simple plan.\n\n• 10 GB Disk\n• 1 TB Bandwidth')

    expect(summary).toBe('A simple plan.')
    expect(features).toEqual(['10 GB Disk', '1 TB Bandwidth'])
  })

  it('reports an empty description as empty rather than as one blank feature', () => {
    expect(parseDescription('')).toEqual({ summary: '', features: [] })
  })
})

describe('nameToKey', () => {
  it('lower-cases a name, joins its words with dashes and drops other characters', () => {
    expect(nameToKey('Alpha Suite')).toBe('alpha-suite')
    expect(nameToKey('Beta  Kit (Pro)!')).toBe('beta-kit-pro')
  })
})

describe('isFamilyGroup', () => {
  it('accepts a group that holds a product', () => {
    expect(isFamilyGroup(group())).toBe(true)
  })

  it('refuses an empty group', () => {
    expect(isFamilyGroup(group({ productCount: 0 }))).toBe(false)
  })
})

describe('familyKey', () => {
  it('names a family by its group name', () => {
    expect(familyKey(group())).toBe('alpha-suite')
  })

  it('gives hosting its fixed key whatever the operator called the group', () => {
    expect(familyKey(group({ id: HOSTING_GROUP_ID, name: 'Anything' }))).toBe(HOSTING_FAMILY_KEY)
  })

  it('falls back to the group id when the name leaves no letters', () => {
    expect(familyKey(group({ id: 8, name: '!!!' }))).toBe('group-8')
  })
})

describe('familyColor', () => {
  it('is the same colour for the same key, taken from the palette', () => {
    expect(familyColor('alpha-suite')).toBe(familyColor('alpha-suite'))
    expect(FAMILY_COLORS).toContain(familyColor('alpha-suite'))
  })
})

/**
 * Builds a catalogue product of a type.
 *
 * @param type The product type as the API sends it.
 * @returns The product.
 */
const typed = (type: ProductType | undefined): PortalProduct => ({ id: 1, groupId: 3, name: 'Plan', type })

describe('dominantProductType', () => {
  it('is the type most products have, the first met winning a tie', () => {
    expect(dominantProductType([typed(ProductType.Vps), typed(ProductType.Ssl), typed(ProductType.Ssl)])).toBe(ProductType.Ssl)
    expect(dominantProductType([typed(ProductType.Vps), typed(ProductType.Ssl)])).toBe(ProductType.Vps)
  })

  it('is null when no product has a type', () => {
    expect(dominantProductType([])).toBeNull()
    expect(dominantProductType([typed(undefined)])).toBeNull()
  })
})

describe('isDomainGroup', () => {
  it('is true only for a group that sells domain names', () => {
    expect(isDomainGroup([typed(ProductType.Domain), typed(ProductType.Domain)])).toBe(true)
    expect(isDomainGroup([typed(ProductType.SharedHosting)])).toBe(false)
  })
})

describe('familyConfig', () => {
  it('derives a catalogue family from its group', () => {
    expect(familyConfig(group())).toEqual({
      name: 'Alpha Suite',
      icon: FAMILY_ICON,
      color: familyColor('alpha-suite'),
      learnMoreUrl: '/products/alpha-suite'
    })
  })

  it('takes the icon from the type of the group products, and the default when there are none', () => {
    expect(familyConfig(group(), [typed(ProductType.Vps)]).icon).toBe('mdi:server-network')
    expect(familyConfig(group(), [typed(ProductType.Ssl), typed(ProductType.Ssl), typed(ProductType.Vps)]).icon).toBe('mdi:shield-lock')
    expect(familyConfig(group(), [typed(undefined)]).icon).toBe(FAMILY_ICON)
  })

  it('gives hosting the hosting icon whatever its products say', () => {
    expect(familyConfig(group({ id: HOSTING_GROUP_ID, name: 'Web Hosting' }), [typed(ProductType.Other)]).icon).toBe('mdi:cloud-check')
  })

  it('points hosting at its own page', () => {
    expect(familyConfig(group({ id: HOSTING_GROUP_ID, name: 'Web Hosting' }))).toMatchObject({
      name: 'Web Hosting',
      learnMoreUrl: '/hosting'
    })
  })
})
