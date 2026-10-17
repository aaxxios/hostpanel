/**
 * The slice of the Nuxt app that carries the active locale, for utilities that format values
 * outside a component and so cannot call `useI18n()`.
 */
export interface AppWithI18n {
  /** The `@nuxtjs/i18n` plugin instance, absent outside a running app. */
  $i18n?: {
    /** The active locale. */
    locale?: {
      /** The locale code, e.g. `hy`. */
      value?: string
    }
  }
}
