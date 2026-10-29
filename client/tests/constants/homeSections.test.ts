/**
 * Unit tests for `constants/homeSections.ts`.
 *
 * @module tests/constants/homeSections.test
 */

import { describe, expect, it } from 'vitest'
import {
  HOME_SECTION_SCROLL_MARGIN_CLASS,
  homeSectionFromHash,
  homeSectionHash,
  homeSectionPath,
} from '~/constants/homeSections'
import { HomeSectionId } from '~/types/homesectionid'

/**
 * Prefixes a path with a locale, as `useLocalePath()` does.
 *
 * @param path Unprefixed path.
 * @returns The path under `/ru`.
 */
const ruPath = (path: string): string => `/ru${path === '/' ? '' : path}`

describe('homeSections', () => {
  it('builds the hash and the localised home address of a section', () => {
    expect(homeSectionHash(HomeSectionId.Process)).toBe('#process')
    expect(homeSectionPath((path) => path, HomeSectionId.Process)).toBe('/#process')
    expect(homeSectionPath(ruPath, HomeSectionId.Services)).toBe('/ru#services')
  })

  it('maps a route hash back to its section, and unknown hashes to nothing', () => {
    expect(homeSectionFromHash('#services')).toBe(HomeSectionId.Services)
    expect(homeSectionFromHash('#process')).toBe(HomeSectionId.Process)
    expect(homeSectionFromHash('#plans')).toBeUndefined()
    expect(homeSectionFromHash('')).toBeUndefined()
  })

  it('keeps room above a section for the header', () => {
    expect(HOME_SECTION_SCROLL_MARGIN_CLASS).toBe('scroll-mt-24')
  })
})
