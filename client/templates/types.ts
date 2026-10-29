/**
 * The default template, the list of templates, and the rule that narrows a configured value to one.
 *
 * @module templates/types
 */

import { TemplateName } from '~/types/templatename'

/** Every template the portal can render. */
export const TEMPLATE_NAMES: readonly TemplateName[] = Object.values(TemplateName)

/** The template used when configuration is absent or unrecognised. */
export const DEFAULT_TEMPLATE: TemplateName = TemplateName.Classic

/**
 * Narrows an untrusted configuration value to a template name.
 *
 * Anything unrecognised (a typo in an admin settings field, an unset
 * environment variable, a template removed from a later release) resolves to
 * {@link DEFAULT_TEMPLATE} so a bad value degrades to the default design
 * instead of leaving the site with nothing to render.
 *
 * @param input Raw configured value.
 * @returns A template name that is guaranteed to exist in the registry.
 */
export const resolveTemplateName = (input: string | null | undefined): TemplateName => {
  return TEMPLATE_NAMES.includes(input as TemplateName)
    ? (input as TemplateName)
    : DEFAULT_TEMPLATE
}
