<template>
  <UiStack gap="none" data-testid="faq-category-page">
    <UiPageHero
      :badge="t('faq.badge')"
      :title="t(`seo.faqCategory.${category}.h1`)"
      :subtitle="t(`seo.faqCategory.${category}.description`)"
    >
      <template #breadcrumbs>
        <UiBreadcrumbs
          :items="breadcrumbs"
          container-class="mb-6 justify-center"
        />
      </template>

      <!-- Other categories -->
      <FaqCategoryNav :active="category" show-all class="mt-10" />
    </UiPageHero>

    <!-- FAQs for this category -->
    <UiSection padding="lg">
      <UiStack class="mx-auto w-full max-w-3xl" gap="xl">
        <!-- Empty state -->
        <UiEmptyState
          v-if="categoryFaqs.length === 0"
          :title="t('faq.noResults')"
          data-testid="faq-empty"
        >
          <UiButton :to="localePath('/faq')" variant="ghost" data-testid="faq-view-all">
            {{ t('faq.viewAll') }}
          </UiButton>
        </UiEmptyState>

        <!-- FAQ accordion -->
        <UiAccordion
          v-else
          :items="accordionItems"
          :default-open="[0]"
          data-testid="faq-accordion"
        />

        <!-- Back to all FAQs -->
        <UiStack align="center">
          <UiButton
            :to="localePath('/faq')"
            variant="subtle"
            size="lg"
            data-testid="faq-back-to-all"
          >
            <ArrowLeft :size="16" :stroke-width="2" class="mr-2" />
            {{ t('faq.backToAll') }}
          </UiButton>
        </UiStack>
      </UiStack>
    </UiSection>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * FAQ topic page: the questions of one topic as an accordion, with its own SEO tags and
 * structured data. An unknown topic is a 404.
 */
import { ArrowLeft } from 'lucide-vue-next'
import { faqs as rawFaqs } from '~/lib/data'
import { FaqCategory } from '~/types/faqcategory'
import { SeoPageType } from '~/types/seopagetype'

/** The current route. */
const route = useRoute()
/** Translates a key in the active language. */
const { t } = useI18n()

/** Operator-set site name that the SEO copy interpolates as `{site}`. */
const { name: siteName } = useSiteIdentity()

/** Localised path builder from the i18n module. */
const localePath = useLocalePath()

/** The topic named by the route. */
const category = computed(() => route.params.category as FaqCategory)

// 404 if the route names a topic that does not exist
if (!Object.values(FaqCategory).includes(category.value)) {
  throw createError({ statusCode: 404, statusMessage: 'FAQ category not found' })
}

/** The trail above the title: the FAQ landing page, then this topic. */
const breadcrumbs = computed(() => [
  { label: t('faq.title'), to: localePath('/faq') },
  { label: t(`faq.categories.${category.value}`) }
])

/** The questions of this topic with their wording in the active language. */
const categoryFaqs = computed(() =>
  rawFaqs
    .filter(faq => faq.category === category.value)
    .map(faq => ({
      ...faq,
      question: t(`faq.items.${faq.id}.question`),
      answer: t(`faq.items.${faq.id}.answer`)
    }))
)

/** The questions in the shape the accordion takes. */
const accordionItems = computed(() => categoryFaqs.value.map(faq => ({
  title: faq.question,
  content: faq.answer
})))

// Per-category SEO
useSeo({
  title: t(`seo.faqCategory.${category.value}.title`, { site: siteName.value }),
  description: t(`seo.faqCategory.${category.value}.description`, { site: siteName.value }),
  keywords: t(`seo.faqCategory.${category.value}.keywords`, { site: siteName.value }),
  type: SeoPageType.Website,
  path: `/faq/${category.value}`
})

// FAQ schema.org for this category only
/** Builder and publisher of the FAQ structured data. */
const { faqSchema, injectSchema } = useSchemaOrg()
injectSchema(faqSchema(categoryFaqs.value.map(faq => ({ question: faq.question, answer: faq.answer }))))
</script>
