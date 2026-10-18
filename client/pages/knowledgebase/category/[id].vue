<template>
  <UiSection padding="lg" data-testid="kb-category-page">
    <UiStack class="mx-auto w-full max-w-4xl" gap="lg">
      <!-- Back link -->
      <UiLink
        :to="localePath('/knowledgebase')"
        tone="muted"
        :underline="false"
        class="inline-flex items-center gap-2 self-start text-sm"
        testid="kb-back"
      >
        <ArrowLeft :size="15" :stroke-width="2" />
        {{ t('kb.backToKb') }}
      </UiLink>

      <!-- Category header -->
      <UiStack direction="row" align="center" gap="md">
        <UiTile
          :icon="Folder"
          size="xl"
          class="border border-primary-500/20 bg-primary-500/10 text-primary-600 dark:text-primary-400"
        />
        <UiStack gap="none">
          <UiHeading :level="1" size="section" data-testid="kb-category-heading">
            {{ categoryName }}
          </UiHeading>
          <UiText size="sm" tone="subtle" data-testid="kb-category-count">
            {{ articles.length }} {{ t('kb.articles') }}
          </UiText>
        </UiStack>
      </UiStack>

      <!-- Loading -->
      <UiStack v-if="pending" gap="sm" data-testid="kb-category-loading">
        <UiSkeleton v-for="i in 6" :key="i" shape="block" class="h-16" />
      </UiStack>

      <!-- Empty -->
      <UiEmptyState
        v-else-if="!articles.length"
        :icon="FileText"
        :title="t('kb.empty')"
        data-testid="kb-category-empty"
      />

      <!-- Articles list -->
      <UiCard
        v-else
        padding="none"
        class="overflow-hidden"
        data-testid="kb-category-articles"
      >
        <UiStack gap="none" class="divide-y divide-gray-100 dark:divide-white/5">
          <KbArticleRow v-for="article in articles" :key="article.id" :article="article" />
        </UiStack>
      </UiCard>
    </UiStack>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * Knowledgebase category page.
 * Lists all articles in the given category.
 */

import { ArrowLeft, Folder, FileText } from 'lucide-vue-next'
import { useContentStore } from '~/stores/content'
import type { KbArticle } from '~/types/kbarticle'

/** The current route. */
const route = useRoute()

/** Localised path builder from the i18n module. */
const localePath = useLocalePath()
/** Translates a key in the active language; `locale` is that language. */
const { t, locale } = useI18n()

/** Id of the category the route names. */
const categoryId = computed(() => String(route.params.id))

// Articles and category names, read through the content store; the key follows the route, so a
// new category is read, and a language change reads it again.
/** The content store that owns the lists this page renders. */
const content = useContentStore()

/** Fetch articles for this category, and the categories to get its name */
const { pending } = await useAsyncData(
  () => `kb:category:${categoryId.value}`,
  () => Promise.all([content.loadCategoryArticles(categoryId.value), content.loadKbCategories()]),
  { watch: [locale] }
)

/** The articles of this category. */
const articles = computed<KbArticle[]>(() => content.articlesFor(categoryId.value))

/** The category's name, or the generic "Categories" label until the store knows it. */
const categoryName = computed(() => content.categoryName(categoryId.value) ?? t('kb.categories'))

watchEffect(() => {
  if (categoryName.value) {
    useSeoMeta({ title: `${categoryName.value} — ${t('kb.title')}` })
  }
})
</script>
