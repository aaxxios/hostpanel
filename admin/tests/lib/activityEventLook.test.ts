/**
 * Tests for the notification-feed event look: every event type has an icon and a theme-driven colour.
 */
import { describe, expect, it } from 'vitest'
import { ACTIVITY_EVENT_LOOK } from '@/lib/activityEventLook'
import { ActivityEventType } from '@/types/activityevent'
import { toneTextClasses, UiTone } from '@/utils/ui/tone'

describe('ACTIVITY_EVENT_LOOK', () => {
  it('covers every event type with an icon', () => {
    for (const type of Object.values(ActivityEventType)) {
      expect(ACTIVITY_EVENT_LOOK[type].icon).not.toBe('')
    }
  })

  it('takes the colours from the shared tone vocabulary', () => {
    expect(ACTIVITY_EVENT_LOOK[ActivityEventType.InvoiceOverdue].color).toBe(toneTextClasses[UiTone.Danger])
    expect(ACTIVITY_EVENT_LOOK[ActivityEventType.ClientRegistered].color).toBe(toneTextClasses[UiTone.Success])
    expect(ACTIVITY_EVENT_LOOK[ActivityEventType.DomainExpiring].color).toBe(toneTextClasses[UiTone.Warning])
  })
})
