<template>
  <UiSection padding="lg" data-testid="announcement-page">
    <UiStack class="mx-auto w-full max-w-3xl" gap="lg">
      <!-- Back link -->
      <UiLink
        :to="localePath('/announcements')"
        tone="muted"
        :underline="false"
        class="inline-flex items-center gap-2 self-start text-sm"
        testid="announcement-back"
      >
        <ArrowLeft :size="15" :stroke-width="2" />
        {{ t('announcements.backToList') }}
      </UiLink>

      <!-- Loading -->
      <UiStack v-if="pending" gap="md" data-testid="announcement-loading">
        <UiSkeleton shape="block" class="h-8 w-2/3" />
        <UiSkeleton class="w-1/4" />
        <UiSkeleton :lines="6" class="mt-8" />
      </UiStack>

      <!-- Error / not found -->
      <UiEmptyState
        v-else-if="error || !item"
        :icon="Megaphone"
        :title="t('announcements.errorDesc')"
        data-testid="announcement-error"
      />

      <!-- Content -->
      <UiStack v-else as="article" gap="lg">
        <!-- Date -->
        <UiText
          as="span"
          size="xs"
          tone="muted"
          class="flex items-center gap-2"
          data-testid="announcement-date"
        >
          <Calendar :size="13" :stroke-width="2" />
          {{ t('announcements.published') }}: {{ formatDate(item.date, locale) }}
        </UiText>

        <!-- Title -->
        <UiHeading :level="1" data-testid="announcement-heading">
          {{ item.title }}
        </UiHeading>

        <UiDivider />

        <!-- Body (HTML from the API, rebuilt from an allow-list of tags) -->
        <UiRichText :html="item.body" />
      </UiStack>
    </UiStack>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * Public announcement detail page.
 * Loads a single announcement by id from the API and renders the full HTML body.
 */

import { ArrowLeft, Calendar, Megaphone } from 'lucide-vue-next'
import { useContentStore } from '~/stores/content'
import { formatDate } from '~/utils/formatDate'
import type { Announcement } from '~/types/announcement'

/** The reader's language, so the publication date is written the way they read dates. */
const { t, locale } = useI18n()

/** The current route. */
const route = useRoute()

/** Localised path builder from the i18n module. */
const localePath = useLocalePath()

/** Fetch full announcements list (through the content store) and find the one matching the route id */
const content = useContentStore()
/** Whether the read is still running. */
const { pending } = await useAsyncData('announcements:list', () => content.loadAnnouncements(), { watch: [locale] })

/** The sentence explaining why the announcement could not be read, or `''`. */
const error = computed(() => content.error)

/** The announcement the route names, once the store has it. */
const item = computed<Announcement | null>(() => content.announcementFor(String(route.params.id)))

watchEffect(() => {
  if (item.value) {
    useSeoMeta({
      title: item.value.title,
      description: item.value.excerpt
    })
  }
})
</script>
