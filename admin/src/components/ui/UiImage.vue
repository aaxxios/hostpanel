<script setup lang="ts">
/**
 * Image.
 *
 * `fit` decides how the picture fills its box and `rounded` the corner radius. `alt` defaults to
 * empty (decorative); pass real text when the picture carries meaning. Other attributes
 * (`width`, `height`, classes) land on the native `<img>`.
 */
import { computed } from 'vue'
import type { UiImageFit, UiImageRounded } from '@/utils/ui/image'

/** Props for UiImage. */
const props = withDefaults(defineProps<{
  /** Address of the picture. */
  src?: string
  /** Alternative text; empty marks the picture decorative. */
  alt?: string
  /** How the picture fills its box. */
  fit?: UiImageFit
  /** Corner radius. */
  rounded?: UiImageRounded
}>(), {
  src: undefined,
  alt: '',
  fit: 'none',
  rounded: 'none',
})

/** Fit classes. */
const fitClasses: Record<UiImageFit, string> = {
  cover: 'object-cover',
  contain: 'object-contain',
  none: '',
}

/** Radius classes. */
const roundedClasses: Record<UiImageRounded, string> = {
  none: '',
  md: 'rounded-lg',
  lg: 'rounded-2xl',
  full: 'rounded-full',
}

/** Classes of the chosen fit and radius. */
const classes = computed<string[]>(() => [fitClasses[props.fit], roundedClasses[props.rounded]])
</script>

<template>
  <img :src="src" :alt="alt" :class="classes">
</template>
