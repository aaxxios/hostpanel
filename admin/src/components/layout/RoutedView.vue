<script setup lang="ts">
/**
 * The routed slot of a layout: a `<RouterView>` that answers a click before the next screen has arrived.
 *
 * vue-router keeps the old screen until the chunk of the next one is downloaded. While the navigation
 * store holds a pending target, the one `RoutedView` whose screen that navigation replaces shows
 * the skeleton instead, and the load error when the download failed for good. Every other level keeps
 * what it shows: a tab change inside the client area keeps the client header and inner sidebar.
 * Use it wherever a layout would write `<RouterView />`.
 */
import { computed, inject, unref } from 'vue'
import { RouterView, useRouter, viewDepthKey } from 'vue-router'
import type { RouteLocationNormalized, RouteRecordNormalized } from 'vue-router'
import ViewLoadError from '@/components/layout/ViewLoadError.vue'
import ViewLoading from '@/components/layout/ViewLoading.vue'
import { useNavigationStore } from '@/stores/navigation'

/** The router, whose current route is what this level shows now. */
const router = useRouter()

/** The navigation in flight, if any. */
const navigation = useNavigationStore()

/** The depth vue-router gives the `<RouterView>` rendered here; the enclosing view provides it. */
const injectedDepth = inject(viewDepthKey, 0)

/**
 * Finds the record a `<RouterView>` at this depth renders for a route.
 *
 * Mirrors vue-router's own lookup: a record without components (a grouping parent) is skipped.
 *
 * @param matched - The matched records of a route, outermost first.
 * @returns The record drawn here, or undefined when the route has none at this depth.
 */
const recordHere = (matched: readonly RouteRecordNormalized[]): RouteRecordNormalized | undefined => {
  let depth = unref(injectedDepth)
  while (matched[depth] !== undefined && !matched[depth]?.components) {
    depth += 1
  }
  return matched[depth]
}

/**
 * Whether a navigation to a target replaces what this level shows.
 *
 * @param target - The target of a navigation, or null when there is none.
 * @returns True when the target draws a different record here.
 */
const changesHere = (target: RouteLocationNormalized | null): boolean =>
  target !== null && recordHere(target.matched) !== recordHere(router.currentRoute.value.matched)

/** True while the screen this level is about to show is still downloading. */
const loading = computed<boolean>(() => changesHere(navigation.pending))

/** True when the screen this level was about to show could not be downloaded. */
const failed = computed<boolean>(() => changesHere(navigation.failed))
</script>

<template>
  <ViewLoading v-if="loading" />
  <ViewLoadError v-else-if="failed" />
  <RouterView v-else />
</template>
