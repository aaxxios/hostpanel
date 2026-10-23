<template>
  <UiStack gap="xs" align="center" class="text-center" data-testid="product-section-header">
    <UiHeading
      :level="UiHeadingLevel.H2"
      size="bare"
      class="text-3xl font-bold tracking-tight text-gray-900 dark:text-white md:text-4xl [.tpl-classic_&]:text-classic-ink"
      data-testid="product-section-title"
    >
      {{ title }}
    </UiHeading>
    <UiText
      v-if="subtitle"
      size="lg"
      tone="muted"
      class="max-w-[75ch] [.tpl-classic_&]:text-classic-muted"
      data-testid="product-section-subtitle"
    >
      {{ subtitle }}
    </UiText>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * The heading every block of a product's detail page opens with: one centred title and an optional
 * lead line, at the same size in every template. Spacing below it is the caller's (`class="mb-8"`).
 */
import { UiHeadingLevel } from '~/utils/ui/enums'

/** Props of ProductSectionHeader. */
interface Props {
  /** The block's title. */
  title: string
  /** Lead line under the title. */
  subtitle?: string
}

withDefaults(defineProps<Props>(), {
  subtitle: undefined,
})
</script>
