/**
 * Brand tokens plugin: puts the operator's colours and typefaces on `:root` before any
 * stylesheet applies.
 *
 * One `<style id="brand-tokens">` in `<head>`, rendered on the server from the
 * `portal.brand.*` settings, so the first paint already carries the brand: a client-side
 * write would flash the template's own colours first. Every consumer of these variables
 * (Tailwind's `primary`/`secondary` scales, aurora's `--ac1/--ac2`, nova's `--n-brand`)
 * names the template's original literal as its `var()` fallback, and this plugin emits
 * **nothing** when no setting is set, so an install that never opened the admin panel
 * renders byte-for-byte what it rendered before.
 *
 * The maths lives in `utils/brandTokens.ts` and `utils/brandPalette.ts`, shared with the
 * admin preview and (as a port) the e-mail renderer. Runs after `portal-settings`, which
 * loads the values.
 *
 * @module plugins/brand-tokens
 */

import { PortalSettingKey } from '~/constants/portalSettingKey'
import { brandTokensCss } from '~/utils/brandTokens'

export default defineNuxtPlugin({
  name: 'brand-tokens',
  dependsOn: ['portal-settings'],
  setup() {
    const { get } = usePortalSettings()

    /** The brand stylesheet built from the operator's colour and font settings; empty when none is set. */
    const css = computed(() => brandTokensCss({
      primary: get(PortalSettingKey.BrandPrimary, 'portalBrandPrimary'),
      accent: get(PortalSettingKey.BrandAccent, 'portalBrandAccent'),
      fontHeading: get(PortalSettingKey.BrandFontHeading, 'portalBrandFontHeading'),
      fontBody: get(PortalSettingKey.BrandFontBody, 'portalBrandFontBody'),
    }))

    useHead({
      style: () => css.value
        ? [{ id: 'brand-tokens', innerHTML: css.value, tagPriority: 'critical' }]
        : [],
    })
  },
})
