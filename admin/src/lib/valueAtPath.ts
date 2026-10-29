/**
 * Reads a value out of a nested object by a dot path, so a report column can name a field of a
 * nested record (`dates.created`) the same way it names a top-level one (`status`).
 */

/** Separator between the segments of a path. */
const PATH_SEPARATOR = '.'

/**
 * Reads the value a dot path points at.
 *
 * @param source - The object to read from.
 * @param path - Property names joined by dots, e.g. `address.city`; a name without a dot is a top-level property.
 * @returns The value at the path; `undefined` when any segment on the way is missing or not an object.
 */
export const valueAtPath = (source: object, path: string): unknown => {
  let current: unknown = source
  for (const segment of path.split(PATH_SEPARATOR)) {
    if (typeof current !== 'object' || current === null) {
      return undefined
    }
    current = (current as Record<string, unknown>)[segment]
  }
  return current
}
