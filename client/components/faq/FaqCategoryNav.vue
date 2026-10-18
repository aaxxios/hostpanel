<template>
  <UiStack
    direction="row"
    wrap
    justify="center"
    gap="sm"
    data-testid="faq-category-nav"
  >
    <UiButton
      v-if="showAll"
      :to="localePath('/faq')"
      variant="subtle"
      size="sm"
      data-testid="faq-category-all"
    >
      {{ t('faq.categories.all') }}
    </UiButton>
    <UiButton
      v-for="category in categories"
      :key="category.value"
      :to="localePath(`/faq/${category.value}`)"
      :variant="category.value === active ? 'primary' : 'subtle'"
      size="sm"
      :data-testid="`faq-category-${category.value}`"
    >
      {{ category.label }}
    </UiButton>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * FaqCategoryNav - the row of links to the FAQ topic pages.
 *
 * Real links rather than client-side tabs, so every topic page is reachable (and crawlable) from
 * the FAQ landing page. On a topic page the current topic is highlighted and an "All" link leads
 * back to the landing page.
 */
import { FaqCategory } from '~/types/faqcategory'

/** Props of FaqCategoryNav. */
interface Props {
  /** The topic whose page is open; it is drawn as the selected link. */
  active?: FaqCategory
  /** Adds the leading "All" link to the landing page. */
  showAll?: boolean
}

withDefaults(defineProps<Props>(), {
  active: undefined,
  showAll: false
})

/** Translates a key in the active language. */
const { t } = useI18n()

/** Localised path builder from the i18n module. */
const localePath = useLocalePath()

/** Every topic with its label in the active language. */
const categories = computed(() => Object.values(FaqCategory).map(value => ({
  value,
  label: t(`faq.categories.${value}`)
})))
</script>
