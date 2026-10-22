/**
 * One catalogue product group, as `GET /api/portal/public/product-groups` returns it.
 *
 * The route forwards the C# `ProductGroupDto` verbatim, camelCase as `System.Text.Json` writes it.
 * A group is what the storefront shows as one product family.
 *
 * @module types/portalproductgroup
 */

/** One catalogue product group. */
export interface PortalProductGroup {
  /** Group primary key. */
  id: number
  /** Display name; the family's page address is derived from it. */
  name: string
  /** Optional description. */
  description?: string | null
  /** Cover image the operator uploaded for the group, when the backend sends one; absent otherwise. */
  imageUrl?: string | null
  /** Whether the operator keeps the group visible in the storefront. */
  isActive: boolean
  /** Number of products in the group. */
  productCount: number
}
