/**
 * One row of the control-panel preview.
 *
 * @module templates/nova/types/dashboardrow
 */

/** One row of the control-panel preview: an icon, a label and a stated value. */
export interface DashboardRow {
  /** Stable key, used for the list key. */
  key: string
  /** Iconify name. */
  icon: string
  /** i18n key for the area name. */
  labelKey: string
  /** i18n key for the stated value. */
  valueKey: string
}
