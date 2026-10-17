<template>
  <component :is="as" :class="classes">
    <UiContainer v-if="contained">
      <slot />
    </UiContainer>
    <slot v-else />
  </component>
</template>

<script setup lang="ts">
/**
 * UiSection - a full-width page band with vertical padding and an optional tinted background.
 *
 * With `contained` its content sits inside the shared page-width UiContainer, so section edges
 * line up with the header and footer.
 */
import { computed } from 'vue'
import { UiSectionPadding, UiSectionTone } from '~/utils/ui/enums'

/** Props of UiSection. */
interface Props {
  /** Tag to render. */
  as?: string
  /** Vertical padding. */
  padding?: `${UiSectionPadding}`
  /** Background of the band. */
  tone?: `${UiSectionTone}`
  /** Wraps the content in UiContainer. */
  contained?: boolean
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), {
  as: 'section',
  padding: UiSectionPadding.Md,
  tone: UiSectionTone.Plain,
  contained: true,
})

/** Padding utility per step. */
const PADDING_CLASSES: Record<UiSectionPadding, string> = {
  [UiSectionPadding.Sm]: 'py-8',
  [UiSectionPadding.Md]: 'py-12 md:py-16',
  [UiSectionPadding.Lg]: 'py-16 md:py-24',
}

/** Class list of the root element. */
const classes = computed(() => [
  PADDING_CLASSES[props.padding as UiSectionPadding],
  props.tone === UiSectionTone.Muted ? 'bg-gray-50 dark:bg-white/[0.02]' : '',
].filter(Boolean).join(' '))
</script>
