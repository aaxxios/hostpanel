/**
 * Design utility functions
 * Helper functions for common design patterns
 */

/**
 * Initials of a name, for avatars.
 *
 * @param name Full name, words separated by spaces.
 * @returns The first letter of each word, upper-cased.
 */
export const getInitials = (name: string): string => {
  return name
    .split(' ')
    .map(n => n[0])
    .join('')
    .toUpperCase()
}
