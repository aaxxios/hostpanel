<template>
  <UiStack gap="none" data-testid="legal-document">
    <UiPageHero :badge="badge" :title="title" :subtitle="subtitle">
      <UiText
        v-if="meta"
        size="sm"
        tone="muted"
        class="mt-4"
        data-testid="legal-last-updated"
      >
        {{ meta }}
      </UiText>
    </UiPageHero>

    <UiSection padding="md">
      <UiStack gap="xl" class="mx-auto w-full max-w-4xl">
        <UiStack
          v-for="(section, index) in sections"
          :key="section.id"
          gap="md"
          :data-testid="`legal-section-${section.id}`"
          class="animate-reveal-up motion-reduce:animate-none"
          :class="revealDelay(index * 100)"
        >
          <UiStack direction="row" align="center" gap="md">
            <UiTile
              size="xl"
              class="border border-primary-500/20 bg-primary-500/10 text-primary-700 dark:text-primary-300"
              data-testid="legal-section-marker"
            >
              <Icon v-if="section.icon" :name="section.icon" size="22" />
              <template v-else>
                {{ index + 1 }}
              </template>
            </UiTile>
            <UiHeading :level="2">
              {{ section.title }}
            </UiHeading>
          </UiStack>

          <UiStack gap="md" class="md:pl-14">
            <UiText
              v-for="(paragraph, paragraphIndex) in section.paragraphs"
              :key="paragraphIndex"
              tone="muted"
              size="lg"
              class="leading-relaxed"
            >
              {{ paragraph }}
            </UiText>

            <UiList
              v-if="section.items?.length"
              :list-style="section.checklist ? 'plain' : 'marked'"
              data-testid="legal-section-items"
            >
              <UiListItem
                v-for="(item, itemIndex) in section.items"
                :key="itemIndex"
                :icon="section.checklist ? CheckCircle : undefined"
              >
                {{ item }}
              </UiListItem>
            </UiList>

            <UiCard
              v-for="(card, cardIndex) in section.cards"
              :key="`card-${cardIndex}`"
              padding="md"
              data-testid="legal-section-card"
            >
              <UiHeading :level="3" class="mb-2">
                {{ card.title }}
              </UiHeading>
              <UiText tone="muted" size="sm">
                {{ card.description }}
              </UiText>
            </UiCard>

            <UiAlert v-if="section.note" variant="info" :icon-size="20" data-testid="legal-section-note">
              {{ section.note }}
            </UiAlert>

            <UiLink
              v-if="section.link"
              :href="section.link.href"
              :underline="false"
              class="font-medium"
              testid="legal-section-link"
            >
              {{ section.link.label }}
            </UiLink>
          </UiStack>
        </UiStack>

        <UiCard
          v-if="contactTitle"
          padding="xl"
          data-testid="legal-contact"
          class="animate-reveal-up motion-reduce:animate-none"
          :class="revealDelay(200)"
        >
          <UiHeading :level="3" size="section" class="mb-4">
            {{ contactTitle }}
          </UiHeading>
          <UiText tone="muted" size="lg" class="mb-6 max-w-2xl">
            {{ contactDescription }}
          </UiText>
          <UiButton
            :to="localePath('/contact')"
            :variant="contactDanger ? 'danger' : 'primary'"
            size="lg"
            data-testid="legal-contact-link"
          >
            <component :is="contactIcon" v-if="contactIcon" :size="20" :stroke-width="2" class="mr-2" />
            {{ contactLink }}
            <ArrowRight :size="20" :stroke-width="2" class="ml-2" />
          </UiButton>
        </UiCard>
      </UiStack>
    </UiSection>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * LegalDocument - the shared page body of every legal text (terms, privacy, cookies, refunds,
 * acceptable use): a hero with the title and the "last updated" line, a numbered or icon-led list
 * of sections, and a closing "contact us" card.
 *
 * The page owns its copy and SEO; this component only lays the text out, so the five documents
 * can not drift apart in how they look.
 */

import { revealDelay } from '~/utils/reveal'
import type { Component } from 'vue'
import { ArrowRight, CheckCircle } from 'lucide-vue-next'
import type { LegalSection } from '~/types/legalsection'

/** Props of LegalDocument. */
interface Props {
  /** Small pill above the title. */
  badge?: string
  /** The document title, rendered as the h1. */
  title: string
  /** One lead sentence under the title. */
  subtitle?: string
  /** The "last updated" line, already worded and dated by the page. */
  meta?: string
  /** The sections, in reading order. */
  sections: LegalSection[]
  /** Heading of the closing contact card; without it the card is not drawn. */
  contactTitle?: string
  /** One sentence in the closing contact card. */
  contactDescription?: string
  /** Label of the button that opens the contact page. */
  contactLink?: string
  /** Paints the contact button as a warning (the abuse-report card of the acceptable-use policy). */
  contactDanger?: boolean
  /** A lucide icon shown before the contact button label. */
  contactIcon?: Component
}

withDefaults(defineProps<Props>(), {
  badge: undefined,
  subtitle: undefined,
  meta: undefined,
  contactTitle: undefined,
  contactDescription: undefined,
  contactLink: undefined,
  contactDanger: false,
  contactIcon: undefined,
})

/** Localised path builder from the i18n module. */
const localePath = useLocalePath()
</script>
