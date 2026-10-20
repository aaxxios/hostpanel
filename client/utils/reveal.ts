/**
 * Entrance reveal shared by the marketing blocks.
 *
 * The reveal is a CSS keyframe (`animate-reveal-up`, see `tailwind.config.ts`), not a
 * scroll-triggered script: the element is never rendered hidden, so its content is in the server
 * HTML, visible before hydration, in both themes, and when nothing ever scrolls it into view. A
 * block takes the classes `animate-reveal-up motion-reduce:animate-none` (written out in the
 * template so Tailwind can read them) and, to stagger a group, `:class="revealDelay(delayMs)"`.
 *
 * @module utils/reveal
 */

/** Longest stagger the reveal supports, in milliseconds. */
const MAX_DELAY_MS = 1500

/** Granularity of the stagger, in milliseconds; a requested delay snaps to the nearest step. */
const STEP_MS = 50

/**
 * Every delay class written out in full, so Tailwind can read each one from this file (a class
 * assembled at run time would be dropped from the bundle).
 */
const DELAY_CLASSES: Record<number, string> = {
  0: '[animation-delay:0ms]',
  50: '[animation-delay:50ms]',
  100: '[animation-delay:100ms]',
  150: '[animation-delay:150ms]',
  200: '[animation-delay:200ms]',
  250: '[animation-delay:250ms]',
  300: '[animation-delay:300ms]',
  350: '[animation-delay:350ms]',
  400: '[animation-delay:400ms]',
  450: '[animation-delay:450ms]',
  500: '[animation-delay:500ms]',
  550: '[animation-delay:550ms]',
  600: '[animation-delay:600ms]',
  650: '[animation-delay:650ms]',
  700: '[animation-delay:700ms]',
  750: '[animation-delay:750ms]',
  800: '[animation-delay:800ms]',
  850: '[animation-delay:850ms]',
  900: '[animation-delay:900ms]',
  950: '[animation-delay:950ms]',
  1000: '[animation-delay:1000ms]',
  1050: '[animation-delay:1050ms]',
  1100: '[animation-delay:1100ms]',
  1150: '[animation-delay:1150ms]',
  1200: '[animation-delay:1200ms]',
  1250: '[animation-delay:1250ms]',
  1300: '[animation-delay:1300ms]',
  1350: '[animation-delay:1350ms]',
  1400: '[animation-delay:1400ms]',
  1450: '[animation-delay:1450ms]',
  1500: '[animation-delay:1500ms]',
}

/**
 * The Tailwind class that delays an element's `animate-reveal-up` entrance.
 *
 * The animation fills `backwards`, so the delayed element holds the first keyframe only for the
 * length of the delay and is fully visible afterwards.
 *
 * @param delayMs Delay before the entrance starts, in milliseconds; 0 or less starts at once and
 * anything past {@link MAX_DELAY_MS} is capped. The value snaps to the nearest 50 ms step.
 * @returns A class to bind with `:class`.
 */
export const revealDelay = (delayMs: number): string => {
  const clamped = Math.min(MAX_DELAY_MS, Math.max(0, delayMs))
  return DELAY_CLASSES[Math.round(clamped / STEP_MS) * STEP_MS]
}
