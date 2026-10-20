/**
 * Entrance reveal shared by the marketing blocks.
 *
 * The reveal is a CSS keyframe (`animate-reveal-up`, see `tailwind.config.ts`), not a
 * scroll-triggered script: the element is never rendered hidden, so its content is in the server
 * HTML, visible before hydration, in both themes, and when nothing ever scrolls it into view. A
 * block takes the classes `animate-reveal-up motion-reduce:animate-none` (written out in the
 * template so Tailwind can read them) and, to stagger a group, `:style="revealStyle(delayMs)"`.
 *
 * @module utils/reveal
 */

/**
 * The inline style that delays an element's `animate-reveal-up` entrance.
 *
 * The animation fills `backwards`, so the delayed element holds the first keyframe only for the
 * length of the delay and is fully visible afterwards.
 *
 * @param delayMs Delay before the entrance starts, in milliseconds; 0 or less starts at once.
 * @returns A style object to bind with `:style`.
 */
export const revealStyle = (delayMs: number): { animationDelay: string } => {
  return { animationDelay: `${Math.max(0, delayMs)}ms` }
}
