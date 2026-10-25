/**
 * Unit tests for `isPartnersVisible`: the one partners section is shown while the Partners or
 * the Trusted by switch is on, and hidden only when both are off.
 *
 * @module tests/utils/partnersVisible.test
 */
import { describe, expect, it } from 'vitest'
import { HomeSection } from '~/constants/homeSection'
import { isPartnersVisible } from '~/utils/partnersVisible'

/**
 * Builds a switch reader that answers off for the given sections and on for the rest.
 *
 * @param off - The sections that are switched off.
 * @returns The reader.
 */
const switches = (off: HomeSection[]) => (section: HomeSection): boolean => !off.includes(section)

describe('isPartnersVisible', () => {
  it.each([
    [[], true],
    [[HomeSection.Partners], true],
    [[HomeSection.Trusted], true],
    [[HomeSection.Partners, HomeSection.Trusted], false],
  ] as [HomeSection[], boolean][])('with %j off the section is shown: %s', (off, shown) => {
    expect(isPartnersVisible(switches(off))).toBe(shown)
  })
})
