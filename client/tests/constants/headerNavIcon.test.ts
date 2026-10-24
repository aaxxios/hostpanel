/**
 * Unit tests for the Support dropdown icons: one literal icon per link, all of them in the
 * installed icon set.
 */
import { readFileSync } from 'node:fs'
import { createRequire } from 'node:module'
import { describe, expect, it } from 'vitest'
import { SUPPORT_LINK_ICON, SupportLinkKey } from '~/constants/headerNavIcon'

describe('SUPPORT_LINK_ICON', () => {
  it('names a distinct, full icon for every support link', () => {
    const icons = Object.values(SupportLinkKey).map((key) => SUPPORT_LINK_ICON[key])

    for (const icon of icons) {
      expect(icon).toMatch(/^mdi:[a-z0-9-]+$/)
    }
    expect(new Set(icons).size).toBe(icons.length)
  })

  it('names only icons that exist in the installed set', () => {
    const raw = readFileSync(createRequire(import.meta.url).resolve('@iconify-json/mdi/icons.json'), 'utf8')

    for (const icon of Object.values(SUPPORT_LINK_ICON)) {
      expect(raw.includes(`\n\t\t"${icon.replace(/^mdi:/, '')}": {`)).toBe(true)
    }
  })
})
