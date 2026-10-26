/**
 * Unit tests for the phone country helpers: the country list, the search, and the conversion
 * between the stored value and the picker/number pair.
 */
import { describe, expect, it } from 'vitest'
import {
  DEFAULT_PHONE_COUNTRY,
  composePhoneValue,
  dialCodeOf,
  filterPhoneCountries,
  listPhoneCountries,
  splitPhoneValue,
} from '~/utils/phoneCountries'

describe('dialCodeOf', () => {
  it('returns the calling code with a plus sign', () => {
    expect(dialCodeOf('AM')).toBe('+374')
    expect(dialCodeOf('US')).toBe('+1')
  })

  it('returns an empty string for an unknown code', () => {
    expect(dialCodeOf('ZZ')).toBe('')
  })
})

describe('listPhoneCountries', () => {
  it('names countries in the requested language, sorted by name', () => {
    const list = listPhoneCountries('en')
    const names = list.map((country) => country.name)

    expect(list.find((country) => country.iso === 'AM')).toMatchObject({ name: 'Armenia', dialCode: '+374' })
    expect(names).toEqual([...names].sort((a, b) => a.localeCompare(b, 'en')))
  })

  it('carries the native name, left empty when it equals the name', () => {
    const list = listPhoneCountries('en')

    expect(list.find((country) => country.iso === 'AM')?.nativeName).toBe('Հայաստան')
    expect(list.find((country) => country.iso === 'US')?.nativeName).toBe('')
  })

  it('names countries in Russian for the Russian locale', () => {
    expect(listPhoneCountries('ru').find((country) => country.iso === 'AM')?.name).toBe('Армения')
  })
})

describe('filterPhoneCountries', () => {
  const list = listPhoneCountries('en')

  it('returns the whole list for an empty query', () => {
    expect(filterPhoneCountries(list, '  ')).toBe(list)
  })

  it('matches the name, case-insensitively', () => {
    expect(filterPhoneCountries(list, 'armen').map((country) => country.iso)).toContain('AM')
  })

  it('matches the native name', () => {
    expect(filterPhoneCountries(list, 'Հայաստան').map((country) => country.iso)).toEqual(['AM'])
  })

  it('matches the start of the ISO code', () => {
    expect(filterPhoneCountries(list, 'am').map((country) => country.iso)).toContain('AM')
  })

  it('matches the calling code with or without the plus sign', () => {
    expect(filterPhoneCountries(list, '+374').map((country) => country.iso)).toEqual(['AM'])
    expect(filterPhoneCountries(list, '374').map((country) => country.iso)).toEqual(['AM'])
  })

  it('returns nothing for a query that matches no country', () => {
    expect(filterPhoneCountries(list, 'zzzzqq')).toEqual([])
  })
})

describe('composePhoneValue', () => {
  it('stores a valid number in international format', () => {
    expect(composePhoneValue('AM', '77 123456')).toBe('+37477123456')
  })

  it('keeps what was typed behind the calling code when the number is not valid yet', () => {
    expect(composePhoneValue('AM', '77 12')).toBe('+374 77 12')
  })

  it('is empty when nothing was typed', () => {
    expect(composePhoneValue('AM', '   ')).toBe('')
  })

  it('accepts a full international number typed in the field', () => {
    expect(composePhoneValue('AM', '+14155552671')).toBe('+14155552671')
  })
})

describe('splitPhoneValue', () => {
  it('splits an international number into country and national part', () => {
    expect(splitPhoneValue('+37477123456', 'US')).toEqual({ iso: 'AM', national: '77123456' })
  })

  it('splits a number that is not valid by its calling code', () => {
    expect(splitPhoneValue('+374 77 12', 'US')).toEqual({ iso: 'AM', national: '7712' })
  })

  it('keeps the fallback country for a number without a plus sign', () => {
    expect(splitPhoneValue('77 123456', DEFAULT_PHONE_COUNTRY)).toEqual({ iso: 'AM', national: '77 123456' })
  })

  it('returns the fallback and an empty number for an empty value', () => {
    expect(splitPhoneValue('', 'GB')).toEqual({ iso: 'GB', national: '' })
  })
})
