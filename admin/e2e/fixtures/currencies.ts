/**
 * Fixtures for the currencies settings page.
 */
import type { CurrencyDto } from '../../src/types/currency'

/** The base currency: its rate is fixed and its edit form disables the rate field. */
export const usd: CurrencyDto = {
  code: 'USD',
  numeric: '840',
  prefix: '$',
  suffix: '',
  decimals: 2,
  rateToBase: 1,
  isBase: true,
  isEnabled: true,
}

/** A non-base currency, the one the specs edit. */
export const eur: CurrencyDto = {
  code: 'EUR',
  numeric: '978',
  prefix: '€',
  suffix: '',
  decimals: 2,
  rateToBase: 0.92,
  isBase: false,
  isEnabled: true,
}
