<template>
  <section v-if="faqs.length" class="py-20 bg-ui-surface relative overflow-hidden" data-testid="sections-faq">
    <!-- Background -->
    <UiDecor class="inset-0">
      <UiDecor class="inset-0 bg-gradient-to-br from-primary-500/5 via-transparent to-primary-500/5 dark:to-secondary-500/5" />
      <UiDecor class="top-1/4 right-1/4 w-96 h-96 bg-primary-500/15 rounded-full blur-[140px] animate-blob" />
      <UiDecor class="bottom-1/3 left-1/3 w-80 h-80 bg-primary-500/10 dark:bg-secondary-500/10 rounded-full blur-[130px] animate-blob animation-delay-2000" />
      <UiDecor class="top-20 left-1/4 w-2 h-2 bg-primary-400/60 rounded-full animate-float [animation-delay:0.6s]" />
      <UiDecor class="top-1/3 right-1/3 w-3 h-3 bg-primary-400/50 dark:bg-secondary-400/50 rounded-full animate-float [animation-delay:1.4s]" />
      <UiDecor class="bottom-1/4 left-1/2 w-2 h-2 bg-cyan-400/60 rounded-full animate-float [animation-delay:2.1s]" />
      <UiDecor class="inset-0 opacity-[0.015] pattern-grid-50" />
      <UiDecor class="top-0 left-1/3 w-px h-full bg-gradient-to-b from-transparent via-primary-500/10 to-transparent" />
      <UiDecor class="top-0 right-1/3 w-px h-full bg-gradient-to-b from-transparent via-primary-500/10 dark:via-secondary-500/10 to-transparent" />
    </UiDecor>

    <UiContainer class="relative z-10">
      <MarketingSectionHeader
        v-if="showHeader"
        class="mb-12"
        :badge="$t('faq.badge')"
        :title="$t('faq.title')"
        :subtitle="$t('faq.subtitle')"
      />

      <!-- Category tabs -->
      <div v-if="showCategories" class="flex flex-wrap justify-center gap-2 mb-12">
        <UiButton
          v-for="category in categories"
          :key="category.value"
          :variant="activeCategory === category.value ? 'primary' : 'subtle'"
          :data-testid="`faq-category-${category.slug}`"
          @click="activeCategory = category.value"
        >
          {{ category.label }}
        </UiButton>
      </div>

      <div class="max-w-3xl mx-auto">
        <!-- Questions -->
        <UiAccordion
          :key="activeCategory"
          class="mb-12"
          :items="accordionItems"
          :default-open="categoryChosen ? [0] : []"
          keep-content
          data-testid="faq-list"
        />

        <!-- View all FAQs (when limited) -->
        <div v-if="limit" class="text-center mb-8">
          <UiButton variant="outline" size="lg" :to="localePath('/faq')" data-testid="faq-view-all">
            <span>{{ $t('faq.viewAll') }}</span>
            <ArrowRight :size="18" :stroke-width="2" class="ml-2" />
          </UiButton>
        </div>

        <!-- Contact card -->
        <UiCard class="relative text-center overflow-hidden group hover:border-primary-500/50 transition-all duration-300" padding="xl" data-testid="faq-contact">
          <UiDecor class="inset-0 bg-gradient-to-br from-primary-500/20 to-primary-500/10 dark:to-secondary-500/10" />
          <UiDecor class="inset-0 bg-gradient-to-br from-primary-500/10 to-primary-500/5 dark:to-secondary-500/5 opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
          <UiDecor class="top-0 right-0 w-32 h-32 bg-primary-500/10 rounded-full blur-3xl" />
          <UiDecor class="bottom-0 left-0 w-24 h-24 bg-primary-500/10 dark:bg-secondary-500/10 rounded-full blur-2xl" />

          <MessageCircle :size="48" :stroke-width="2" class="relative text-primary-600 dark:text-primary-400 mx-auto mb-4 group-hover:scale-110 group-hover:rotate-6 transition-transform duration-300" />
          <UiHeading :level="3" size="subsection" class="relative text-xl mb-2">{{ $t('faq.cta.title') }}</UiHeading>
          <UiText tone="muted" size="sm" class="relative mb-6 max-w-md mx-auto">{{ $t('faq.cta.description') }}</UiText>
          <UiButton class="relative hover:shadow-lg hover:shadow-primary-500/50 transition-all duration-300" :to="localePath('/contact')" data-testid="faq-contact-button">
            <MessageSquare :size="18" :stroke-width="2" class="mr-2" />
            {{ $t('faq.cta.button') }}
          </UiButton>

          <UiDecor class="top-0 right-0 w-14 h-14 border-r-2 border-t-2 border-primary-500/50 rounded-tr-2xl opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
        </UiCard>
      </div>
    </UiContainer>

    <!-- Section corner decorations -->
    <UiDecor class="top-0 right-0 w-40 h-40 border-r-2 border-t-2 border-primary-500/25 dark:border-secondary-500/25" />
    <UiDecor class="bottom-0 left-0 w-40 h-40 border-l-2 border-b-2 border-primary-500/25" />
  </section>
</template>

<script setup lang="ts">
/**
 * FAQ section with category filtering.
 *
 * The questions come from the storefront store (`GET /api/portal/public/faqs`, localised by the API); with
 * none the section renders nothing. Choosing a
 * category reloads the list and opens its first question; before any choice every question is
 * closed. With a `limit` the section is a preview that links to the full FAQ page.
 */
import { ArrowRight, MessageCircle, MessageSquare } from 'lucide-vue-next'
import { useStorefrontStore } from '~/stores/storefront'
import { FaqCategory } from '~/types/faqcategory'
import { faqCategorySlug } from '~/utils/faqCategory'

/** Props of SectionsFAQ. */
interface Props {
  /** Limit number of FAQs to show (for the home page preview). */
  limit?: number
  /** Show category tabs. */
  showCategories?: boolean
  /** Show the section header. */
  showHeader?: boolean
}

/** Props of the component with their defaults applied. */
const props = withDefaults(defineProps<Props>(), {
  limit: undefined,
  showCategories: true,
  showHeader: true,
})

/** The "every category" choice of the tabs. */
const ALL_CATEGORIES = 'all'

/** The choice a tab stands for: a topic or all of them. */
type CategoryChoice = FaqCategory | typeof ALL_CATEGORIES

/** Translation helpers and the active language. */
const { t, locale } = useI18n()
/** Prefixes a path with the active language. */
const localePath = useLocalePath()

/** The storefront content: testimonials, partners, projects and FAQs. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:faqs', () => storefront.loadFaqs(), { watch: [locale] })

/** Every question, in display order. */
const faqs = computed(() => storefront.faqs)

/** The tabs: all, then one per topic; `slug` spells the locale key and the test id. */
const categories = computed(() =>
  ([ALL_CATEGORIES, ...Object.values(FaqCategory)] satisfies CategoryChoice[]).map((value) => {
    const slug = value === ALL_CATEGORIES ? ALL_CATEGORIES : faqCategorySlug(value)
    return {
      value,
      slug,
      label: t(`faq.categories.${slug}`),
    }
  }))

/** The category chosen in the tabs. */
const activeCategory = ref<CategoryChoice>(ALL_CATEGORIES)

/** Whether the visitor has chosen a category yet; the first question opens only after that. */
const categoryChosen = ref(false)

watch(activeCategory, () => {
  categoryChosen.value = true
})

/** The questions of the active category, cut to `limit` when one is set, with their translated text. */
const accordionItems = computed(() => {
  let result = faqs.value.filter((faq) => activeCategory.value === ALL_CATEGORIES || faq.category === activeCategory.value)
  if (props.limit) {
    result = result.slice(0, props.limit)
  }
  return result.map((faq) => ({
    title: faq.question,
    content: faq.answer,
  }))
})
</script>
