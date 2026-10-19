/**
 * The support working hours the operator publishes on the contact page.
 *
 * The hours and their time zone are the operator's own values, from the admin-managed
 * `portal.contact.*` settings or the matching environment variables. Nothing is written
 * in the source: a self-hosted install has its own hours and its own zone, and an install
 * that sets neither shows no row at all.
 *
 * @module composables/useContactHours
 */

import { PortalSettingKey } from '~/constants/portalSettingKey'

/**
 * Reads the published working hours.
 *
 * @returns The reactive display line.
 */
export const useContactHours = () => {
  const { get } = usePortalSettings()

  /**
   * Hours followed by the time zone, e.g. the two values joined by a space.
   * Empty when the hours are empty, even if a time zone is set: a zone alone says nothing.
   */
  const workingHours = computed(() => {
    const hours = get(PortalSettingKey.ContactHours, 'portalContactHours')
    if (!hours) {
      return ''
    }

    const timezone = get(PortalSettingKey.ContactTimezone, 'portalContactTimezone')
    return timezone ? `${hours} ${timezone}` : hours
  })

  return { workingHours }
}
