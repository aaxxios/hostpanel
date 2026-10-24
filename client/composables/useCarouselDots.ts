/**
 * Keeps a `UiCarouselDots` in step with a Swiper carousel.
 *
 * The carousel does not render its own pagination: the dots are a kit component driven from the
 * Swiper instance, so they are styled with Tailwind and need no stylesheet aimed at Swiper's class names.
 *
 * @module composables/useCarouselDots
 */

import { ref, shallowRef } from 'vue'

/** The part of a Swiper instance the dots read and move. */
export interface DotsCarousel {
  /** Index of the active slide, loop clones excluded. */
  realIndex: number
  /** Moves to a slide by its real index, also in loop mode. */
  slideToLoop: (index: number) => void
}

/**
 * Tracks the active slide of a carousel and moves it when a dot is pressed.
 *
 * @returns The active index, the handlers to bind to the carousel's `swiper` and `slide-change` events, and the dot handler.
 */
export const useCarouselDots = () => {
  /** The carousel instance, once the browser has created it. */
  const carousel = shallowRef<DotsCarousel | null>(null)
  /** Zero-based index of the active slide. */
  const active = ref(0)

  /**
   * Remembers the carousel and reads its position.
   *
   * @param instance The Swiper instance.
   */
  const onSwiper = (instance: DotsCarousel): void => {
    carousel.value = instance
    active.value = instance.realIndex
  }

  /**
   * Reads the position after the carousel moved, by a dot, an arrow, a swipe or autoplay.
   *
   * @param instance The Swiper instance.
   */
  const onSlideChange = (instance: DotsCarousel): void => {
    active.value = instance.realIndex
  }

  /**
   * Moves the carousel to the slide of a pressed dot.
   *
   * @param index Zero-based index of the slide.
   */
  const select = (index: number): void => {
    carousel.value?.slideToLoop(index)
  }

  return { active, onSwiper, onSlideChange, select }
}
