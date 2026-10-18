/**
 * Image upload endpoints of the admin API.
 *
 * No state lives here; the settings and slides stores own it. Both endpoints take a
 * multipart `file` field and answer `{ url }`, and both reject with an `ApiError`
 * carrying the problem+json body.
 */

import { useApi } from '@/composables/useApi'
import type { BrandingImageKind, UploadedImage } from '@/types/upload'

/**
 * Endpoint functions for the image uploads.
 *
 * @returns One typed function per upload endpoint.
 */
export const useUploadApi = () => {
  const { upload } = useApi()

  /**
   * Uploads a branding image.
   *
   * @param kind - Which branding slot the image is for.
   * @param file - The image file.
   * @returns The stored image's URL.
   */
  const uploadBrandingImage = (kind: BrandingImageKind, file: File): Promise<UploadedImage> =>
    upload<UploadedImage>(`/admin/settings/branding/${kind}`, file)

  /**
   * Uploads a slide image.
   *
   * @param file - The image file.
   * @returns The stored image's URL.
   */
  const uploadSlideImage = (file: File): Promise<UploadedImage> =>
    upload<UploadedImage>('/admin/slides/upload-image', file)

  return { uploadBrandingImage, uploadSlideImage }
}
