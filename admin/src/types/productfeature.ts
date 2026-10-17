/** One specification line of a product, as the API returns it. */
export interface ProductFeature {
  /** Unique line identifier. */
  id: number
  /** Product the line belongs to. */
  productId: number
  /** Row label; rows line up across plans by this text. */
  label: string
  /** Value shown for this product. */
  value: string
  /** Position in the list, 0-based. */
  sortOrder: number
}

/** Body that adds a specification line to a product. */
export interface CreateProductFeaturePayload {
  /** Product the line belongs to. */
  productId: number
  /** Row label. */
  label: string
  /** Value shown for the product. */
  value: string
  /** Position of the new line. */
  sortOrder: number
}

/** Body that replaces a specification line. */
export interface UpdateProductFeaturePayload {
  /** Line identifier, repeated in the body as the API expects. */
  id: number
  /** Row label. */
  label: string
  /** Value shown for the product. */
  value: string
  /** Position in the list. */
  sortOrder: number
}
