/**
 * Shared behaviour of the kit's overlays (UiDialog, UiDrawer): body scroll lock and Escape.
 *
 * Several overlays can be open at once (a confirm dialog over a drawer), so the scroll lock is a
 * module-level counter rather than a flag: the page scrolls again only when the last overlay
 * closes. The lock is the Tailwind `overflow-hidden` class on `<body>`, not an inline style.
 *
 * @module utils/ui/overlay
 */

import { nextTick, onBeforeUnmount, watch } from 'vue'

/** Number of overlays currently holding the scroll lock. */
let lockCount = 0

/** Locks page scrolling (re-entrant: call `unlockBodyScroll` once per call). */
export const lockBodyScroll = (): void => {
  lockCount += 1
  if (lockCount === 1 && typeof document !== 'undefined') {
    document.body.classList.add('overflow-hidden')
  }
}

/** Releases one scroll lock; the page scrolls again when none remain. */
export const unlockBodyScroll = (): void => {
  lockCount = Math.max(0, lockCount - 1)
  if (lockCount === 0 && typeof document !== 'undefined') {
    document.body.classList.remove('overflow-hidden')
  }
}

/** Selector of the elements Tab can land on inside a panel. */
const FOCUSABLE = 'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'

/** Open overlays, oldest first; only the last one reacts to Escape and Tab (nested dialogs). */
const stack: symbol[] = []

/**
 * Wires an overlay to the scroll lock, Escape, a focus trap and focus return while it is open.
 *
 * Focus moves into the panel on open, Tab cycles inside it, and on close focus goes back to the
 * element that opened the overlay. With nested overlays only the topmost one reacts to keys.
 *
 * @param isOpen Reactive getter for the overlay's open state.
 * @param close Called when the reader presses Escape.
 * @param panel Getter for the panel element that traps focus; without it only Escape is wired.
 */
export const useOverlay = (isOpen: () => boolean, close: () => void, panel?: () => HTMLElement | null): void => {
  /** Whether this overlay currently holds a scroll lock. */
  let holdsLock = false

  /** Identity of this overlay in the open stack. */
  const id = Symbol('overlay')

  /** The element focused before the overlay opened, focused again on close. */
  let trigger: HTMLElement | null = null

  /**
   * Handles Escape (close) and Tab (trap) for the topmost overlay.
   *
   * @param event The document key press.
   */
  const onKeydown = (event: KeyboardEvent): void => {
    if (stack[stack.length - 1] !== id) {
      return
    }
    if (event.key === 'Escape') {
      close()
      return
    }
    const root = panel?.()
    if (event.key !== 'Tab' || !root) {
      return
    }
    const items = Array.from(root.querySelectorAll<HTMLElement>(FOCUSABLE))
    const first = items[0]
    const last = items[items.length - 1]
    if (!first || !last) {
      event.preventDefault()
      root.focus()
      return
    }
    if (event.shiftKey && (document.activeElement === first || document.activeElement === root)) {
      event.preventDefault()
      last.focus()
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault()
      first.focus()
    }
  }

  /** Releases the lock, the key listener and the stack slot, and returns focus to the trigger. */
  const release = (): void => {
    if (!holdsLock) {
      return
    }
    holdsLock = false
    unlockBodyScroll()
    document.removeEventListener('keydown', onKeydown)
    const at = stack.indexOf(id)
    if (at >= 0) {
      stack.splice(at, 1)
    }
    trigger?.focus?.()
    trigger = null
  }

  /** Moves focus into the panel once it has rendered. */
  const focusPanel = (): void => {
    const root = panel?.()
    if (root && !root.contains(document.activeElement)) {
      (root.querySelector<HTMLElement>(FOCUSABLE) ?? root).focus()
    }
  }

  watch(isOpen, (open) => {
    if (open && !holdsLock) {
      holdsLock = true
      trigger = document.activeElement instanceof HTMLElement ? document.activeElement : null
      stack.push(id)
      lockBodyScroll()
      document.addEventListener('keydown', onKeydown)
      void nextTick(focusPanel)
    } else if (!open) {
      release()
    }
  }, { immediate: true })

  onBeforeUnmount(release)
}
