<script setup lang="ts">
/**
 * Badge showing a ticket's lifecycle status, coloured by the status and labelled in the
 * operator's language. Shared by the tickets list and the ticket detail page.
 */
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import UiBadge from '@/components/ui/UiBadge.vue'
import { ticketStatusTone } from '@/lib/tones'
import type { TicketStatus } from '@/types/ticket'

/** Props for SupportTicketStatusBadge. */
const props = defineProps<{
  /** The status to show. */
  status: TicketStatus
}>()

const { t } = useI18n()

/** Translated label of the status. */
const label = computed<string>(() => t(`ticketStatus.${props.status}`))
</script>

<template>
  <UiBadge :tone="ticketStatusTone[status]">{{ label }}</UiBadge>
</template>
