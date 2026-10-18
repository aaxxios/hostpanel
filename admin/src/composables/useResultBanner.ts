/**
 * The result banner of a list screen: one line saying how the last bulk action went, which clears
 * itself after a few seconds.
 *
 * The currencies and TLD screens both run actions (refresh rates, import, sync) whose outcome has
 * no row to attach to; they share this instead of each keeping a message, a flag and a timer.
 */
import { onBeforeUnmount, ref } from 'vue'
import type { Ref } from 'vue'

/** How long the banner stays, in milliseconds. */
export const RESULT_BANNER_MS = 5000

/** What {@link useResultBanner} hands back. */
export interface ResultBanner {
  /** The sentence on show, or null when the banner is hidden. */
  message: Ref<string | null>
  /** Whether the sentence reports a failure. */
  isError: Ref<boolean>
  /** Shows a sentence for {@link RESULT_BANNER_MS}, replacing any earlier one. */
  show: (text: string, failed: boolean) => void
  /** Hides the banner at once. */
  clear: () => void
}

/**
 * Creates the banner state for one screen.
 *
 * @returns The message, the error flag and the two actions; the pending timer is cancelled when the
 * screen unmounts.
 */
export const useResultBanner = (): ResultBanner => {
  /** The sentence on show. */
  const message = ref<string | null>(null)

  /** Whether the sentence reports a failure. */
  const isError = ref(false)

  /** Handle of the pending auto-hide timer. */
  let timer: ReturnType<typeof setTimeout> | null = null

  /** Cancels the pending auto-hide timer, if any. */
  const cancelTimer = (): void => {
    if (timer !== null) {
      clearTimeout(timer)
      timer = null
    }
  }

  /**
   * Hides the banner at once.
   */
  const clear = (): void => {
    cancelTimer()
    message.value = null
    isError.value = false
  }

  /**
   * Shows a sentence and schedules it to disappear.
   *
   * @param text - The finished sentence.
   * @param failed - Whether it reports a failure.
   */
  const show = (text: string, failed: boolean): void => {
    cancelTimer()
    message.value = text
    isError.value = failed
    timer = setTimeout(clear, RESULT_BANNER_MS)
  }

  onBeforeUnmount(cancelTimer)

  return { message, isError, show, clear }
}
