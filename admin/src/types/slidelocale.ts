/**
 * The languages a slide carries text for. The members are the locale codes the API stores in
 * `SlideTranslation.locale`; adding a language means adding a member here and its tab label.
 */

/** A language a slide can be translated into. */
export enum SlideLocale {
  /** English. */
  En = 'en',
  /** Russian. */
  Ru = 'ru',
  /** Armenian. */
  Hy = 'hy',
}
