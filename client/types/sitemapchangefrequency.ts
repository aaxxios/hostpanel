/**
 * How often a sitemap entry is expected to change; the values are the sitemap protocol's own.
 *
 * @module types/sitemapchangefrequency
 */
export enum SitemapChangeFrequency {
  /** Changes every day. */
  Daily = 'daily',
  /** Changes every week. */
  Weekly = 'weekly',
  /** Changes every month. */
  Monthly = 'monthly',
  /** Changes every year. */
  Yearly = 'yearly'
}
