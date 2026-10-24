<template>
  <template v-if="plan.uploaded">
    <!-- Unoptimised: an upload is proxied from the API, not served from public/, so IPX cannot optimise it. -->
    <UiImage
      v-if="plan.light"
      :src="plan.light"
      :alt="alt"
      :loading="loading"
      :fit="UiImageFit.Contain"
      unoptimised
      testid="app-logo-light"
      :class="[imgClass, plan.swap ? 'dark:hidden' : plateClass]"
    />
    <UiImage
      v-if="plan.swap || !plan.light"
      :src="plan.dark"
      :alt="alt"
      :loading="loading"
      :fit="UiImageFit.Contain"
      unoptimised
      testid="app-logo-dark"
      :class="[imgClass, plan.swap ? 'hidden dark:block' : plateClass]"
    />
  </template>
  <template v-else>
    <UiImage
      v-if="plan.light"
      :src="plan.light"
      :alt="alt"
      :width="width"
      :height="height"
      :loading="loading"
      :fit="UiImageFit.Contain"
      testid="app-logo-light"
      :class="[imgClass, 'dark:hidden']"
    />
    <UiImage
      :src="plan.dark"
      :alt="alt"
      :width="width"
      :height="height"
      :loading="loading"
      :fit="UiImageFit.Contain"
      testid="app-logo-dark"
      :class="[imgClass, plan.light ? 'hidden dark:block' : '']"
    />
  </template>
</template>

<script setup lang="ts">
/**
 * The one logo component: the operator's upload when there is one, otherwise the bundled
 * wordmark, in the variant that stays readable on the background it sits on.
 *
 * On the page-mode surface both wordmarks are rendered and swapped by the `dark:` variant
 * (never a JavaScript pick — see `useBrandLogo` for the hydration reason); on an
 * always-dark surface only the dark-background wordmark is rendered.
 */
import { UiImageFit, UiImageLoading } from '~/utils/ui/enums'
import { LOGO_PLATE_CLASSES, LOGO_PLATE_CLASSES_DARK_MODE, LogoSurface, resolveAppLogo } from '~/utils/appLogo'

/** Props of the logo with their defaults applied. */
const props = withDefaults(
  defineProps<{
    /** The background the logo sits on. */
    surface?: LogoSurface
    /** Tailwind classes for the image (size, hover). */
    imgClass?: string
    /** Intrinsic width hint for the bundled image. */
    width?: number
    /** Intrinsic height hint for the bundled image. */
    height?: number
    /** Image loading strategy. */
    loading?: `${UiImageLoading}`
  }>(),
  { surface: LogoSurface.Auto, imgClass: '', width: 200, height: 60, loading: UiImageLoading.Eager },
)

/** The operator's wordmarks, whether a dark variant exists, and the brand name used as alt text. */
const { light, dark, hasDarkVariant, alt } = useBrandLogo()

/** What to render for the current settings and surface. */
const plan = computed(() => resolveAppLogo(
  { light: light.value, dark: dark.value, hasDarkVariant: hasDarkVariant.value },
  props.surface,
))

/**
 * The plate behind a lone upload: always on an always-dark surface, only in dark mode on the
 * page-mode surface, and nothing when a dark variant (or the bundled pair) already reads.
 */
const plateClass = computed(() => {
  if (!plan.value.plate) {
    return ''
  }
  return props.surface === LogoSurface.Dark ? LOGO_PLATE_CLASSES : LOGO_PLATE_CLASSES_DARK_MODE
})
</script>
