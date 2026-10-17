/**
 * Tests for the i18n lint logic: missing, extra, unused and unknown-literal keys, plus dynamic
 * key prefixes counting as use.
 */
import { describe, expect, it } from 'vitest'
import { checkI18n, flattenKeys, type Catalogues } from '../../scripts/lib/i18nCheck'

/**
 * Builds catalogues from key lists.
 *
 * @param en - English keys.
 * @param ru - Russian keys.
 * @param hy - Armenian keys.
 * @returns The catalogues.
 */
const make = (en: string[], ru: string[], hy: string[]): Catalogues => ({
  en: new Set(en),
  ru: new Set(ru),
  hy: new Set(hy),
})

describe('flattenKeys', () => {
  it('flattens nested objects with a prefix', () => {
    expect(flattenKeys({ a: { b: 'x', c: 'y' } }, 'area')).toEqual(['area.a.b', 'area.a.c'])
  })
})

describe('checkI18n', () => {
  it('passes for complete, used catalogues', () => {
    const keys = ['common.save']
    expect(checkI18n({ catalogues: make(keys, keys, keys), source: "t('common.save')" })).toEqual([])
  })

  it('reports a key missing in a language and an extra key', () => {
    const problems = checkI18n({
      catalogues: make(['a.x'], ['a.y'], ['a.x']),
      source: "t('a.x') t('a.y')",
    })
    expect(problems).toContain('missing in ru: a.x')
    expect(problems).toContain('extra in ru (absent from en): a.y')
  })

  it('reports an unused key', () => {
    const problems = checkI18n({ catalogues: make(['a.x'], ['a.x'], ['a.x']), source: '' })
    expect(problems).toContain('unused key: a.x')
  })

  it('reports a literal t() key absent from en', () => {
    const problems = checkI18n({ catalogues: make([], [], []), source: "t('nope.key')" })
    expect(problems).toContain("t('nope.key') is used but absent from en")
  })

  it('counts a dynamic prefix as use of every key under it', () => {
    const keys = ['nav.home', 'nav.about']
    const source = 't(`nav.${name}`)'
    expect(checkI18n({ catalogues: make(keys, keys, keys), source })).toEqual([])
  })

  it('reads keys passed to translate() like t()', () => {
    const keys = ['calendar.months.1', 'common.save']
    const source = "translate('common.save') translate(`calendar.months.${i}`)"
    expect(checkI18n({ catalogues: make(keys, keys, keys), source })).toEqual([])
  })
})
