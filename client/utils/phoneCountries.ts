/**
 * Country data and number handling for the phone field.
 *
 * The country list comes from the phone-number library (which countries have a calling code) and
 * the runtime's own region names (`Intl.DisplayNames`), so the list ships no copy of country data
 * and is named in the reader's language. The native name is the same region in the language that
 * country most likely speaks. Everything here is pure; the components own the state.
 *
 * @module utils/phoneCountries
 */

import { getCountries, getCountryCallingCode, parsePhoneNumberFromString } from 'libphonenumber-js'
import type { CountryCode } from 'libphonenumber-js'

/** The country the phone field offers first: the operator's home market. */
export const DEFAULT_PHONE_COUNTRY = 'AM'

/** One row of the country list. */
export interface PhoneCountry {
  /** ISO 3166-1 alpha-2 code, upper case. */
  iso: string
  /** Calling code with the plus sign (`+374`). */
  dialCode: string
  /** Name in the reader's language. */
  name: string
  /** Name in the country's own language; empty when it equals {@link PhoneCountry.name}. */
  nativeName: string
}

/** A phone value split into the country and the number typed after the calling code. */
export interface PhoneParts {
  /** ISO code of the country the number belongs to. */
  iso: string
  /** The national part of the number. */
  national: string
}

/**
 * Reads the calling code of a country.
 *
 * @param iso ISO code of the country.
 * @returns The calling code with the plus sign, or an empty string for an unknown code.
 */
export const dialCodeOf = (iso: string): string => {
  try {
    return `+${getCountryCallingCode(iso as CountryCode)}`
  } catch {
    return ''
  }
}

/**
 * Names a country in the language that country most likely speaks.
 *
 * @param iso ISO code of the country.
 * @returns The native name, or an empty string when the runtime has none.
 */
const nativeNameOf = (iso: string): string => {
  try {
    const language = new Intl.Locale(`und-${iso}`).maximize().language
    return new Intl.DisplayNames([`${language}-${iso}`], { type: 'region' }).of(iso) ?? ''
  } catch {
    return ''
  }
}

/**
 * Builds the country list in a language, sorted by name.
 *
 * @param locale BCP 47 tag of the reader's language.
 * @returns Every country that has a calling code and a real name.
 */
export const listPhoneCountries = (locale: string): PhoneCountry[] => {
  const names = new Intl.DisplayNames([locale], { type: 'region' })
  const countries: PhoneCountry[] = []
  for (const iso of getCountries()) {
    const name = names.of(iso) ?? iso
    if (name === iso) {
      continue // a code the runtime cannot name is not a country a reader can pick
    }
    const native = nativeNameOf(iso)
    countries.push({
      iso,
      dialCode: dialCodeOf(iso),
      name,
      nativeName: native.toLowerCase() === name.toLowerCase() ? '' : native,
    })
  }
  return countries.sort((a, b) => a.name.localeCompare(b.name, locale))
}

/**
 * Filters the list by what the reader typed.
 *
 * A query of digits (with an optional plus) matches the start of a calling code; any other query
 * matches inside the name or native name, or the start of the ISO code.
 *
 * @param countries The full list.
 * @param query The search text.
 * @returns The matching countries in list order; the whole list for an empty query.
 */
export const filterPhoneCountries = (countries: PhoneCountry[], query: string): PhoneCountry[] => {
  const term = query.trim().toLowerCase()
  if (!term) {
    return countries
  }
  const digits = term.replace(/^\+/, '')
  if (/^\d+$/.test(digits)) {
    return countries.filter((country) => country.dialCode.slice(1).startsWith(digits))
  }
  return countries.filter((country) =>
    country.name.toLowerCase().includes(term)
    || country.nativeName.toLowerCase().includes(term)
    || country.iso.toLowerCase().startsWith(term),
  )
}

/**
 * Builds the value the field stores from the chosen country and the typed number.
 *
 * A number that is valid for the country is stored in international format (`+37410123456`);
 * anything else is stored as typed, behind the calling code, so the reader's text is never lost.
 *
 * @param iso ISO code of the chosen country.
 * @param national What the reader typed after the calling code.
 * @returns The value to store; empty when nothing was typed.
 */
export const composePhoneValue = (iso: string, national: string): string => {
  const typed = national.trim()
  if (!typed) {
    return ''
  }
  const parsed = parsePhoneNumberFromString(typed, iso as CountryCode)
  if (parsed?.isValid()) {
    return parsed.number
  }
  return typed.startsWith('+') ? typed : `${dialCodeOf(iso)} ${typed}`
}

/**
 * Finds the country whose calling code starts a number.
 *
 * @param digits The digits after the plus sign.
 * @returns The ISO code of the first country with that calling code, or undefined.
 */
const countryByDigits = (digits: string): string | undefined => {
  for (let length = Math.min(3, digits.length); length >= 1; length -= 1) {
    const prefix = digits.slice(0, length)
    const match = getCountries().find((iso) => String(getCountryCallingCode(iso)) === prefix)
    if (match) {
      return match
    }
  }
  return undefined
}

/**
 * Splits a stored value back into the country and the national number.
 *
 * @param value The stored value.
 * @param fallback The country used when the value names none.
 * @returns The country and the national part to show in the field.
 */
export const splitPhoneValue = (value: string, fallback: string): PhoneParts => {
  const text = value.trim()
  if (!text.startsWith('+')) {
    return { iso: fallback, national: text }
  }
  const parsed = parsePhoneNumberFromString(text)
  if (parsed?.country) {
    return { iso: parsed.country, national: parsed.nationalNumber }
  }
  const digits = text.replace(/\D/g, '')
  const iso = countryByDigits(digits)
  if (!iso) {
    return { iso: fallback, national: text }
  }
  return { iso, national: digits.slice(String(getCountryCallingCode(iso as CountryCode)).length) }
}
