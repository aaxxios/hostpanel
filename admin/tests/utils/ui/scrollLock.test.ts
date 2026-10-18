/**
 * Unit tests for the reference-counted page scroll lock.
 */
import { describe, expect, it } from 'vitest'
import { lockScroll, unlockScroll } from '@/utils/ui/scrollLock'

describe('scrollLock', () => {
  it('keeps the page locked until the last nested holder releases, then restores it', () => {
    document.body.style.overflow = 'auto'

    lockScroll()
    lockScroll()
    unlockScroll()

    expect(document.body.style.overflow).toBe('hidden')

    unlockScroll()

    expect(document.body.style.overflow).toBe('auto')
  })
})
