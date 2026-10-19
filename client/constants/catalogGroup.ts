/**
 * The one product group id the storefront reads by name. Every other group is read from the
 * catalogue's group list (`stores/catalog.ts`).
 *
 * @module constants/catalogGroup
 */

/**
 * Product group the storefront's plan cards draw from — shared hosting.
 *
 * Without it the endpoint returns the whole catalogue, so the hosting page listed SSL
 * certificates, mailboxes and domain registration beside the hosting plans.
 */
export const HOSTING_GROUP_ID = 1
