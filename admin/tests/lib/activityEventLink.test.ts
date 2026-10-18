/**
 * Tests for the notification link: each event kind points at the screen of the record it is about.
 */
import { describe, expect, it } from 'vitest'
import { activityEventLink } from '@/lib/activityEventLink'
import { ActivityEventType } from '@/types/activityevent'

/**
 * Builds an event of a kind.
 *
 * @param type - The event kind.
 * @returns A feed event about entity 12.
 */
const event = (type: ActivityEventType) => ({ type, message: 'm', occurredAt: '2026-05-01T09:00:00Z', entityId: 12 })

describe('activityEventLink', () => {
  it('opens the new client', () => {
    expect(activityEventLink(event(ActivityEventType.ClientRegistered))).toBe('/clients/12')
  })

  it('opens the overdue invoice', () => {
    expect(activityEventLink(event(ActivityEventType.InvoiceOverdue))).toBe('/billing/12')
  })

  it('opens the domains list for an expiring domain', () => {
    expect(activityEventLink(event(ActivityEventType.DomainExpiring))).toBe('/domains')
  })
})
