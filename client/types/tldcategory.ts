/**
 * Category slug a registrar can tag a domain extension with.
 *
 * A closed set of the slugs the storefront has a translation for; each value is also the last
 * segment of the `domains.pricing.categories.*` locale key. A category outside the set is still
 * shown, with a capitalised version of its slug (see `utils/tldCategory.ts`).
 */
export enum TldCategory {
  /** Low-priced extensions. */
  Budget = 'budget',
  /** Extensions for companies and trade. */
  Business = 'business',
  /** Extensions for groups, clubs and causes. */
  Community = 'community',
  /** Extensions for artists, media and design. */
  Creative = 'creative',
  /** Extensions tied to a city or a locality. */
  Local = 'local',
  /** The most registered extensions. */
  Popular = 'popular',
  /** Other (legacy) catch-all group. */
  Other = 'other',
  /** Extensions for professions. */
  Professional = 'professional',
  /** Extensions for IT professionals. */
  ProfessionalIt = 'professional-it',
  /** Extensions for property. */
  RealEstate = 'real-estate',
  /** Extensions tied to a country or a region. */
  Regional = 'regional',
  /** Extensions for online shops. */
  Shopping = 'shopping',
  /** Extensions for technology (long form). */
  Technology = 'technology',
  /** Extensions for technology. */
  Tech = 'tech'
}
