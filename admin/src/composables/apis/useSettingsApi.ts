/**
 * Endpoints behind the settings store: system settings, email templates, products, product
 * and groups.
 *
 * No state lives here; the settings store owns it. Every function rejects with an `ApiError`
 * carrying the problem+json body.
 */

import { buildQuery, useApi } from '@/composables/useApi'
import type { EmailTemplate } from '@/types/emailtemplate'
import type { CreateProductPayload, Product, ProductGroup, UpdateProductPayload } from '@/types/product'
import type { Setting } from '@/types/setting'
import { ApiPath } from '@/lib/apiPaths'
import { HttpMethod } from '@/types/http'

/**
 * Endpoint functions for settings, email templates and products.
 *
 * @returns One typed function per endpoint.
 */
export const useSettingsApi = () => {
  const { request } = useApi()

  /**
   * Lists the system settings.
   *
   * @returns Every setting row.
   */
  const listSettings = (): Promise<Setting[]> => request<Setting[]>(ApiPath.AdminSettings)

  /**
   * Writes one setting's value; the API updates by id and never creates.
   *
   * @param id - Setting id.
   * @param value - New value.
   * @returns Nothing; the API answers 204.
   */
  const updateSetting = (id: number, value: string): Promise<void> =>
    request<void>(`${ApiPath.AdminSettings}/${id}`, { method: HttpMethod.Put, body: JSON.stringify({ value }) })

  /**
   * Lists the email templates.
   *
   * @returns Every template.
   */
  const listEmailTemplates = (): Promise<EmailTemplate[]> =>
    request<EmailTemplate[]>(ApiPath.AdminEmailTemplates)

  /**
   * Lists every product, inactive and unsellable ones included.
   *
   * @returns Every product.
   */
  const listProducts = (): Promise<Product[]> =>
    request<Product[]>(`${ApiPath.Products}${buildQuery({ activeOnly: false, includeUnsellable: true })}`)

  /**
   * Lists every product group, inactive ones included.
   *
   * @returns Every group.
   */
  const listProductGroups = (): Promise<ProductGroup[]> =>
    request<ProductGroup[]>(`${ApiPath.Products}/groups${buildQuery({ activeOnly: false })}`)

  /**
   * Creates a product.
   *
   * @param payload - Product data.
   * @returns The new product id.
   */
  const createProduct = (payload: CreateProductPayload): Promise<number> =>
    request<number>(ApiPath.Products, { method: HttpMethod.Post, body: JSON.stringify(payload) })

  /**
   * Updates a product; the id travels in the body as well as the path.
   *
   * @param id - Product id.
   * @param payload - Updated product data.
   * @returns Nothing; the API answers 204.
   */
  const updateProduct = (id: number, payload: UpdateProductPayload): Promise<void> =>
    request<void>(`${ApiPath.Products}/${id}`, { method: HttpMethod.Put, body: JSON.stringify({ id, ...payload }) })

  return {
    listSettings,
    updateSetting,
    listEmailTemplates,
    listProducts,
    listProductGroups,
    createProduct,
    updateProduct,
  }
}
