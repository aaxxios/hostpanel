/**
 * The "1 year / 2 years / 3 years" choices of the domain renewal and transfer forms.
 *
 * @module utils/yearOptions
 */

/** One choice of the period select. */
export interface YearOption {
  /** Text shown to the reader. */
  label: string
  /** The number of years sent to the API. */
  value: number
}

/** The longest period offered, in years. */
export const MAX_YEARS = 3

/**
 * Builds the period choices from the two translated unit words.
 *
 * @param singular - The word for one year, already translated.
 * @param plural - The word for several years, already translated.
 * @returns One option per year from 1 to {@link MAX_YEARS}.
 */
export const buildYearOptions = (singular: string, plural: string): YearOption[] =>
  Array.from({ length: MAX_YEARS }, (_, index) => ({
    label: `${index + 1} ${index === 0 ? singular : plural}`,
    value: index + 1
  }))
