/**
 * Unit tests for the home sections: every section has a settings key under `portal.sections.`,
 * a default, and only the portfolio is off by default.
 */
import { describe, expect, it } from 'vitest'
import { HOME_SECTION_DEFAULT, HOME_SECTION_SETTING, HomeSection } from '@/types/homesection'

describe('HomeSection', () => {
  it('names twenty sections', () => {
    expect(Object.values(HomeSection)).toHaveLength(20)
  })

  it('maps every section to a distinct portal.sections key ending in its own name', () => {
    const keys = Object.values(HomeSection).map((section) => HOME_SECTION_SETTING[section])

    expect(new Set(keys).size).toBe(keys.length)
    for (const section of Object.values(HomeSection)) {
      expect(HOME_SECTION_SETTING[section]).toBe(`portal.sections.${section}`)
    }
  })

  it('turns only the portfolio off by default', () => {
    const off = Object.values(HomeSection).filter((section) => !HOME_SECTION_DEFAULT[section])

    expect(off).toEqual([HomeSection.Projects])
  })
})
