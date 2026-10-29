/**
 * Rendering a money amount without inventing the currency it is in.
 *
 * ## Where the currency comes from
 *
 * - **The client profile.** `ClientDto.Currency` in the C# API is a nullable ISO 4217 **code**
 *   (`backend/src/Innovayse.Hostpanel.Application/Clients/Common/ClientDto.cs`);
 *   `server/api/portal/client/me.get.ts` forwards it and `types/clientuser.ts` types it as
 *   `string | null`. It reaches this file as `store.user?.currency`, passed as
 *   {@link CurrencyInfo.code}.
 * - **Not `GET /api/portal/public/currencies`.** It proxies `/api/reference/currencies`, which
 *   returns `CurrencyList.All`, the static ISO 4217 reference table `{ code, name, symbol }`,
 *   not the operator's configured billing currencies.
 *
 * ## `formatMoney` is the sibling for the common case
 *
 * `stores/currency.ts` loads the operator's configured currencies, each with its own
 * `prefix`/`suffix`/`decimals`, from `GET /api/currencies`. A call site that knows which
 * configured currency an amount is in (a cart item, an invoice, a product price) calls
 * `utils/formatMoney.ts`'s `formatMoney(amount, currency)` instead of this function, because it
 * renders with the operator's own decimal count rather than `Intl`'s per-ISO-code guess. This
 * function covers the cases where only a bare ISO code, or nothing at all, is known.
 *
 * ## What this does instead of guessing
 *
 * Nothing in the frontend knows whether `24000` is drams, roubles or dollars. Printing `֏`
 * because the visitor happens to be reading Armenian would put a currency on a bill that
 * nobody verified, and a wrong symbol on a bill is worse than none, because it is believed.
 *
 * `Intl.NumberFormat` gives grouping and decimals with no symbol at all, so an amount with no
 * known currency still reads as money: `24,000.00`, or `24 000,00` for a Russian or Armenian
 * reader. When the API *does* send a currency (an ISO code, or a bare prefix/suffix pair)
 * it is used, because then it came from the operator's configuration rather than from a guess
 * made here.
 *
 * @module utils/formatCurrency
 */

import type { AppWithI18n } from '~/types/appwithi18n'

/**
 * What an amount renders as when there is no number to render.
 *
 * The same em dash `utils/formatDate.ts` uses, so an empty row reads consistently down a
 * description list whichever kind of field is missing.
 */
export const EMPTY_AMOUNT = '—'

/** Well-formed ISO 4217: three letters. Anything else is not a code `Intl` will accept. */
const ISO_4217 = /^[A-Za-z]{3}$/

/**
 * Everything the API might have told us about the currency an amount is in.
 *
 * Every field is optional and every field is routinely absent: see the module note. Passing
 * an empty object is the normal case today, not a degenerate one.
 */
export interface CurrencyInfo {
  /**
   * ISO 4217 code, e.g. `AMD`. The only field that yields a properly localised currency
   * rendering, because it is the only one `Intl.NumberFormat` can place and pluralise itself.
   */
  code?: string | null

  /** Symbol the operator configured to print before the amount, e.g. `$`. */
  prefix?: string | null

  /** Symbol the operator configured to print after the amount, e.g. ` AMD`. */
  suffix?: string | null

  /**
   * `@nuxtjs/i18n` locale code, deciding the grouping and decimal separators. Omit inside a
   * component to use the locale of the request being rendered.
   */
  locale?: string
}

/**
 * Reads the language to group digits in, without a module-level cache.
 *
 * Same reasoning as `utils/formatDate.ts`: a module-scoped "current locale" is shared by every
 * visitor a Nitro process is serving at once, so the locale is resolved per call from the Nuxt
 * instance of the request being rendered.
 *
 * @param locale - What the caller passed, if anything.
 * @returns The caller's locale, else the current request's, else `en`.
 */
const resolveLocale = (locale?: string): string => {
  if (locale) {
    return locale
  }

  try {
    const i18n = (useNuxtApp() as AppWithI18n).$i18n
    return i18n?.locale?.value ?? 'en'
  }
  catch {
    return 'en'
  }
}

/**
 * Coerces an amount the API sent into a number, or `null` when it is not one.
 *
 * @param amount - The field as it arrived. legacy-shaped endpoints send decimal *strings*
 *                 (`"24000.00"`), so a string is the common case rather than the exception.
 * @returns The numeric value, or `null` for anything absent or unparseable.
 */
const toNumber = (amount: string | number | null | undefined): number | null => {
  if (amount === null || amount === undefined || amount === '') {
    return null
  }

  const value = typeof amount === 'number' ? amount : Number(amount)
  return Number.isFinite(value) ? value : null
}

/**
 * Formats a money amount, using a currency only when the API supplied one.
 *
 * Three outcomes, in order of how much the API told us:
 *
 * - An ISO 4217 `code`: `Intl` places the symbol and picks the minor-unit count itself, so
 *   `AMD` renders with no decimals and `USD` with two, in the right position for the locale.
 * - A `prefix` and/or `suffix` and no code: the operator's own symbols wrap a grouped number.
 * - Neither: a grouped number with two decimals and **no symbol**. This is the honest answer
 *   and currently the production one; see the module note for why guessing is worse.
 *
 * @param amount - The amount, as a number or the decimal string the API sends.
 * @param info - What is known about the currency. Omit entirely for the no-currency case.
 * @returns The formatted amount, or {@link EMPTY_AMOUNT} when there is no number to show.
 */
export const formatCurrency = (
  amount: string | number | null | undefined,
  info: CurrencyInfo = {}
): string => {
  const value = toNumber(amount)
  if (value === null) {
    return EMPTY_AMOUNT
  }

  const locale = resolveLocale(info.locale)
  const code = info.code?.trim()

  if (code && ISO_4217.test(code)) {
    try {
      return new Intl.NumberFormat(locale, { style: 'currency', currency: code.toUpperCase() })
        .format(value)
    }
    catch {
      // `Intl` accepts any three-letter code and only throws on a malformed one, which the
      // regex above already rejects, so this is belt and braces against an ICU build that is
      // stricter. Falling through loses the symbol; throwing would lose the amount.
    }
  }

  const grouped = new Intl.NumberFormat(locale, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(value)

  const prefix = info.prefix ?? ''
  const suffix = info.suffix ?? ''
  return `${prefix}${grouped}${suffix}`
}
