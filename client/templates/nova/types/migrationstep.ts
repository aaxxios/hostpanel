/**
 * One numbered step of the migration walkthrough.
 *
 * @module templates/nova/types/migrationstep
 */

/** One numbered step of the migration walkthrough. */
export interface MigrationStep {
  /** Stable key, used for the list key. */
  key: string
  /** i18n key for the step title. */
  titleKey: string
  /** i18n key for the step body. */
  bodyKey: string
}
