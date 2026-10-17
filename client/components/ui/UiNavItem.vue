<template>
  <component
    :is="tag"
    v-bind="tagAttrs"
    :data-testid="testid"
    :class="classes"
  >
    <Loader2 v-if="loading" :size="15" :stroke-width="2" class="flex-shrink-0 animate-spin" />
    <component :is="icon" v-else-if="icon" :size="15" :stroke-width="2" class="flex-shrink-0" />
    <slot />
  </component>
</template>

<script setup lang="ts">
/**
 * UiNavItem - one entry of a UiNavGroup.
 *
 * Renders a NuxtLink (`to`), an anchor that opens in a new tab (`href`) or a button (neither).
 * `active` marks the current panel; `tone="warning"` highlights an entry that asks for action.
 */
import { computed, resolveComponent } from 'vue'
import type { Component } from 'vue'
import { Loader2 } from 'lucide-vue-next'
import { UiNavTone } from '~/utils/ui/enums'

/** Props of UiNavItem. */
interface Props {
  /** A lucide icon component. */
  icon?: Component
  /** Marks the entry as the current one. */
  active?: boolean
  /** Disables the entry. */
  disabled?: boolean
  /** Shows a spinner in place of the icon and disables the entry. */
  loading?: boolean
  /** In-app route; renders a NuxtLink. */
  to?: string
  /** External URL; renders an anchor that opens in a new tab. */
  href?: string
  /** Colour behaviour. */
  tone?: `${UiNavTone}`
  /** `data-testid` of the entry. */
  testid?: string
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), {
  icon: undefined,
  active: false,
  disabled: false,
  loading: false,
  to: undefined,
  href: undefined,
  tone: UiNavTone.Default,
  testid: undefined,
})

/** NuxtLink for a route, an anchor for a URL, a button otherwise. */
const tag = computed(() => {
  if (props.to) {
    return resolveComponent('NuxtLink')
  }
  return props.href ? 'a' : 'button'
})

/** Attributes that belong to the chosen element. */
const tagAttrs = computed(() => {
  if (props.to) {
    return { to: props.to }
  }
  if (props.href) {
    return { href: props.href, target: '_blank', rel: 'noopener noreferrer' }
  }
  return { type: 'button', disabled: props.disabled || props.loading }
})

/** Class list of the entry. */
const classes = computed(() => {
  const base = 'w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm transition-colors text-left'
  if (props.active) {
    return `${base} bg-primary-50 dark:bg-primary-500/10 text-primary-600 dark:text-primary-400 font-medium`
  }
  if (props.disabled) {
    return `${base} text-gray-300 dark:text-gray-600 cursor-not-allowed`
  }
  if (props.tone === UiNavTone.Warning) {
    return `${base} text-yellow-600 dark:text-yellow-400 hover:bg-yellow-50 dark:hover:bg-yellow-500/10 font-medium`
  }
  return `${base} text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-white/5 disabled:opacity-60 disabled:cursor-wait`
})
</script>
