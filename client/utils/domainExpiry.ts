/**
 * Whether a domain is close to its expiry date.
 *
 * Shared by the domain list and the domain detail page, so they agree on the sentinel for
 * "no date" and on whether the last day counts.
 *
 * @module utils/domainExpiry
 */

import { EMPTY_DATE, formatDate } from '~/utils/formatDate'

/** How many days before expiry a domain counts as "expiring soon". */
export const EXPIRING_SOON_DAYS = 30

/** Milliseconds in a day. */
const MS_PER_DAY = 86_400_000

/** How many days before expiry the dashboard starts offering a domain for renewal. */
export const EXPIRY_WARNING_DAYS = 45

/**
 * Checks whether a domain expires within a number of days from now.
 *
 * Instant arithmetic, deliberately not routed through the calendar-day reading `formatDate`
 * gives: "within N days" is a question about elapsed time. A zero-date sentinel or an
 * unparseable value is never inside the window.
 *
 * @param expiresAt - ISO 8601 expiration date as the API sent it.
 * @param days - The window length in days.
 * @param includePast - Whether a date that has already passed counts (the dashboard offers
 * lapsed domains for renewal, the list badge does not).
 * @param now - The moment to compare against; injectable so a test does not depend on the clock.
 * @returns True when the expiry lies inside the window.
 */
export const isExpiringWithin = (
  expiresAt: string | null | undefined,
  days: number,
  includePast: boolean = false,
  now: number = Date.now()
): boolean => {
  if (!expiresAt || formatDate(expiresAt) === EMPTY_DATE) {
    return false
  }
  const daysLeft = (new Date(expiresAt).getTime() - now) / MS_PER_DAY
  if (Number.isNaN(daysLeft)) {
    return false
  }
  return (includePast || daysLeft >= 0) && daysLeft <= days
}

/**
 * Checks whether a domain expires within {@link EXPIRING_SOON_DAYS} days from now.
 *
 * @param expiresAt - ISO 8601 expiration date as the API sent it.
 * @param now - The moment to compare against; injectable so a test does not depend on the clock.
 * @returns True when the expiry lies between now and 30 days ahead.
 */
export const isExpiringSoon = (expiresAt: string | null | undefined, now: number = Date.now()): boolean =>
  isExpiringWithin(expiresAt, EXPIRING_SOON_DAYS, false, now)
