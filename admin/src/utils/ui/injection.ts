/**
 * Provide/inject contracts between kit parents and their parts.
 *
 * A table hands its density to its rows and cells this way, so a screen sets it once on
 * `<UiTable density="compact">` instead of repeating padding classes on every cell.
 */
import type { InjectionKey } from 'vue'

/** Closed set of table paddings. */
export const UiTableDensity = {
  /** No padding of its own; the screen's classes decide (compatibility). */
  Plain: 'plain',
  Compact: 'compact',
  Comfortable: 'comfortable',
} as const

/** One of the table paddings. */
export type UiTableDensity = (typeof UiTableDensity)[keyof typeof UiTableDensity]

/** What `UiTable` shares with its parts. */
export interface UiTableContext {
  /** Padding applied to header and data cells. */
  density: UiTableDensity
}

/** Injection key for the table context. */
export const UI_TABLE_CONTEXT: InjectionKey<UiTableContext> = Symbol('ui-table-context')
