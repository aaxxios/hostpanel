/**
 * Unit tests for `utils/shareImage.ts`: the share-image fields a page emits.
 *
 * @module tests/utils/shareImage.test
 */

import { describe, expect, it } from 'vitest'
import { shareImageMeta } from '~/utils/shareImage'

describe('shareImageMeta', () => {
  it('describes the bundled card with its absolute URL, size and type', () => {
    expect(shareImageMeta('https://panel.test', 'Site')).toEqual({
      ogImage: 'https://panel.test/og-image.jpg',
      twitterImage: 'https://panel.test/og-image.jpg',
      ogImageAlt: 'Site',
      twitterImageAlt: 'Site',
      ogImageWidth: 1200,
      ogImageHeight: 630,
      ogImageType: 'image/jpeg',
    })
  })

  it('gives a page image only its URL and alt, since its size is unknown', () => {
    expect(shareImageMeta('https://panel.test', 'Page', '/img/x.png')).toEqual({
      ogImage: 'https://panel.test/img/x.png',
      twitterImage: 'https://panel.test/img/x.png',
      ogImageAlt: 'Page',
      twitterImageAlt: 'Page',
    })
  })
})
