/**
 * Test support for the classic template's own building blocks.
 *
 * `mountWithKit` registers only `components/ui/`; the classic home sections also render
 * `ClassicSectionHeader`, `ClassicCard` and `ClassicIconTile` from `components/classic/`, which
 * Nuxt would auto-import. This map registers them under those names for a mount.
 *
 * @module tests/helpers/classicKit
 */

import type { Component } from 'vue'
import ClassicCard from '~/components/classic/ClassicCard.vue'
import ClassicIconTile from '~/components/classic/ClassicIconTile.vue'
import ClassicSectionHeader from '~/components/classic/ClassicSectionHeader.vue'

/** The classic building blocks by the name Nuxt would auto-import them under. */
export const CLASSIC_COMPONENTS: Record<string, Component> = {
  ClassicCard,
  ClassicIconTile,
  ClassicSectionHeader,
}
