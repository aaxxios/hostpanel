<template>
  <UiSection v-if="items.length" padding="lg" class="border-b border-nova-border bg-nova-surface" data-testid="nova-projects">
    <NovaSectionHeader :eyebrow="t('nova.projects.eyebrow')" :title="t('nova.projects.title')" />

    <UiBalancedGrid as="ul" :count="items.length" class="mt-10" data-testid="nova-projects-grid">
      <li v-for="project in items" :key="project.id" class="h-full" :data-testid="`project-${project.id}`">
        <UiCard padding="none" class="flex h-full flex-col overflow-hidden !border-nova-border !bg-nova-bg">
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
            class="h-44 w-full rounded-none bg-nova-brand text-4xl font-bold text-white"
          >
            {{ project.title.charAt(0) }}
          </UiTile>
          <UiStack gap="sm" class="flex-1 p-6">
            <UiHeading :level="3" size="bare" class="text-lg font-bold text-nova-ink">{{ project.title }}</UiHeading>
            <UiText v-if="project.summary" size="sm" tone="inherit" class="text-nova-muted">{{ project.summary }}</UiText>
            <UiLink
              v-if="project.url"
              :href="project.url"
              tone="inherit"
              :underline="false"
              :testid="`project-link-${project.id}`"
              class="font-semibold text-nova-brand"
            >
              {{ t('nova.projects.visit') }}
            </UiLink>
          </UiStack>
        </UiCard>
      </li>
    </UiBalancedGrid>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova projects grid: a card per portfolio project with its first image (a lettered tile when it
 * has none), title, summary and a link to the live site. The projects come from the storefront
 * store (`GET /api/public/projects`); with none the section renders nothing.
 */
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'
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
