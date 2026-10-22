<template>
  <component
    :is="item.url ? UiLink : UiStack"
    v-bind="item.url ? { href: item.url, tone: 'inherit', underline: false } : {}"
    class="group flex h-12 w-full items-center justify-center rounded-md px-2 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-current"
    :data-testid="`${testidPrefix}-${item.id}`"
  >
    <UiImage
      v-if="item.logoUrl"
      :src="item.logoUrl"
      :alt="item.name"
      unoptimised
      fit="contain"
      class="h-8 max-w-[13rem] w-auto opacity-60 grayscale transition duration-200 dark:brightness-125 group-hover:opacity-100 group-hover:grayscale-0 group-focus-visible:opacity-100 group-focus-visible:grayscale-0"
      :testid="`${testidPrefix}-logo-${item.id}`"
    />
    <UiText
      v-else
      as="span"
      tone="inherit"
      weight="semibold"
      class="whitespace-nowrap text-lg tracking-tight opacity-60 transition-opacity duration-200 group-hover:opacity-100 group-focus-visible:opacity-100"
    >{{ item.name }}</UiText>
  </component>
</template>

<script setup lang="ts">
/**
 * UiLogoWallItem - one cell of a `UiLogoWall`: the partner's logo, or its name as a plain
 * wordmark when it has none, linked to the partner's site when it has one.
 *
 * Muted (grey, lower opacity, a little brighter on a dark page) until hovered or focused, then
 * full colour. The wordmark takes the colour of the surrounding text, so the screen decides it.
 */
import UiLink from '~/components/ui/UiLink.vue'
import UiStack from '~/components/ui/UiStack.vue'
import type { LogoWallItem } from '~/utils/ui/logoWall'

/** Props of UiLogoWallItem. */
interface Props {
  /** The logo to draw. */
  item: LogoWallItem
  /** Prefix of the test ids: `<prefix>-<id>` for the cell, `<prefix>-logo-<id>` for the image. */
  testidPrefix: string
}

defineProps<Props>()
</script>
