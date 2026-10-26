/**
 * The one bit test behind every "may this person do that" decision in the client portal.
 *
 * The permission value is the integer of `ClientPermission` flags the API reports for the signed-in
 * person (`clientPermissions` on `GET /api/auth/me`, carried on the portal's `ClientUser`). It only
 * decides what to show; the API enforces the same flags on every portal request, so a hidden link is
 * a courtesy and never the protection.
 *
 * @module utils/holdsPermission
 */

/**
 * Whether a permission value contains every bit of the required flag.
 *
 * @param held - The permission integer the person holds; a missing value holds nothing.
 * @param required - One flag, or several combined with bitwise OR (all of them are required).
 *        `0` names no requirement and is always satisfied.
 * @returns True when every required bit is set in `held`.
 */
export const holdsPermission = (held: number | null | undefined, required: number): boolean => {
  if (required === 0) {
    return true
  }
  return ((held ?? 0) & required) === required
}
