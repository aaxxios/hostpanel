/**
 * Reading the usage figures a hosting service reports.
 *
 * The backend sends disk and bandwidth use and allowance as megabytes, with an allowance of 0
 * standing for "no limit". Both service screens (the list card and the detail page) draw the same
 * bar, so the arithmetic lives here once.
 *
 * @module utils/serviceUsage
 */

/**
 * Whether an allowance is a real limit.
 *
 * @param limitMb - The allowance in megabytes.
 * @returns False for 0 (or less), which means unlimited.
 */
export const hasUsageLimit = (limitMb: number): boolean => limitMb > 0

/**
 * How much of an allowance is used, as a percentage capped at 100.
 *
 * @param usedMb - Use so far, in megabytes.
 * @param limitMb - The allowance, in megabytes.
 * @returns 0 to 100; 0 when the allowance is not positive.
 */
export const usagePercent = (usedMb: number, limitMb: number): number => {
  if (limitMb <= 0) {
    return 0
  }
  return Math.min(100, (usedMb / limitMb) * 100)
}
