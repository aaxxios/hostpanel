/**
 * Resolves components for the active portal template.
 *
 * `defineAsyncComponent` is awaited by Vue during server rendering, so slots
 * resolved through this composable appear in the server-rendered HTML rather
 * than only after hydration: the template system must never cost the site its
 * indexable markup.
 *
 * @returns The active template name and a resolver for its slots.
 *
 * @module composables/useTemplate
 */

import { PortalSettingKey } from '~/constants/portalSettingKey'
import type { Component } from 'vue'
import { templates } from '~/templates/registry'
import { resolveTemplateName } from '~/templates/types'
import type { TemplateName } from '~/types/templatename'
import type { TemplateSlot } from '~/types/templateslot'

/**
 * Binds the operator's template choice to the registry of slot loaders.
 *
 * @returns The active template `name` and `slot`, which resolves a slot to its component.
 */
export const useTemplate = () => {
  const { get } = usePortalSettings()

  // The operator's admin setting wins, then NUXT_PUBLIC_PORTAL_TEMPLATE, then
  // the built-in default. resolveTemplateName turns anything unrecognised
  // (a typo in the admin field, a template dropped in a later release) back
  // into the default rather than leaving the site with nothing to render.
  const name = computed<TemplateName>(() =>
    resolveTemplateName(get(PortalSettingKey.Template, 'portalTemplate')))

  /**
   * Resolves one slot of the active template.
   *
   * @param key Slot to resolve.
   * @returns An async component suitable for `<component :is>`.
   */
  const slot = (key: TemplateSlot): Component =>
    defineAsyncComponent(templates[name.value][key] as () => Promise<Component>)

  return { name, slot }
}
