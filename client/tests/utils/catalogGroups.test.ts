import { describe, expect, it } from 'vitest'
import { HOSTING_GROUP_ID } from '~/constants/catalogGroup'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import {
  FAMILY_COLORS,
  FAMILY_ICON,
  HOSTING_FAMILY_KEY,
  familyColor,
  familyConfig,
  familyKey,
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
  isActive: true,
  productCount: 2,
  ...overrides
})


/**
 * The exact shape a operator-authored description arrives in when it was edited as
 * HTML: prose, then check-marked items separated by `<br />`. Printed straight into
 * a card it showed its own markup — "✔ 20 GB Disk Space <br /> ✔ 200 GB Bandwidth".
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
  it('accepts an active group that holds a product', () => {
    expect(isFamilyGroup(group())).toBe(true)
  })

  it('refuses an inactive group and an empty one', () => {
    expect(isFamilyGroup(group({ isActive: false }))).toBe(false)
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

describe('familyConfig', () => {
  it('derives a catalogue family from its group', () => {
    expect(familyConfig(group())).toEqual({
      name: 'Alpha Suite',
      icon: FAMILY_ICON,
      color: familyColor('alpha-suite'),
      learnMoreUrl: '/products/alpha-suite'
    })
  })

  it('points hosting at its own page', () => {
    expect(familyConfig(group({ id: HOSTING_GROUP_ID, name: 'Web Hosting' }))).toMatchObject({
      name: 'Web Hosting',
      learnMoreUrl: '/hosting'
    })
  })
})
