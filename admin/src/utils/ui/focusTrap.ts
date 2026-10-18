/**
 * Keyboard focus containment for modal surfaces (`UiDialog`, `UiDrawer`).
 *
 * While a dialog is open, Tab and Shift+Tab must cycle inside it instead of reaching the page
 * behind. This module holds the pure logic so the dialog stays a template and the behaviour is
 * unit-testable on a plain element.
 */

/** Selector matching every element a user can tab to. */
const FOCUSABLE_SELECTOR =
  'a[href], button:not([disabled]), input:not([disabled]):not([type="hidden"]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'

/**
 * Lists the focusable descendants of a container, in DOM order.
 *
 * @param root - Container to search.
 * @returns The focusable elements, possibly empty.
 */
export const focusableWithin = (root: HTMLElement): HTMLElement[] => {
  return Array.from(root.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR))
}

/**
 * Moves focus to the first focusable descendant, or to the container itself when it has none.
 *
 * @param root - Container to focus into.
 */
export const focusFirst = (root: HTMLElement): void => {
  const first = focusableWithin(root)[0]
  if (first) {
    first.focus()
    return
  }
  root.focus()
}

/**
 * Keeps Tab navigation inside the container: wraps from the last element to the first and the
 * other way round for Shift+Tab. Call it from a `keydown` handler.
 *
 * @param root - The modal surface.
 * @param event - The keydown event; only `Tab` is handled.
 */
export const trapFocus = (root: HTMLElement, event: KeyboardEvent): void => {
  if (event.key !== 'Tab') {
    return
  }
  const items = focusableWithin(root)
  const first = items[0]
  const last = items[items.length - 1]
  if (!first || !last) {
    event.preventDefault()
    root.focus()
    return
  }
  const active = document.activeElement
  if (event.shiftKey && active === first) {
    event.preventDefault()
    last.focus()
  } else if (!event.shiftKey && active === last) {
    event.preventDefault()
    first.focus()
  }
}
