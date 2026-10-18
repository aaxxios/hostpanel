/**
 * Reading the usage figures a hosting service reports.
 *
 * The control panel sends disk and bandwidth use and allowance as strings of megabytes, with an
 * empty string or `"0"` standing for "no limit". Both service screens (the list card and the
 * detail page) draw the same bar, so the parsing lives here once.
 *
 * @module utils/serviceUsage
 */

/**
 * Whether a quota string names a real limit.
 *
 * @param limit - The allowance as the API sent it.
 * @returns False for an absent, empty or `"0"` allowance, which means unlimited.
 */
export const hasUsageLimit = (limit: string | null | undefined): boolean => Boolean(limit) && limit !== '0'

/**
 * How much of an allowance is used, as a percentage capped at 100.
 *
 * @param used - Use so far, as the API sent it.
 * @param limit - The allowance, as the API sent it.
 * @returns 0 to 100; 0 when either figure is not a number or the allowance is not positive.
 */
export const usagePercent = (used: string | null | undefined, limit: string | null | undefined): number => {
  const usedValue = Number.parseInt(used ?? '', 10)
  const limitValue = Number.parseInt(limit ?? '', 10)
  if (!Number.isFinite(usedValue) || !Number.isFinite(limitValue) || limitValue <= 0) {
    return 0
  }
  return Math.min(100, (usedValue / limitValue) * 100)
}
