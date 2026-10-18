<template>
  <UiStack gap="xs" data-testid="usage-bar">
    <UiStack direction="row" justify="between">
      <UiText as="span" size="xs" tone="subtle">{{ label }}</UiText>
      <UiText as="span" size="xs" tone="subtle">{{ $t('client.services.usageOf', { used, limit }) }}</UiText>
    </UiStack>
    <UiProgress :value="usagePercent(used, limit)" :label="label" :tone="tone" />
  </UiStack>
</template>

<script setup lang="ts">
/**
 * UsageBar - one quota of a hosting service (disk or bandwidth): its name, "used / allowance" in
 * megabytes and a progress bar.
 */
import type { UiTone } from '~/utils/ui/enums'
import { usagePercent } from '~/utils/serviceUsage'

/** Props of UsageBar. */
interface Props {
  /** What is measured, for example "Disk". */
  label: string
  /** Use so far, in megabytes, as the API sent it. */
  used: string
  /** The allowance, in megabytes, as the API sent it. */
  limit: string
  /** Colour of the bar. */
  tone?: `${UiTone}`
}

withDefaults(defineProps<Props>(), {
  tone: 'primary',
})
</script>
