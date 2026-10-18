<script setup lang="ts">
/**
 * Centered card shared by the sign-in, first-run setup and email verification screens.
 *
 * Draws the brand mark, the screen title and an optional subtitle; the screen's own content is the
 * default slot. It holds no state and no decoration of its own, so the three screens cannot drift.
 */
import { useI18n } from 'vue-i18n'
import UiCard from '@/components/ui/UiCard.vue'
import UiHeading from '@/components/ui/UiHeading.vue'
import UiImage from '@/components/ui/UiImage.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'

/** Props for AuthShell. */
defineProps<{
  /** Screen title (already translated). */
  title: string
  /** Line under the title (already translated). */
  subtitle?: string
}>()

/** Translation function. */
const { t } = useI18n()
</script>

<template>
  <UiStack align="center" justify="center" class="min-h-dvh bg-surface-base p-4">
    <UiCard padding="lg" class="w-full max-w-sm">
      <UiStack gap="lg">
        <UiStack direction="row" align="center" gap="sm">
          <UiImage src="/logo.svg" alt="" class="w-9 h-9" />
          <UiText tone="primary" size="lg" class="font-display font-bold">
            {{ t('shared.innovayse') }}
          </UiText>
        </UiStack>
        <UiStack gap="xs">
          <UiHeading :level="1" variant="page" data-testid="auth-title">
            {{ title }}
          </UiHeading>
          <UiText v-if="subtitle !== undefined" tone="secondary" size="md">
            {{ subtitle }}
          </UiText>
        </UiStack>
        <slot />
      </UiStack>
    </UiCard>
  </UiStack>
</template>
