/**
 * Vocabulary of the advanced-filter rows (`UiAdvancedFilters`): how a row's value is matched
 * against a field. Shared by the component and by the pure helper that narrows a returned page.
 */

/** Closed set of ways a filter row matches its value. */
export const UiFilterCondition = {
  /** The field equals the value (ignoring case). */
  Exact: 'exact',
  /** The field contains the value (ignoring case). */
  Contains: 'contains',
} as const

/** One of the {@link UiFilterCondition} values. */
export type UiFilterCondition = (typeof UiFilterCondition)[keyof typeof UiFilterCondition]
