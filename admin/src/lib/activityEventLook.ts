/**
 * How each notification-feed event type looks: its icon and the colour of that icon.
 *
 * Shared by the full notification page and the dashboard's recent-activity card so the two read
 * the same. The colours come from the shared tone vocabulary, so they follow the theme.
 */

import { Icon } from '@/lib/icons'
import { ActivityEventType } from '@/types/activityevent'
import { toneTextClasses, UiTone } from '@/utils/ui/tone'

/** Icon markup and colour class of one event type. */
export interface ActivityEventLook {
  /** Inner SVG markup of the icon. */
  icon: string
  /** Foreground colour class of the icon. */
  color: string
}

/** Icon and colour per event type, so a list reads at a glance. */
export const ACTIVITY_EVENT_LOOK: Record<ActivityEventType, ActivityEventLook> = {
  [ActivityEventType.ClientRegistered]: { icon: Icon.Users, color: toneTextClasses[UiTone.Success] },
  [ActivityEventType.InvoiceOverdue]: { icon: Icon.Alert, color: toneTextClasses[UiTone.Danger] },
  [ActivityEventType.DomainExpiring]: { icon: Icon.Globe, color: toneTextClasses[UiTone.Warning] },
}
