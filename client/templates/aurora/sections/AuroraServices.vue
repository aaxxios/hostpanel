<template>
  <UiSection id="services" padding="lg" :contained="false" class="relative border-t border-line">
    <UiText tone="inherit" size="inherit" :class="[EYEBROW_CLASS, 'text-ac2']">{{ t('aurora.services.eyebrow') }}</UiText>
    <UiHeading :level="2" size="bare" :class="[SECTION_TITLE_CLASS, 'mt-3 max-w-[680px]']">
      {{ t('aurora.services.title') }}
    </UiHeading>

    <UiGrid cols="auto" gap="lg" class="mt-10">
      <UiCard
        v-for="service in services"
        :key="service.key"
        tone="bare"
        hover
        :data-testid="`service-card-${service.key}`"
        class="border-line bg-surf hover:border-ac1"
      >
        <UiStack gap="sm">
          <UiTile size="lg" :class="[service.tint, 'font-mono font-medium text-tile-ink']">{{ service.tag }}</UiTile>
          <UiText as="span" size="inherit" tone="inherit" weight="bold" class="text-[18px] text-tx">{{ service.title }}</UiText>
          <UiText as="span" size="inherit" tone="inherit" class="text-[15px] leading-relaxed text-mut">{{ service.body }}</UiText>
        </UiStack>
      </UiCard>
    </UiGrid>
  </UiSection>
</template>

<script setup lang="ts">
/** aurora services grid — four editorial cards (hosting, servers, domains, billing), no data dependency. */
import { EYEBROW_CLASS, SECTION_TITLE_CLASS } from '~/templates/aurora/typography'

/** Translator for the active locale. */
const { t } = useI18n()

/** The four services: stable key, translated tile tag, copy, and a named token class for the tile gradient. */
const services = computed(() => [
  { key: 'hosting', tag: t('aurora.services.hostingTag'), title: t('aurora.services.hostingTitle'), body: t('aurora.services.hostingBody'), tint: 'bg-tile-sky' },
  { key: 'servers', tag: t('aurora.services.serversTag'), title: t('aurora.services.serversTitle'), body: t('aurora.services.serversBody'), tint: 'bg-tile-lilac' },
  { key: 'domains', tag: t('aurora.services.domainsTag'), title: t('aurora.services.domainsTitle'), body: t('aurora.services.domainsBody'), tint: 'bg-tile-mint' },
  { key: 'billing', tag: t('aurora.services.billingTag'), title: t('aurora.services.billingTitle'), body: t('aurora.services.billingBody'), tint: 'bg-tile-pink' },
])
</script>
