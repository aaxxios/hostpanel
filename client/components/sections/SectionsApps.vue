<template>
  <section class="py-12 md:py-20 bg-ui-page" data-testid="sections-apps">
    <UiContainer>
      <MarketingSectionHeader
        class="mb-6"
        :badge="$t('hostingSections.apps.badge')"
        :title="$t('hostingSections.apps.title')"
        :subtitle="$t('hostingSections.apps.subtitle')"
      />

      <UiText v-if="installer.count" tone="muted" class="text-center mb-10" data-testid="sections-apps-count">
        {{ $t('hostingSections.apps.count', { count: installer.count }) }}
      </UiText>

      <UiBalancedGrid :count="installer.categories.length" class="mt-6" data-testid="sections-apps-grid">
        <UiCard
          v-for="category in installer.categories"
          :key="category"
          hover
          :data-testid="`app-${category}`"
        >
          <UiStack direction="row" align="center" gap="md">
            <UiAccentTile :icon="CATEGORY_ICONS[category]" :class="ACCENT_BRAND_CLASS" />
            <UiHeading :level="3" size="subsection">{{ $t(`hostingSections.apps.categories.${category}`) }}</UiHeading>
          </UiStack>
        </UiCard>
      </UiBalancedGrid>
    </UiContainer>
  </section>
</template>

<script setup lang="ts">
/**
 * One-click apps section: the categories of the installer and, when the operator set it, how
 * many apps it offers. The section is shown whenever its toggle is on.
 */
import { InstallerAppCategory } from '~/constants/installerAppCategory'
import { ACCENT_BRAND_CLASS } from '~/utils/ui/accent'

/** The installer's app count and its categories. */
const installer = useInstallerApps()

/** Icon of each category. */
const CATEGORY_ICONS: Record<InstallerAppCategory, string> = {
  [InstallerAppCategory.Cms]: 'mdi:file-document-edit-outline',
  [InstallerAppCategory.Shop]: 'mdi:cart-outline',
  [InstallerAppCategory.Forum]: 'mdi:forum-outline',
  [InstallerAppCategory.Wiki]: 'mdi:book-open-page-variant-outline',
  [InstallerAppCategory.Blog]: 'mdi:post-outline',
  [InstallerAppCategory.Gallery]: 'mdi:image-multiple-outline',
}
</script>
