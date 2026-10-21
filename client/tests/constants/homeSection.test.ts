/**
 * Unit tests for `constants/homeSection.ts`: every section has a switch and a default, and only
 * the portfolio projects are off until the operator turns them on.
 *
 * @module tests/constants/homeSection.test
 */

import { describe, expect, it } from 'vitest'
import { HOME_SECTION_DEFAULT, HOME_SECTION_SETTING, HomeSection } from '~/constants/homeSection'

/** Every section the enum declares. */
const sections = Object.values(HomeSection)

describe('HomeSection', () => {
  it('has unique values', () => {
    expect(new Set(sections).size).toBe(sections.length)
  })

  it('maps every section to a portal.sections.* setting named after it', () => {
    for (const section of sections) {
      expect(HOME_SECTION_SETTING[section]).toBe(`portal.sections.${section}`)
    }
  })

  it('has a default for every section, all on except projects', () => {
    for (const section of sections) {
      expect(HOME_SECTION_DEFAULT[section]).toBe(section !== HomeSection.Projects)
    }
  })
})
