<template>
  <UiStack direction="row" align="center" justify="between" gap="sm" :data-testid="testid">
    <UiText as="span" size="sm" tone="muted">{{ label }}</UiText>
    <slot>
      <UiText as="span" size="sm" weight="bold">{{ value }}</UiText>
    </slot>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * One label/value line of a summary card: the label muted on the left, the value bold on the
 * right. The default slot replaces the value when it needs more than text (a badge).
 */

/** Props of ClientSummaryRow. */
interface Props {
  /** What the line is about. */
  label: string
  /** The value as text; ignored when the default slot is used. */
  value?: string
  /** `data-testid` of the row. */
  testid?: string
}

withDefaults(defineProps<Props>(), {
  value: undefined,
  testid: undefined
})
</script>
