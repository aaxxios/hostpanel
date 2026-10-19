/**
 * Tests for the store-side translator: the same catalogue the components read, resolved per
 * call so a language switch is honoured, with a raw-value fallback for unknown identifiers.
 */

import { afterEach, describe, expect, it } from 'vitest'
import { setLocale } from '@/i18n'
import { MessageLanguage } from '@/locales/messages'
import { translate, translateOrRaw } from '@/lib/translate'

describe('translate', () => {
  afterEach(() => {
    setLocale(MessageLanguage.En)
  })

  it('resolves a key in the active language', () => {
    expect(translate('errors.noAnswer')).toBe('Could not reach the server. Check your connection and try again.')
  })

  it('follows a language switch made after the module was loaded', () => {
    setLocale(MessageLanguage.Ru)

    expect(translate('errors.noAnswer')).toBe('Не удалось связаться с сервером. Проверьте соединение и повторите попытку.')

    setLocale(MessageLanguage.Hy)

    expect(translate('errors.noAnswer')).toBe('Չհաջողվեց կապվել սերվերի հետ։ Ստուգեք կապը և փորձեք կրկին։')
  })

  it('fills named placeholders', () => {
    expect(translate('quotes.convertedSuccess', { id: 42 })).toBe('Quote converted to invoice #42 successfully.')
  })
})

describe('translateOrRaw', () => {
  afterEach(() => {
    setLocale(MessageLanguage.En)
  })

  it('translates a known identifier', () => {
    setLocale(MessageLanguage.Ru)

    expect(translateOrRaw('ticketStatus.AwaitingReply', 'AwaitingReply')).toBe('Ожидает ответа')
  })

  it('returns the raw value when the catalogue has no such key', () => {
    expect(translateOrRaw('ticketStatus.SomethingNew', 'SomethingNew')).toBe('SomethingNew')
  })
})
