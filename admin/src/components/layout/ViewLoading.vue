<script setup lang="ts">
/**
 * Stand-in shown in place of a screen whose code is still downloading.
 *
 * The router switches to the screen at once; this keeps the content area from sitting on the old
 * page, or on nothing, until the chunk arrives. The screen's own skeleton takes over from it.
 */
import { useI18n } from 'vue-i18n'
import UiSkeleton from '@/components/ui/UiSkeleton.vue'
import UiStack from '@/components/ui/UiStack.vue'
import UiText from '@/components/ui/UiText.vue'
import { UiSkeletonShape } from '@/utils/ui/skeleton'

/** Translation function. */
const { t } = useI18n()
</script>

<template>
  <UiStack gap="md" role="status" aria-busy="true" data-testid="view-loading">
    <UiSkeleton :lines="1" class="w-1/3" />
    <UiSkeleton :shape="UiSkeletonShape.Block" />
    <UiSkeleton :lines="3" />
    <UiText tone="secondary" size="md">
      {{ t('common.loading') }}
    </UiText>
  </UiStack>
</template>
