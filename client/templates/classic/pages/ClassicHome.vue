<template>
  <UiStack gap="none" data-testid="classic-home">
    <!-- H1 for SEO - visually hidden, semantically the first heading on the page -->
    <UiHeading :level="UiHeadingLevel.H1" class="sr-only" data-testid="classic-home-title">{{ $t('seo.home.h1') }}</UiHeading>

    <!-- Products full-screen slider -->
    <SectionsProductsFullSlider />

    <!-- Partners / clients logos -->
    <SectionsPartners />

    <!-- Why choose us -->
    <SectionsWhyChooseUs />

    <!-- Process -->
    <SectionsProcess />

    <!-- Testimonials -->
    <SectionsTestimonials />

    <!-- Products -->
    <SectionsProducts />

    <!-- FAQ -->
    <SectionsFAQ :limit="6" :show-categories="false" />

    <!-- Call to action -->
    <SectionsCTA />
  </UiStack>
</template>

<script setup lang="ts">
/**
 * ClassicHome - the classic template's homepage: the original landing composition, a hidden
 * H1 followed by the marketing sections.
 */
import { UiHeadingLevel } from '~/utils/ui/enums'
</script>
