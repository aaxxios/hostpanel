/**
 * Unit tests for the product-type cover map: every type names an illustration that ships in
 * `public/covers`, the files are single-colour masks, and an unknown type falls back to the generic one.
 */
import { existsSync, readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { describe, expect, it } from 'vitest'
import { DEFAULT_COVER_CLASS, PRODUCT_TYPE_COVER, productTypeCover } from '~/utils/productTypeCover'
import { ProductType } from '~/types/producttype'

/** Matches the file name inside a cover's `url(...)`. */
const COVER_FILE = /url\(\/covers\/([a-z-]+\.svg)\)/

/** The folder the illustrations ship in. */
const COVERS_DIR = resolve(__dirname, '../../public/covers')

describe('PRODUCT_TYPE_COVER', () => {
  it('names a cover for every product type', () => {
    for (const type of Object.values(ProductType)) {
      expect(PRODUCT_TYPE_COVER[type]).toMatch(COVER_FILE)
    }
  })

  it('points every cover at a file that ships with the client', () => {
    for (const classes of [...Object.values(PRODUCT_TYPE_COVER), DEFAULT_COVER_CLASS]) {
      const file = COVER_FILE.exec(classes)?.[1] ?? ''
      expect(existsSync(resolve(COVERS_DIR, file))).toBe(true)
    }
  })

  it('draws every known type with its own file, none of them the generic one', () => {
    const known = Object.values(ProductType).filter((type) => type !== ProductType.Other)
    const files = known.map((type) => COVER_FILE.exec(PRODUCT_TYPE_COVER[type])?.[1])

    expect(new Set(files).size).toBe(known.length)
    expect(files).not.toContain('generic.svg')
  })

  it('fits every illustration whole, never cropped to fill a wide cover box', () => {
    for (const classes of [...Object.values(PRODUCT_TYPE_COVER), DEFAULT_COVER_CLASS]) {
      expect(classes).toContain('[mask-size:contain]')
      expect(classes).not.toContain('[mask-size:cover]')
    }
  })

  it('draws every illustration inside its own canvas, so fitting it whole shows all of it', () => {
    for (const classes of [...Object.values(PRODUCT_TYPE_COVER), DEFAULT_COVER_CLASS]) {
      const svg = readFileSync(resolve(COVERS_DIR, COVER_FILE.exec(classes)?.[1] ?? ''), 'utf8')
      expect(svg).toMatch(/viewBox="0 0 400 160"/)
    }
  })

  it('keeps the illustrations as black shapes, so the accent can fill them through a mask', () => {
    for (const classes of [...Object.values(PRODUCT_TYPE_COVER), DEFAULT_COVER_CLASS]) {
      const svg = readFileSync(resolve(COVERS_DIR, COVER_FILE.exec(classes)?.[1] ?? ''), 'utf8')
      expect(svg).not.toMatch(/#(?!000\b)[0-9a-f]{3,6}\b/i)
    }
  })
})

describe('productTypeCover', () => {
  it('returns the type cover, and the generic one for a missing or unknown type', () => {
    expect(productTypeCover(ProductType.Vps)).toBe(PRODUCT_TYPE_COVER[ProductType.Vps])
    expect(productTypeCover('Mystery')).toBe(DEFAULT_COVER_CLASS)
    expect(productTypeCover(undefined)).toBe(DEFAULT_COVER_CLASS)
    expect(productTypeCover(null)).toBe(DEFAULT_COVER_CLASS)
  })
})
