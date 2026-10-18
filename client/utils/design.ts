/**
 * Design utility functions
 * Helper functions for common design patterns
 */

/**
 * Get initials from name for avatars
 */
export const getInitials = (name: string): string => {
  return name
    .split(' ')
    .map(n => n[0])
    .join('')
    .toUpperCase()
}
