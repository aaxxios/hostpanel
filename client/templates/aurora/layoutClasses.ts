/**
 * Layout class strings shared by the aurora two-column sections.
 *
 * Complete class names so Tailwind reads them from this file.
 *
 * @module templates/aurora/layoutClasses
 */

/**
 * The short column of a two-column section: from `lg` it stays in view while the long column
 * beside it scrolls, so the space next to a tall list is never empty. `top-28` clears the header
 * and `self-start` stops the column stretching to the row height. Stacked normally on mobile.
 */
export const STICKY_COLUMN_CLASS = 'lg:sticky lg:top-28 lg:self-start'
