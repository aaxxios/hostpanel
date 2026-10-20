<template>
  <UiSection id="services" padding="lg" :contained="false" class="relative border-t border-line">
    <UiText tone="inherit" size="inherit" :class="[EYEBROW_CLASS, 'text-ac2']">{{ t('aurora.services.eyebrow') }}</UiText>
    <UiHeading :level="2" size="bare" :class="[SECTION_TITLE_CLASS, 'mt-3 max-w-[680px]']">
      {{ t('aurora.services.title') }}
    </UiHeading>

    <UiGrid cols="auto" gap="lg" class="mt-8">
      <UiCard
        v-for="service in services"
        :key="service.key"
        tone="bare"
        hover
        :data-testid="`service-card-${service.key}`"
        class="border-line bg-surf shadow-card transition-[border-color,transform] duration-200 hover:-translate-y-0.5 hover:border-ac1"
      >
        <UiStack gap="sm">
          <UiTile size="lg" :icon="service.icon" :class="[service.tint, 'text-white']" :data-testid="`service-icon-${service.key}`" />
          <UiText as="span" size="inherit" tone="inherit" weight="bold" class="text-[18px] text-tx">{{ service.title }}</UiText>
          <UiText as="span" size="inherit" tone="inherit" class="text-[15px] leading-relaxed text-mut">{{ service.body }}</UiText>
        </UiStack>
      </UiCard>
    </UiGrid>
  </UiSection>
</template>

<script setup lang="ts">
/** aurora services grid — four editorial cards (hosting, servers, domains, billing), each with an icon on a gradient tile, no data dependency. */
import { Cpu, Globe, Receipt, Server } from 'lucide-vue-next'
import { EYEBROW_CLASS, SECTION_TITLE_CLASS } from '~/templates/aurora/typography'

/** Translator for the active locale. */
const { t } = useI18n()

/** The four services: stable key, tile icon (white on the gradient), copy, and a named token class for the tile gradient. */
const services = computed(() => [
  { key: 'hosting', icon: Server, title: t('aurora.services.hostingTitle'), body: t('aurora.services.hostingBody'), tint: 'bg-tile-sky' },
  { key: 'servers', icon: Cpu, title: t('aurora.services.serversTitle'), body: t('aurora.services.serversBody'), tint: 'bg-tile-lilac' },
  { key: 'domains', icon: Globe, title: t('aurora.services.domainsTitle'), body: t('aurora.services.domainsBody'), tint: 'bg-tile-mint' },
  { key: 'billing', icon: Receipt, title: t('aurora.services.billingTitle'), body: t('aurora.services.billingBody'), tint: 'bg-tile-pink' },
])
</script>
