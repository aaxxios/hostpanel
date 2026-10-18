/**
 * Unit tests for the focus trap: Tab wraps inside the container.
 */
import { afterEach, describe, expect, it } from 'vitest'
import { focusableWithin, focusFirst, trapFocus } from '@/utils/ui/focusTrap'

/**
 * Builds a container with two buttons and appends it to the document.
 *
 * @returns The container and its buttons.
 */
const setup = () => {
  const root = document.createElement('div')
  root.tabIndex = -1
  const first = document.createElement('button')
  const second = document.createElement('button')
  root.append(first, second)
  document.body.append(root)
  return { root, first, second }
}

/**
 * Creates a keydown event for a key.
 *
 * @param key - Key name.
 * @param shiftKey - Whether Shift is held.
 * @returns The event.
 */
const key = (key: string, shiftKey = false): KeyboardEvent =>
  new KeyboardEvent('keydown', { key, shiftKey, cancelable: true })

afterEach(() => {
  document.body.innerHTML = ''
})

describe('focus trap', () => {
  it('lists focusable descendants and skips disabled ones', () => {
    const { root, second } = setup()
    second.disabled = true

    expect(focusableWithin(root)).toHaveLength(1)
  })

  it('focuses the first focusable element', () => {
    const { root, first } = setup()

    focusFirst(root)

    expect(document.activeElement).toBe(first)
  })

  it('wraps Tab from the last element to the first', () => {
    const { root, first, second } = setup()
    second.focus()
    const event = key('Tab')

    trapFocus(root, event)

    expect(event.defaultPrevented).toBe(true)
    expect(document.activeElement).toBe(first)
  })

  it('wraps Shift+Tab from the first element to the last', () => {
    const { root, first, second } = setup()
    first.focus()
    const event = key('Tab', true)

    trapFocus(root, event)

    expect(event.defaultPrevented).toBe(true)
    expect(document.activeElement).toBe(second)
  })

  it('leaves Tab alone in the middle and ignores other keys', () => {
    const { root, first } = setup()
    first.focus()
    const tab = key('Tab')
    const other = key('a')

    trapFocus(root, tab)
    trapFocus(root, other)

    expect(tab.defaultPrevented).toBe(false)
    expect(other.defaultPrevented).toBe(false)
  })
})
