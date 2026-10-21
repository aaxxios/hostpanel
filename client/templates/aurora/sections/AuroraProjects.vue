<template>
  <UiSection v-if="items.length" id="projects" padding="lg" :contained="false" class="relative border-t border-line" data-testid="aurora-projects">
    <UiStack gap="none">
      <UiText tone="inherit" size="inherit" :class="[EYEBROW_CLASS, 'text-ac3']">{{ t('aurora.projects.eyebrow') }}</UiText>
      <UiHeading :level="2" size="bare" :class="[SECTION_TITLE_CLASS, 'mt-3']">
        {{ t('aurora.projects.title') }}
      </UiHeading>
    </UiStack>

    <UiBalancedGrid :count="items.length" class="mt-9" data-testid="aurora-projects-grid">
      <UiCard
        v-for="project in items"
        :key="project.id"
        tone="bare"
        padding="none"
        :data-testid="`project-${project.id}`"
        class="overflow-hidden border-line2 bg-card-hi shadow-card transition-[border-color,transform] duration-200 hover:-translate-y-0.5 hover:border-ac1"
      >
        <UiImage
          v-if="project.imageUrls[0]"
          :src="project.imageUrls[0]"
          :alt="project.title"
          unoptimised
          class="h-44 w-full"
          :testid="`project-image-${project.id}`"
        />
        <UiTile
          v-else
          size="lg"
          :data-testid="`project-placeholder-${project.id}`"
          class="h-44 w-full rounded-none bg-brand font-display text-4xl font-bold text-on-brand"
        >
          {{ project.title.charAt(0) }}
        </UiTile>
        <UiStack gap="sm" class="p-6">
          <UiHeading :level="3" size="bare" class="font-display text-xl font-bold text-tx">{{ project.title }}</UiHeading>
          <UiText v-if="project.summary" size="sm" tone="inherit" class="text-mut2">{{ project.summary }}</UiText>
          <UiLink
            v-if="project.url"
            :href="project.url"
            tone="inherit"
            :underline="false"
            :testid="`project-link-${project.id}`"
            class="font-semibold text-ac1"
          >
            {{ t('aurora.projects.visit') }}
          </UiLink>
        </UiStack>
      </UiCard>
    </UiBalancedGrid>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * aurora projects grid: a card per portfolio project with its first image (a lettered tile when it
 * has none), title, summary and a link to the live site. The projects come from the storefront
 * store (`GET /api/public/projects`); with none the section renders nothing.
 */
import { EYEBROW_CLASS, SECTION_TITLE_CLASS } from '~/templates/aurora/typography'
import { useStorefrontStore } from '~/stores/storefront'

/** Translator and the active language. */
const { t, locale } = useI18n()

/** The storefront content: testimonials, partners, projects and FAQs. */
const storefront = useStorefrontStore()

// Not awaited: Nuxt still holds the server render for it, and the section stays synchronous.
useAsyncData('storefront:projects', () => storefront.loadProjects(), { watch: [locale] })

/** The projects, in display order. */
const items = computed(() => storefront.projects)
</script>
