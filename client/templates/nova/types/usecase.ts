/**
 * One audience card of the use-cases grid.
 *
 * @module templates/nova/types/usecase
 */

/** One audience card in the use-cases grid. */
export interface UseCase {
  /** Stable key, used for the list key. */
  key: string
  /** Iconify name. */
  icon: string
  /** i18n key for the title. */
  titleKey: string
  /** i18n key for the body. */
  bodyKey: string
}
