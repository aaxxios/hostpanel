<script setup lang="ts">
/**
 * Edit and delete icon actions for one row or card of a support list.
 *
 * Edit is a link when `editTo` is given (the edit page is a route) and a button otherwise (the
 * edit happens in place). Delete always only asks: the page opens its own confirmation dialog.
 * `testidPrefix` names both controls (`<prefix>-edit`, `<prefix>-delete`).
 */
import { useI18n } from 'vue-i18n'
import type { RouteLocationRaw } from 'vue-router'
import UiIcon from '@/components/ui/UiIcon.vue'
import UiIconButton from '@/components/ui/UiIconButton.vue'
import UiLink from '@/components/ui/UiLink.vue'
import { Icon } from '@/lib/icons'
import { UiButtonVariant } from '@/utils/ui/button'
import { UiSize } from '@/utils/ui/size'

/** Props for SupportRowActions. */
withDefaults(defineProps<{
  /** Prefix of the two controls' `data-testid`s. */
  testidPrefix: string
  /** Route of the edit page; omit when the edit happens in place. */
  editTo?: RouteLocationRaw
}>(), {
  editTo: undefined,
})

/** Events of SupportRowActions. */
const emit = defineEmits<{
  /** The in-place edit was asked for (only when there is no `editTo`). */
  edit: []
  /** The delete was asked for. */
  delete: []
}>()

const { t } = useI18n()
</script>

<template>
  <span class="inline-flex items-center gap-1">
    <UiLink
      v-if="editTo !== undefined"
      :to="editTo"
      :variant="UiButtonVariant.Ghost"
      :size="UiSize.Sm"
      :aria-label="t('shared.edit')"
      :title="t('shared.edit')"
      :data-testid="`${testidPrefix}-edit`"
    >
      <UiIcon :markup="Icon.Edit" :size="UiSize.Sm" />
    </UiLink>
    <UiIconButton
      v-else
      :label="t('shared.edit')"
      :size="UiSize.Sm"
      :data-testid="`${testidPrefix}-edit`"
      @click="emit('edit')"
    >
      <UiIcon :markup="Icon.Edit" :size="UiSize.Sm" />
    </UiIconButton>
    <UiIconButton
      :label="t('shared.delete')"
      :size="UiSize.Sm"
      :data-testid="`${testidPrefix}-delete`"
      @click="emit('delete')"
    >
      <UiIcon :markup="Icon.Trash" :size="UiSize.Sm" />
    </UiIconButton>
  </span>
</template>
