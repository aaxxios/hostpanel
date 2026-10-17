<template>
  <!-- Status badge used across the client area for services, domains, invoices and tickets -->
  <UiBadge :variant="tone" size="sm" rounded data-testid="status-badge">{{ label }}</UiBadge>
</template>

<script setup lang="ts">
/**
 * The status badge shared by services, domains, invoices and tickets: the status word in the
 * reader's language, in the tone the status table gives it.
 */
import { statusTone } from '~/utils/clientStatus'

/** Props of ClientStatusBadge. */
interface Props {
  /** Status string as the API sent it, e.g. `Active`, `Unpaid`, `PendingTransfer`. */
  status: string
}

/** Props of the component with their defaults applied. */
const props = defineProps<Props>()

/** Translator for the strings this file builds itself. */
const { t, te } = useI18n()

/** Tone of the badge, from the status table. */
const tone = computed(() => statusTone(props.status))

/** Localised status words; a status the catalogue does not name is shown as the API sent it. */
const label = computed(() => {
  const key = `client.status.${props.status}`
  return te(key) ? t(key) : props.status
})
</script>
