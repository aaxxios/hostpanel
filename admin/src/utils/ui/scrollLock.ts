/**
 * Page scroll lock shared by the modal surfaces. Reference-counted so nested dialogs keep the page
 * locked until the last one closes, and the original `overflow` is restored exactly once.
 */

/** Number of surfaces currently holding the lock. */
let holders = 0

/** The inline `overflow` of `<body>` before the first lock. */
let previousOverflow = ''

/** Locks page scrolling; every call must be paired with one `unlockScroll`. */
export const lockScroll = (): void => {
  if (holders === 0) {
    previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
  }
  holders += 1
}

/** Releases one hold; restores the page scroll when the last hold is released. */
export const unlockScroll = (): void => {
  if (holders === 0) {
    return
  }
  holders -= 1
  if (holders === 0) {
    document.body.style.overflow = previousOverflow
  }
}
