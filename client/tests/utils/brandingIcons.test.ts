/**
 * Unit tests for `utils/brandingIcons.ts`: which icon links a stored favicon URL becomes.
 *
 * @module tests/utils/brandingIcons.test
 */

import { describe, expect, it } from 'vitest'
import { BrandingIconRel } from '~/types/brandingiconrel'
import { brandingIcons, isUploadedBranding, toHeadIconLink } from '~/utils/brandingIcons'

describe('isUploadedBranding', () => {
  it('accepts a file inside a generated icon directory', () => {
    expect(isUploadedBranding('/uploads/branding/favicon/abc/favicon-32x32.png')).toBe(true)
  })

  it('rejects a legacy flat upload', () => {
    expect(isUploadedBranding('/uploads/branding/favicon-abc.png')).toBe(false)
  })

  it('rejects a pasted URL outside the uploads prefix', () => {
    expect(isUploadedBranding('https://cdn.example/icon.png')).toBe(false)
  })
})

describe('brandingIcons', () => {
  it('returns the built-in set when nothing is stored', () => {
    const icons = brandingIcons('')

    expect(icons).toHaveLength(9)
    expect(icons[0]!.href).toBe('/favicon.ico')
    expect(icons.find(icon => icon.type === 'image/svg+xml')!.href).toBe('/favicon.svg')
  })

  it('adds the pinned-tab silhouette only to the built-in set', () => {
    const mask = brandingIcons('').find(icon => icon.rel === BrandingIconRel.MaskIcon)

    expect(mask).toEqual({ rel: BrandingIconRel.MaskIcon, href: '/safari-pinned-tab.svg', type: '', color: '#5E8EF3' })
    expect(brandingIcons('/uploads/branding/favicon/abc/favicon-32x32.png').some(icon => icon.rel === BrandingIconRel.MaskIcon)).toBe(false)
  })

  it('returns a single link for a pasted URL', () => {
    expect(brandingIcons('https://cdn.example/icon.svg')).toEqual([
      { rel: BrandingIconRel.Icon, href: 'https://cdn.example/icon.svg', type: '' },
    ])
  })

  it('derives the generated set beside an uploaded file', () => {
    const icons = brandingIcons('/uploads/branding/favicon/abc/favicon-32x32.png')

    expect(icons).toHaveLength(5)
    expect(icons.every(icon => icon.href.startsWith('/uploads/branding/favicon/abc/'))).toBe(true)
    expect(icons.every(icon => icon.type === 'image/png')).toBe(true)
    expect(icons.find(icon => icon.rel === BrandingIconRel.AppleTouchIcon)!.href).toBe('/uploads/branding/favicon/abc/apple-touch-icon.png')
  })
})

describe('toHeadIconLink', () => {
  it('keeps a mask icon tint and drops its empty type', () => {
    expect(toHeadIconLink({ rel: BrandingIconRel.MaskIcon, href: '/m.svg', type: '', color: '#000000' }))
      .toEqual({ rel: BrandingIconRel.MaskIcon, href: '/m.svg', color: '#000000' })
  })

  it('carries type and sizes on an apple touch icon', () => {
    expect(toHeadIconLink({ rel: BrandingIconRel.AppleTouchIcon, href: '/a.png', type: 'image/png', sizes: '180x180' }))
      .toEqual({ rel: BrandingIconRel.AppleTouchIcon, href: '/a.png', type: 'image/png', sizes: '180x180' })
  })

  it('omits empty attributes on a favicon', () => {
    expect(toHeadIconLink({ rel: BrandingIconRel.Icon, href: '/f.ico', type: '' }))
      .toEqual({ rel: BrandingIconRel.Icon, href: '/f.ico' })
  })
})
