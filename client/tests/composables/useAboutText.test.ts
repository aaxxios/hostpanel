/**
 * Tests for `useAboutText`: the operator's story in the visitor's language, split into paragraphs.
 *
 * @module tests/composables/useAboutText.test
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { useAboutText } from '~/composables/useAboutText'

/**
 * Stubs the settings reader and the active language.
 *
 * @param values Setting key to value.
 * @param locale The visitor's language.
 */
const stub = (values: Record<string, string>, locale = 'en'): void => {
  vi.stubGlobal('usePortalSettings', () => ({ get: (key: string) => values[key] ?? '' }))
  vi.stubGlobal('useI18n', () => ({ locale: ref(locale) }))
}

describe('useAboutText', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('splits the story at blank lines and trims each paragraph', () => {
    stub({ 'portal.about.text': ' First.\n\n  \nSecond line\nstill second. ' })

    expect(useAboutText().paragraphs.value).toEqual(['First.', 'Second line\nstill second.'])
  })

  it('reads the Russian story for a Russian visitor', () => {
    stub({ 'portal.about.text': 'English.', 'portal.about.text.ru': 'Русский.' }, 'ru')

    expect(useAboutText().paragraphs.value).toEqual(['Русский.'])
  })

  it('reads the Armenian story for an Armenian visitor', () => {
    stub({ 'portal.about.text': 'English.', 'portal.about.text.hy': 'Հայերեն։' }, 'hy')

    expect(useAboutText().paragraphs.value).toEqual(['Հայերեն։'])
  })

  it('falls back to the default story when the language has none', () => {
    stub({ 'portal.about.text': 'English.' }, 'hy')

    expect(useAboutText().paragraphs.value).toEqual(['English.'])
  })

  it('is empty when no story is set', () => {
    stub({})

    expect(useAboutText().paragraphs.value).toEqual([])
  })
})
