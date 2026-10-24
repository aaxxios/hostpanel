/**
 * Tests for `useCarouselDots`: the active index follows the carousel it is handed, and a pressed
 * dot moves that carousel (also in loop mode, through `slideToLoop`).
 *
 * @module tests/composables/useCarouselDots.test
 */

import { describe, expect, it, vi } from 'vitest'
import type { Mock } from 'vitest'
import { useCarouselDots } from '~/composables/useCarouselDots'
import type { DotsCarousel } from '~/composables/useCarouselDots'

/**
 * Builds a stand-in carousel instance.
 *
 * @param realIndex The index it reports as active.
 * @returns The instance, with a spy for `slideToLoop`.
 */
const carousel = (realIndex: number): DotsCarousel & { slideToLoop: Mock<(index: number) => void> } => ({
  realIndex,
  slideToLoop: vi.fn<(index: number) => void>(),
})

describe('useCarouselDots', () => {
  it('starts on the first slide', () => {
    const { active } = useCarouselDots()

    expect(active.value).toBe(0)
  })

  it('reads the position when the carousel is created', () => {
    const { active, onSwiper } = useCarouselDots()

    onSwiper(carousel(2))

    expect(active.value).toBe(2)
  })

  it('follows the carousel when it moves', () => {
    const { active, onSwiper, onSlideChange } = useCarouselDots()
    const instance = carousel(0)
    onSwiper(instance)

    instance.realIndex = 3
    onSlideChange(instance)

    expect(active.value).toBe(3)
  })

  it('moves the carousel to the slide of a pressed dot', () => {
    const { onSwiper, select } = useCarouselDots()
    const instance = carousel(0)
    onSwiper(instance)

    select(1)

    expect(instance.slideToLoop).toHaveBeenCalledWith(1)
  })

  it('ignores a dot pressed before the carousel exists', () => {
    const { select, active } = useCarouselDots()

    expect(() => select(1)).not.toThrow()
    expect(active.value).toBe(0)
  })
})
