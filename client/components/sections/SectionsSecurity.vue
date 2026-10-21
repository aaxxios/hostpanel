<template>
  <section v-if="features.length" class="py-12 md:py-20 bg-ui-page" data-testid="sections-security">
    <UiContainer>
      <MarketingSectionHeader
        class="mb-12"
        :badge="$t('hostingSections.security.badge')"
        :title="$t('hostingSections.security.title')"
        :subtitle="$t('hostingSections.security.subtitle')"
      />

      <UiBalancedGrid :count="features.length" data-testid="sections-security-grid">
        <UiCard
          v-for="feature in features"
          :key="feature"
          hover
          :data-testid="`security-${feature}`"
        >
          <UiStack gap="sm">
            <UiAccentTile :icon="FEATURE_ICONS[feature]" :class="ACCENT_BRAND_CLASS" />
            <UiHeading :level="3" size="subsection">{{ $t(`hostingSections.security.features.${feature}.title`) }}</UiHeading>
            <UiText tone="muted" size="sm">{{ $t(`hostingSections.security.features.${feature}.description`) }}</UiText>
          </UiStack>
        </UiCard>
      </UiBalancedGrid>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * Security section: one card per protection the plans include (SSL, backups, DDoS protection,
 * malware scanning, firewall). Only enabled features show; with none the section renders nothing.
 */
import { SecurityFeatureKey } from '~/constants/securityFeatureKey'
import { ACCENT_BRAND_CLASS } from '~/utils/ui/accent'

/** The enabled protections, in display order. */
const features = useSecurityFeatures()

/** Icon of each protection. */
const FEATURE_ICONS: Record<SecurityFeatureKey, string> = {
  [SecurityFeatureKey.Ssl]: 'mdi:lock-check',
  [SecurityFeatureKey.Backups]: 'mdi:backup-restore',
  [SecurityFeatureKey.Ddos]: 'mdi:shield-alert',
  [SecurityFeatureKey.Malware]: 'mdi:bug-check',
  [SecurityFeatureKey.Firewall]: 'mdi:wall-fire',
}
</script>
