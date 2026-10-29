/**
 * Behaviour shared by the modal surfaces (`UiDialog`, `UiDrawer`): close on Escape, keep Tab inside,
 * move focus in when opened and give it back to the opener when closed.
 *
 * Surfaces can nest (a confirmation opened from a dialog), so the open ones are kept in a stack and
 * Escape always closes only the topmost, whichever surface the keystroke reached.
 */
import { nextTick, onBeforeUnmount, onMounted, watch } from 'vue'
import type { Ref } from 'vue'
import { focusFirst, trapFocus } from '@/utils/ui/focusTrap'
import { lockScroll, unlockScroll } from '@/utils/ui/scrollLock'
import { Key } from './keys'

/** One open surface in the stack: what closes it. */
interface OpenSurface {
  /** Asks the surface to close. */
  close: () => void
}

/** The open modal surfaces, the most recently opened last. */
const openSurfaces: OpenSurface[] = []

/**
 * Wires a modal surface's keyboard and focus behaviour.
 *
 * @param panel - The surface element (the dialog or drawer panel).
 * @param isOpen - Whether the surface is currently shown.
 * @param close - Called when the user presses Escape.
 * @returns The `keydown` handler to bind on the surface.
 */
export const useModalSurface = (
  panel: Ref<HTMLElement | null>,
  isOpen: () => boolean,
  close: () => void,
): ((event: KeyboardEvent) => void) => {
  let opener: HTMLElement | null = null
  /** This surface's place in the stack of open surfaces. */
  const self: OpenSurface = { close }
  /** Whether this surface currently holds the page scroll lock. */
  let locked = false

  /** Remembers the focused element and moves focus into the panel once it is rendered. */
  const enter = async (): Promise<void> => {
    if (!locked) {
      lockScroll()
      openSurfaces.push(self)
      locked = true
    }
    opener = document.activeElement instanceof HTMLElement ? document.activeElement : null
    await nextTick()
    if (panel.value !== null) {
      focusFirst(panel.value)
    }
  }

  /** Gives focus back to the element that opened the surface. */
  const leave = (): void => {
    if (locked) {
      unlockScroll()
      openSurfaces.splice(openSurfaces.indexOf(self), 1)
      locked = false
    }
    opener?.focus()
    opener = null
  }

  onMounted(() => {
    if (isOpen()) {
      void enter()
    }
  })

  watch(isOpen, (open) => {
    if (open) {
      void enter()
    } else {
      leave()
    }
  })

  onBeforeUnmount(leave)

  return (event: KeyboardEvent): void => {
    if (event.key === Key.Escape) {
      event.stopPropagation()
      const topmost = locked ? openSurfaces[openSurfaces.length - 1] : undefined
      const target = topmost ?? self
      target.close()
      return
    }
    if (panel.value !== null) {
      trapFocus(panel.value, event)
    }
  }
}
