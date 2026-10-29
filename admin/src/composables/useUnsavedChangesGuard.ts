/** Composable that parks a route change while the reader decides about unsaved edits. */
import { ref, type Ref } from 'vue'
import { onBeforeRouteLeave } from 'vue-router'

/** What {@link useUnsavedChangesGuard} hands back to the view. */
export interface UnsavedChangesGuard {
  /** True while the question is on screen and the navigation is parked. */
  pending: Ref<boolean>
  /** Answers "leave anyway": the parked navigation proceeds. */
  confirmLeave: () => void
  /** Answers "stay": the parked navigation is cancelled. */
  cancelLeave: () => void
}

/**
 * Parks a route change while the reader is asked about unsaved edits.
 *
 * A form that navigates away silently loses whatever was typed, so the guard exists. It asks
 * through the view's modal rather than the browser's `confirm()`: `onBeforeRouteLeave` accepts
 * a returned promise and the router waits for it, so the guard returns one and this composable
 * resolves it when the modal reports an answer.
 *
 * @param isDirty Called at navigation time; return true to ask, false to leave without asking.
 *                It is a function rather than a ref so the caller can fold in conditions like
 *                "a save is already in flight, so this is not a loss".
 * @returns The modal's open state and the two answers.
 *
 * @example
 * const guard = useUnsavedChangesGuard(() => isDirty.value && !saving.value)
 * // <UiDialog variant="confirm" v-if="guard.pending.value" @confirm="guard.confirmLeave" @close="guard.cancelLeave" />
 */
export const useUnsavedChangesGuard = (isDirty: () => boolean): UnsavedChangesGuard => {
  const pending = ref(false)

  /**
   * Resolver of the promise the guard returned, held between the question and the answer.
   *
   * Null whenever no navigation is parked, which is what makes a stray answer harmless.
   */
  let resolveLeave: ((leave: boolean) => void) | null = null

  onBeforeRouteLeave(() => {
    if (!isDirty()) {
      return true
    }

    pending.value = true
    return new Promise<boolean>((resolve) => {
      resolveLeave = resolve
    })
  })

  /**
   * Closes the question and releases the parked navigation with the reader's answer.
   *
   * @param leave - True to let the navigation proceed, false to cancel it.
   */
  const answer = (leave: boolean): void => {
    pending.value = false
    resolveLeave?.(leave)
    resolveLeave = null
  }

  /**
   * Lets the parked navigation proceed.
   */
  const confirmLeave = (): void => {
    answer(true)
  }

  /**
   * Cancels the parked navigation.
   */
  const cancelLeave = (): void => {
    answer(false)
  }

  return {
    pending,
    confirmLeave,
    cancelLeave,
  }
}
