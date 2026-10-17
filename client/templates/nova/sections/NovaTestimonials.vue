<template>
  <UiSection v-if="testimonials.length" padding="lg" class="border-b border-nova-border bg-nova-surface">
    <NovaSectionHeader :eyebrow="t('nova.testimonials.eyebrow')" :title="t('nova.testimonials.title')" />

    <UiGrid as="ul" cols="3" gap="lg" class="mt-10">
      <li v-for="testimonial in testimonials" :key="testimonial.key" class="h-full">
        <UiCard padding="lg" class="flex h-full flex-col !border-nova-border !bg-nova-bg">
          <figure class="flex h-full flex-col">
            <Icon name="lucide:quote" class="h-6 w-6 text-nova-brand" aria-hidden="true" />
            <blockquote class="mt-4 flex-1 text-[15px] leading-[1.7] text-nova-ink">
              {{ testimonial.quote }}
            </blockquote>
            <figcaption class="mt-5 border-t border-nova-border pt-4 text-[15px]">
              <span class="font-semibold text-nova-ink">{{ testimonial.name }}</span>
              <span v-if="testimonial.role" class="block text-sm text-nova-muted">{{ testimonial.role }}</span>
            </figcaption>
          </figure>
        </UiCard>
      </li>
    </UiGrid>
  </UiSection>
</template>

<script setup lang="ts">
/**
 * nova testimonials.
 *
 * No endpoint serves customer quotes, so `TESTIMONIALS` is empty and this
 * section renders nothing at all — not a placeholder, and certainly not an
 * invented name. The markup is here so that when a real source appears, the
 * only thing needed is the data.
 */
import NovaSectionHeader from '~/templates/nova/sections/NovaSectionHeader.vue'
import { TESTIMONIALS } from '~/templates/nova/content'
import type { Testimonial } from '~/templates/nova/types/testimonial'

/** Props of NovaTestimonials. */
const props = withDefaults(defineProps<{
  /** Overrides the compiled-in list; both are empty until a source exists. */
  items?: readonly Testimonial[]
}>(), { items: undefined })

/** Translator for the copy. */
const { t } = useI18n()

/** The quotes to show: the override when given, the compiled-in list otherwise. */
const testimonials = computed(() => props.items ?? TESTIMONIALS)
</script>
