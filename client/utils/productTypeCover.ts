/**
 * The generated cover illustration of a product family, by the type of service its products sell.
 *
 * The illustrations are single-colour silhouettes shipped as client assets (`public/covers/*.svg`),
 * so every install has them. A card paints one as a CSS mask over a block filled with the family
 * accent, which is why the same file reads in light and dark and takes any family colour. Every
 * class is spelled out in full: Tailwind emits only literals it finds in the scanned folders (this one is under utils/ for that reason; constants/ is not scanned), so a class
 * built at run time (`mask-[url(/covers/${x}.svg)]`) would paint nothing.
 *
 * @module utils/productTypeCover
 */

import { ProductType } from '~/types/producttype'

/** Mask classes shared by every cover: centred, cropped to fill, no tiling. */
const COVER_MASK_BASE = '[mask-size:cover] [mask-position:center] [mask-repeat:no-repeat]'

/** Cover used when a family's type is unknown or has no illustration of its own. */
export const DEFAULT_COVER_CLASS = `[mask-image:url(/covers/generic.svg)] ${COVER_MASK_BASE}`

/** Mask classes of each product type's cover illustration. */
export const PRODUCT_TYPE_COVER: Readonly<Record<ProductType, string>> = {
  [ProductType.SharedHosting]: `[mask-image:url(/covers/hosting.svg)] ${COVER_MASK_BASE}`,
  [ProductType.Vps]: `[mask-image:url(/covers/vps.svg)] ${COVER_MASK_BASE}`,
  [ProductType.Dedicated]: `[mask-image:url(/covers/dedicated.svg)] ${COVER_MASK_BASE}`,
  [ProductType.Domain]: `[mask-image:url(/covers/domain.svg)] ${COVER_MASK_BASE}`,
  [ProductType.Ssl]: `[mask-image:url(/covers/ssl.svg)] ${COVER_MASK_BASE}`,
  [ProductType.Email]: `[mask-image:url(/covers/email.svg)] ${COVER_MASK_BASE}`,
  [ProductType.Addon]: `[mask-image:url(/covers/addon.svg)] ${COVER_MASK_BASE}`,
  [ProductType.Backup]: `[mask-image:url(/covers/backup.svg)] ${COVER_MASK_BASE}`,
  [ProductType.Other]: DEFAULT_COVER_CLASS,
}

/**
 * The cover classes of a product type.
 *
 * @param type The type as the API sent it; may be missing or unknown.
 * @returns The type's mask classes, or the generic cover's when there is none.
 */
export const productTypeCover = (type: string | null | undefined): string =>
  PRODUCT_TYPE_COVER[type as ProductType] ?? DEFAULT_COVER_CLASS
