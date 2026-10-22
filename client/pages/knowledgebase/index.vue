<template>
  <UiStack gap="none" data-testid="kb-page">
    <!-- Hero / Search -->
    <UiPageHero
      :badge="t('kb.badge')"
      :title="t('kb.title')"
      :subtitle="t('kb.subtitle')"
    >
      <!-- Search bar -->
      <UiForm
        spacing="none"
        class="mx-auto mt-10 max-w-3xl"
        data-testid="kb-search-form"
        @submit="doSearch"
      >
        <UiStack direction="row" gap="xs">
          <UiInput
            v-model="searchQuery"
            class="flex-1"
            :placeholder="t('kb.searchPlaceholder')"
            testid="kb-search-input"
          >
            <template #prefix>
              <Search :size="16" :stroke-width="2" class="text-gray-400" />
            </template>
          </UiInput>
          <UiButton type="submit" size="lg" data-testid="kb-search-submit">
            <Search :size="16" :stroke-width="2" class="mr-2" />
            {{ t('kb.search') }}
          </UiButton>
        </UiStack>
      </UiForm>
    </UiPageHero>

    <!-- Content -->
    <UiSection padding="md">
      <UiStack gap="xl" class="mx-auto w-full max-w-5xl">
        <!-- Search results -->
        <template v-if="searched">
          <UiStack direction="row" justify="between" align="center">
            <UiHeading :level="2" size="subsection" data-testid="kb-search-count">
              {{ searchResults.length }} {{ t('kb.articles') }}
            </UiHeading>
            <UiButton
              variant="ghost"
              size="sm"
              data-testid="kb-clear-search"
              @click="clearSearch"
            >
              <X :size="14" :stroke-width="2" class="mr-1.5" />
              {{ t('kb.backToKb') }}
            </UiButton>
          </UiStack>

          <UiStack v-if="searchPending" gap="sm" data-testid="kb-search-loading">
            <UiSkeleton v-for="i in 5" :key="i" shape="block" class="h-20" />
          </UiStack>

          <UiEmptyState
            v-else-if="!searchResults.length"
            :icon="Search"
            :title="t('kb.noResults')"
            data-testid="kb-search-empty"
          />

          <UiCard
            v-else
            padding="none"
            class="overflow-hidden"
            data-testid="kb-search-results"
          >
            <UiStack gap="none" class="divide-y divide-gray-100 dark:divide-white/5 [.tpl-classic_&]:divide-classic-border">
              <KbArticleRow v-for="article in searchResults" :key="article.id" :article="article" />
            </UiStack>
          </UiCard>
        </template>

        <!-- Default: categories + popular -->
        <template v-else>
          <!-- Categories grid -->
          <UiStack gap="lg" data-testid="kb-categories">
            <UiHeading :level="2" size="subsection" class="flex items-center gap-2">
              <FolderOpen :size="20" :stroke-width="2" class="text-primary-700 dark:text-primary-400 [.tpl-classic_&]:text-classic-brand" />
              {{ t('kb.categories') }}
            </UiHeading>

            <UiGrid v-if="catPending" cols="2">
              <UiSkeleton v-for="i in 4" :key="i" shape="block" class="h-24" />
            </UiGrid>

            <UiText v-else-if="!categories.length" tone="muted" size="sm">
              {{ t('kb.empty') }}
            </UiText>

            <UiGrid v-else cols="2">
              <UiLinkCard
                v-for="cat in categories"
                :key="cat.id"
                :to="localePath(`/knowledgebase/category/${cat.id}`)"
                :testid="`kb-category-${cat.id}`"
              >
                <UiStack direction="row" justify="between" align="center">
                  <UiStack direction="row" align="center" gap="sm">
                    <UiTile
                      :icon="Folder"
                      size="xl"
                      class="border border-primary-500/20 bg-primary-500/10 text-primary-700 dark:text-primary-300 [.tpl-classic_&]:rounded-md [.tpl-classic_&]:border-classic-border [.tpl-classic_&]:bg-classic-brand-soft [.tpl-classic_&]:text-classic-brand"
                    />
                    <UiText
                      as="span"
                      weight="semibold"
                      class="transition-colors group-hover:text-primary-600 dark:group-hover:text-primary-400 group-hover:[.tpl-classic_&]:text-classic-brand"
                    >
                      {{ cat.name }}
                    </UiText>
                  </UiStack>
                  <UiBadge variant="primary" size="sm" rounded class="[.tpl-classic_&]:rounded-md [.tpl-classic_&]:bg-classic-brand-soft [.tpl-classic_&]:text-classic-brand">
                    {{ cat.articlecount }} {{ cat.articlecount === 1 ? t('kb.article') : t('kb.articles') }}
                  </UiBadge>
                </UiStack>
              </UiLinkCard>
            </UiGrid>
          </UiStack>

          <!-- Most popular articles -->
          <UiStack gap="lg" data-testid="kb-popular">
            <UiHeading :level="2" size="subsection" class="flex items-center gap-2">
              <Star :size="20" :stroke-width="2" class="text-yellow-600 dark:text-yellow-400" />
              {{ t('kb.mostPopular') }}
            </UiHeading>

            <UiStack v-if="artPending" gap="sm">
              <UiSkeleton v-for="i in 5" :key="i" shape="block" class="h-20" />
            </UiStack>

            <UiText v-else-if="!popularArticles.length" tone="muted" size="sm">
              {{ t('kb.empty') }}
            </UiText>

            <UiCard v-else padding="none" class="overflow-hidden">
              <UiStack gap="none" class="divide-y divide-gray-100 dark:divide-white/5 [.tpl-classic_&]:divide-classic-border">
                <KbArticleRow v-for="article in popularArticles" :key="article.id" :article="article" />
              </UiStack>
            </UiCard>
          </UiStack>
        </template>
      </UiStack>
    </UiSection>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * Public knowledgebase index page.
 * Shows categories grid + most popular articles.
 * Supports live search via query param.
 */

import { Search, FolderOpen, Folder, Star, X } from 'lucide-vue-next'
import { useContentStore } from '~/stores/content'
import type { KbArticle } from '~/types/kbarticle'
import type { KbCategory } from '~/types/kbcategory'

/** Localised path builder from the i18n module. */
const localePath = useLocalePath()
/** The current route. */
const route = useRoute()
/** Router, used to keep the search in the URL. */
const router = useRouter()
/** Translates a key in the active language; `locale` is that language. */
const { t, locale } = useI18n()

useSeoMeta({
  title: t('kb.title'),
  description: t('kb.subtitle')
})

// Categories and popular articles, read through the content store; a language change reads them again.
/** The content store that owns the lists this page renders. */
const content = useContentStore()
/** Whether the landing read is still running. */
const { pending: landingPending } = await useAsyncData(
  'kb:landing',
  () => Promise.all([content.loadKbCategories(), content.loadPopularArticles()]),
  { watch: [locale] }
)

/** The categories grid and the popular list are loaded together, so they share one pending flag. */
const catPending = landingPending

/** Same flag for the popular list. */
const artPending = landingPending

/** All top-level categories */
const categories = computed<KbCategory[]>(() => content.kbCategories)

/** Popular articles (sorted by views, no category filter) */
const popularArticles = computed<KbArticle[]>(() => content.popularArticles)

// ── Search ──────────────────────────────────────────────────────────────────

/** The text in the search box; starts from `?q=` so a shared link repeats the search. */
const searchQuery = ref(String(route.query.q ?? ''))

/** Whether the results view (rather than the landing view) is showing. */
const searched = ref(!!route.query.q)

/** Whether a search is in flight. */
const searchPending = computed(() => content.searching)

/** The articles the last search found. */
const searchResults = computed<KbArticle[]>(() => content.searchResults)

/** Clear search and return to default view */
const clearSearch = (): void => {
  searchQuery.value = ''
  searched.value = false
  content.clearSearch()
  router.replace({ query: {} })
}

/** Run search and update URL query string */
const doSearch = async (): Promise<void> => {
  if (!searchQuery.value.trim()) {
    clearSearch()
    return
  }
  searched.value = true
  router.replace({ query: { q: searchQuery.value } })
  await content.searchArticles(searchQuery.value)
}

// Re-run search if page is loaded with ?q= param
onMounted(async () => {
  if (route.query.q) {
    await content.searchArticles(searchQuery.value)
  }
})
</script>
