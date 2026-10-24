/**
 * Unit tests for the demo slide covers: one original SVG per demo slide ships under `public/demo/slides`, and
 * the backend demo seeder points each slide at one of them.
 */
import { existsSync, readFileSync, readdirSync } from 'node:fs'
import { resolve } from 'node:path'
import { describe, expect, it } from 'vitest'

/** The folder the demo slide covers ship in. */
const SLIDES_DIR = resolve(__dirname, '../../public/demo/slides')

/** The demo seeder that names each slide's cover. */
const SEEDER = resolve(__dirname, '../../../backend/src/Innovayse.Hostpanel.Infrastructure/Demo/DemoContentSeeder.cs')

/** The keys the seeder gives its slides. */
const SEEDED_KEYS = ['web-hosting', 'domains', 'cloud-vps', 'ssl', 'email', 'dedicated', 'offers']

describe('demo slide covers', () => {
  it('ships one SVG per demo slide', () => {
    expect(readdirSync(SLIDES_DIR).sort()).toEqual(SEEDED_KEYS.map((key) => `${key}.svg`).sort())
  })

  it('is referenced by the seeder for every slide, through the bundled folder', () => {
    const source = readFileSync(SEEDER, 'utf8')

    expect(source).toContain('"/demo/slides/"')
    for (const key of SEEDED_KEYS) {
      expect(source).toContain(`SlideImageUrl("${key}")`)
      expect(existsSync(resolve(SLIDES_DIR, `${key}.svg`))).toBe(true)
    }
  })

  it('is a self-contained illustration with a gradient and no external reference or brand', () => {
    for (const key of SEEDED_KEYS) {
      const svg = readFileSync(resolve(SLIDES_DIR, `${key}.svg`), 'utf8')
      expect(svg).toMatch(/viewBox="0 0 800 600"/)
      expect(svg).toContain('linearGradient')
      expect(svg).not.toMatch(/href=|<image|<text|https?:\/\/(?!www\.w3\.org)/)
    }
  })
})
