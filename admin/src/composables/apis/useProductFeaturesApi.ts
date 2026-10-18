/**
 * Endpoints behind the product features store: the specification lines of a product.
 *
 * No state lives here; every function rejects with an `ApiError` carrying the problem+json body.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type {
  CreateProductFeaturePayload,
  ProductFeature,
  UpdateProductFeaturePayload,
} from '@/types/productfeature'

/**
 * Endpoint functions for `/product-features`.
 *
 * @returns One typed function per endpoint.
 */
export const useProductFeaturesApi = () => {
  const { request } = useApi()

  /**
   * Lists a product's specification lines.
   *
   * @param productId - Product id.
   * @returns The lines.
   */
  const listFeatures = (productId: number): Promise<ProductFeature[]> =>
    request<ProductFeature[]>(`/product-features${buildQuery({ productId })}`)

  /**
   * Adds a line.
   *
   * @param payload - Product, label, value and position.
   * @returns Nothing.
   */
  const createFeature = (payload: CreateProductFeaturePayload): Promise<void> =>
    request<void>('/product-features', { method: 'POST', body: JSON.stringify(payload) })

  /**
   * Replaces a line.
   *
   * @param payload - Line id, label, value and position.
   * @returns Nothing.
   */
  const updateFeature = (payload: UpdateProductFeaturePayload): Promise<void> =>
    request<void>(`/product-features/${payload.id}`, { method: 'PUT', body: JSON.stringify(payload) })

  /**
   * Removes a line.
   *
   * @param id - Line id.
   * @returns Nothing.
   */
  const deleteFeature = (id: number): Promise<void> =>
    request<void>(`/product-features/${id}`, { method: 'DELETE' })

  return { listFeatures, createFeature, updateFeature, deleteFeature }
}
