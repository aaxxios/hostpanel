/**
 * The registry of template components: one lazy loader per template and slot.
 *
 * @module templates/registry
 */

import { TemplateName } from '~/types/templatename'
import { TemplateSlot } from '~/types/templateslot'

/** Defers loading a template component until the slot is actually rendered. */
export type TemplateLoader = () => Promise<unknown>

/**
 * The single place a template name and slot resolve to a component.
 *
 * Loaders are dynamic imports so the browser only fetches the active
 * template's components, and so a component can be added without any
 * string-based lookup against Nuxt's auto-import namespace.
 */
export const templates: Record<TemplateName, Record<TemplateSlot, TemplateLoader>> = {
  [TemplateName.Aurora]: {
    [TemplateSlot.Header]: () => import('~/templates/aurora/layout/AuroraHeader.vue'),
    [TemplateSlot.Footer]: () => import('~/templates/aurora/layout/AuroraFooter.vue'),
    [TemplateSlot.Home]: () => import('~/templates/aurora/pages/AuroraHome.vue'),
    [TemplateSlot.Hosting]: () => import('~/templates/aurora/pages/AuroraHosting.vue'),
    [TemplateSlot.Domains]: () => import('~/templates/aurora/pages/AuroraDomains.vue'),
    [TemplateSlot.Checkout]: () => import('~/templates/aurora/pages/AuroraCheckout.vue'),
  },
  /*
   * nova reuses aurora's domain search and checkout deliberately. Both are
   * working flows against live endpoints — the checkout in particular is the
   * ordering path — and a second copy of them would be a second place for a
   * payment bug to live. nova supplies its own header, footer and page bodies,
   * which is where its design actually differs.
   */
  [TemplateName.Nova]: {
    [TemplateSlot.Header]: () => import('~/templates/nova/layout/NovaHeader.vue'),
    [TemplateSlot.Footer]: () => import('~/templates/nova/layout/NovaFooter.vue'),
    [TemplateSlot.Home]: () => import('~/templates/nova/pages/NovaHome.vue'),
    [TemplateSlot.Hosting]: () => import('~/templates/nova/pages/NovaHosting.vue'),
    [TemplateSlot.Domains]: () => import('~/templates/aurora/pages/AuroraDomains.vue'),
    [TemplateSlot.Checkout]: () => import('~/templates/aurora/pages/AuroraCheckout.vue'),
  },
  [TemplateName.Classic]: {
    [TemplateSlot.Header]: () => import('~/templates/classic/layout/ClassicHeader.vue'),
    [TemplateSlot.Footer]: () => import('~/templates/classic/layout/ClassicFooter.vue'),
    [TemplateSlot.Home]: () => import('~/templates/classic/pages/ClassicHome.vue'),
    [TemplateSlot.Hosting]: () => import('~/templates/classic/pages/ClassicHosting.vue'),
    [TemplateSlot.Domains]: () => import('~/templates/classic/pages/ClassicDomains.vue'),
    [TemplateSlot.Checkout]: () => import('~/templates/classic/pages/ClassicCheckout.vue'),
  },
}
