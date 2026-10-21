<template>
  <section v-if="channels.length" class="py-12 md:py-20 bg-ui-page" data-testid="sections-support">
    <UiContainer>
      <MarketingSectionHeader
        class="mb-12"
        :badge="$t('hostingSections.support.badge')"
        :title="$t('hostingSections.support.title')"
        :subtitle="$t('hostingSections.support.subtitle')"
      />

      <UiBalancedGrid :count="channels.length" :class="gridWidth" data-testid="sections-support-grid">
        <UiCard
          v-for="item in channels"
          :key="item.channel"
          hover
          :data-testid="`support-${item.channel}`"
        >
          <UiStack gap="sm">
            <UiAccentTile :icon="CHANNEL_ICONS[item.channel]" :class="ACCENT_BRAND_CLASS" />
            <UiHeading :level="3" size="subsection">{{ $t(`hostingSections.support.channels.${item.channel}.title`) }}</UiHeading>
            <UiText tone="muted" size="sm">{{ $t(`hostingSections.support.channels.${item.channel}.description`) }}</UiText>
            <UiBadge v-if="item.responseTime" variant="success" rounded :data-testid="`support-response-${item.channel}`">
              {{ $t('hostingSections.support.responseTime', { time: item.responseTime }) }}
            </UiBadge>
            <UiLink v-if="item.href" :href="item.href" :underline="false" :testid="`support-link-${item.channel}`">
              {{ item.value ?? $t(`hostingSections.support.channels.${item.channel}.action`) }}
            </UiLink>
          </UiStack>
        </UiCard>
      </UiBalancedGrid>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * Support section: the ways to reach the team (live chat, ticket, email, phone) with the reply
 * time the operator promises for each. Only configured channels show.
 */
import { SupportChannel } from '~/constants/supportChannel'
import { ACCENT_BRAND_CLASS } from '~/utils/ui/accent'

/** The support channels on offer, in display order. */
const channels = useSupportChannels()

/** Icon of each channel. */
const CHANNEL_ICONS: Record<SupportChannel, string> = {
  [SupportChannel.Chat]: 'mdi:chat-processing-outline',
  [SupportChannel.Ticket]: 'mdi:ticket-outline',
  [SupportChannel.Email]: 'mdi:email-outline',
  [SupportChannel.Phone]: 'mdi:phone-outline',
}

/** Width caps by card count, so a lone card does not stretch across the whole page. */
const GRID_WIDTH_BY_COUNT: Record<number, string> = { 1: 'mx-auto max-w-xl', 2: 'mx-auto max-w-4xl' }

/** Classes that cap the grid width for the current number of channels. */
const gridWidth = computed(() => GRID_WIDTH_BY_COUNT[channels.value.length] ?? '')
</script>
