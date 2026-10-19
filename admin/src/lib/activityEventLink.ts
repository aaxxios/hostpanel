/**
 * Maps a notification-feed activity event to the admin screen it opens.
 */

import { ActivityEventType } from '@/types/activityevent'
import type { ActivityEvent } from '@/types/activityevent'
import { AppRoute, clientPath, invoicePath } from '@/lib/routes'

/**
 * Resolves where a notification-feed item should navigate to.
 *
 * A domain has no top-level detail route — only nested under its client
 * (`/clients/:id/domains/:domainId`), and the feed does not carry the owning
 * client's id — so a domain-expiring event links to the domains list instead
 * of a specific record.
 *
 * @param event - The activity event to link.
 * @returns The admin SPA route path for this event.
 */
export const activityEventLink = (event: ActivityEvent): string => {
  switch (event.type) {
    case ActivityEventType.ClientRegistered:
      return clientPath(event.entityId)
    case ActivityEventType.InvoiceOverdue:
      return invoicePath(event.entityId)
    case ActivityEventType.DomainExpiring:
      return AppRoute.Domains
  }
}
