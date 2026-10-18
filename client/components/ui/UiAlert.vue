<template>
  <div class="flex items-start gap-3 p-4 rounded-xl border" :class="styles.wrap" role="alert">
    <component :is="icon" :size="iconSize" :stroke-width="2" class="flex-shrink-0 mt-0.5" :class="styles.icon" />
    <div class="flex-1 min-w-0">
      <p v-if="title" class="font-semibold text-sm" :class="styles.title">{{ title }}</p>
      <div class="text-sm" :class="[styles.body, title ? 'mt-0.5' : '']">
        <slot />
      </div>
    </div>
    <slot name="action" />
  </div>
</template>

<script setup lang="ts">
/**
 * UiAlert - an inline banner for a warning, error, info or success message.
 *
 * Slots: default (message body) and `action` (a trailing button or link).
 */
import { computed } from 'vue'
import { AlertTriangle, CheckCircle, Info } from 'lucide-vue-next'
import { UiAlertVariant } from '~/utils/ui/enums'
import { ALERT_TONE_CLASSES } from '~/utils/ui/tones'

/** Props of UiAlert. */
interface Props {
  /** What the banner means; picks colours and icon. */
  variant?: `${UiAlertVariant}`
  /** Bold first line above the body. */
  title?: string
  /** Pixel size of the leading icon. */
  iconSize?: number
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), {
  variant: UiAlertVariant.Warning,
  title: undefined,
  iconSize: 18,
})

/** Leading icon per meaning. */
const ICONS = {
  [UiAlertVariant.Warning]: AlertTriangle,
  [UiAlertVariant.Error]: AlertTriangle,
  [UiAlertVariant.Info]: Info,
  [UiAlertVariant.Success]: CheckCircle,
}

/** Colour classes of the chosen meaning. */
const styles = computed(() => ALERT_TONE_CLASSES[props.variant as UiAlertVariant])

/** Icon component of the chosen meaning. */
const icon = computed(() => ICONS[props.variant as UiAlertVariant])
</script>
