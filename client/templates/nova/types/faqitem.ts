/**
 * One question of the FAQ accordion.
 *
 * @module templates/nova/types/faqitem
 */

/** One question in the FAQ accordion. */
export interface FaqItem {
  /** Stable key, used for the list key. */
  key: string
  /** i18n key for the question. */
  questionKey: string
  /** i18n key for the answer. */
  answerKey: string
}
