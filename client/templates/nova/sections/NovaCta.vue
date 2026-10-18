<template>
  <UiButton
    variant="bare"
    :to="to"
    :href="href"
    :full-width="fullWidth"
    :class="classes"
    data-testid="nova-cta"
  >
    <slot />
  </UiButton>
</template>

<script setup lang="ts">
/**
 * nova call-to-action link.
 *
 * A bare `UiButton` (structure and focus ring only) painted with nova's own tokens, so a button
 * looks the same wherever a section, the header or a pricing card needs one. It renders a route
 * link for `to` and an anchor for `href`; `data-testid` and `@click` fall through to the element.
 */
import { computed } from 'vue'
import { NovaCtaSize, NovaCtaTone } from '~/templates/nova/enums'

/** Props of NovaCta. */
interface Props {
  /** In-app route (already locale-prefixed); renders a route link. */
  to?: string
  /** Plain URL; renders an anchor. Used where the router must not take the click. */
  href?: string
  /** Look of the link. */
  tone?: `${NovaCtaTone}`
  /** Size step. */
  size?: `${NovaCtaSize}`
  /** Lifts the link a few pixels on hover (motion-safe only). */
  lift?: boolean
  /** Stretches the link to the width of its container. */
  fullWidth?: boolean
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), {
  to: undefined,
  href: undefined,
  tone: NovaCtaTone.Accent,
  size: NovaCtaSize.Md,
  lift: false,
  fullWidth: false,
})

/** Colours per tone, written as whole literals so Tailwind emits them. */
const TONE_CLASSES: Record<NovaCtaTone, string> = {
  [NovaCtaTone.Accent]: 'bg-nova-accent font-bold text-nova-on-accent hover:brightness-95',
  [NovaCtaTone.Outline]: 'border border-nova-border font-semibold text-nova-ink hover:border-nova-brand hover:text-nova-brand',
  [NovaCtaTone.Quiet]: 'font-medium text-nova-muted hover:text-nova-ink',
}

/** Height, padding and type size per step. */
const SIZE_CLASSES: Record<NovaCtaSize, string> = {
  [NovaCtaSize.Lg]: 'min-h-[52px] px-7 text-base',
  [NovaCtaSize.Md]: 'min-h-[48px] px-6 text-[15px]',
  [NovaCtaSize.Card]: 'min-h-[48px] px-5 text-[15px]',
  [NovaCtaSize.Bar]: 'min-h-[44px] text-[15px]',
}

/** Class list of the link. */
const classes = computed(() => [
  'rounded-xl',
  TONE_CLASSES[props.tone as NovaCtaTone],
  SIZE_CLASSES[props.size as NovaCtaSize],
  props.lift ? 'motion-safe:hover:-translate-y-0.5' : '',
].filter(Boolean).join(' '))
</script>
