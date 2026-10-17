<script setup lang="ts">
/**
 * Full-page admin notification feed — the destination of the top bar bell's "View all" link.
 *
 * Shows every event type the feed carries (client registrations, overdue invoices, domain
 * expiries), not just invoices. Reads `stores/activity`; each row links to the record it is about.
 */
import { onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import UiAlert from '@/components/ui/UiAlert.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiEmptyState from '@/components/ui/UiEmptyState.vue'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiLink from '@/components/ui/UiLink.vue'
import UiPageHeader from '@/components/ui/UiPageHeader.vue'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { activityEventLink } from '@/lib/activityEventLink'
import { formatRelativeFromNow } from '@/lib/format'
import { Icon } from '@/lib/icons'
import { useActivityStore } from '@/stores/activity'
import { ActivityEventType } from '@/types/activityevent'

const { t, locale } = useI18n()

/** Activity store: the feed and its load state. */
const activity = useActivityStore()

/** How many events the full page asks for (the bell previews fewer). */
const FEED_LIMIT = 50

/** Icon markup and colour class per event type, so the list reads at a glance. */
const EVENT_LOOK: Record<ActivityEventType, { icon: string; color: string }> = {
  [ActivityEventType.ClientRegistered]: { icon: Icon.Users, color: 'text-status-green' },
  [ActivityEventType.InvoiceOverdue]: { icon: Icon.Alert, color: 'text-status-red' },
  [ActivityEventType.DomainExpiring]: { icon: Icon.Globe, color: 'text-status-yellow' },
}

onMounted(() => activity.fetchActivity(FEED_LIMIT))
</script>

<template>
  <UiStack>
    <UiPageHeader
      :title="t('common.notifications')"
      :subtitle="t('common.viewAllNotifications')"
      data-testid="notifications-header"
    />

    <UiAlert v-if="activity.error" variant="error" data-testid="notifications-error">
      {{ activity.error }}
    </UiAlert>

    <UiSkeleton v-else-if="activity.loading && activity.events.length === 0" :lines="4" />

    <UiCard v-else-if="activity.events.length === 0">
      <UiEmptyState :title="t('common.noNotifications')" data-testid="notifications-empty" />
    </UiCard>

    <UiCard v-else data-testid="notifications-list">
      <UiLink
        v-for="(event, index) in activity.events"
        :key="`${event.type}-${event.occurredAt}-${index}`"
        :to="activityEventLink(event)"
        class="flex items-start gap-3 px-4 py-3.5 border-b border-border last:border-0 hover:bg-surface-hover transition-colors"
        :data-testid="`notification-${index}`"
      >
        <UiIcon :markup="EVENT_LOOK[event.type].icon" size="sm" :class="EVENT_LOOK[event.type].color" />
        <UiStack gap="xs">
          <UiText>{{ event.message }}</UiText>
          <UiText tone="muted" size="xs">{{ formatRelativeFromNow(event.occurredAt, locale) }}</UiText>
        </UiStack>
      </UiLink>
    </UiCard>
  </UiStack>
</template>
