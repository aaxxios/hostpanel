<template>
  <UiSection v-if="features.length" padding="lg" class="border-b border-nova-border">
    <div class="max-w-[620px]">
      <NovaSectionHeader
        :eyebrow="t('nova.security.eyebrow')"
        :title="t('nova.security.title')"
        :lead="t('nova.security.lead')"
      />
    </div>

    <UiGrid as="ul" cols="3" gap="lg" class="mt-10">
      <NovaFeatureCard v-for="feature in features" :key="feature.key" :feature="feature" />
    </UiGrid>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova security section.
 *
 * SSL and two-factor sign-in are implemented in this codebase and stated
 * plainly. A firewall, a malware scanner and a backup schedule live on the
 * operator's servers, so each of those waits for its setting before it claims
 * anything.
 */
import NovaFeatureCard from '~/templates/nova/sections/FeatureCard.vue'
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'
import { SECURITY_FEATURES } from '~/templates/nova/content'
import { useVisibleFeatures } from '~/templates/nova/features'

/** Translator for the copy. */
const { t } = useI18n()
/** The security claims this install may state. */
const features = useVisibleFeatures(SECURITY_FEATURES)
</script>
