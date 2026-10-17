/** One marketing slide, as `GET /api/portal/public/slides` returns it. */
export interface SlidePublic {
  /** Unique slide identifier. */
  id: number
  /** MDI icon name (without the `mdi:` prefix). */
  iconName: string
  /** Hex brand color for the slide. */
  brandColor: string
  /** Background image path. */
  imageUrl: string
  /** Optional live demo URL. */
  demoUrl: string | null
  /** URL for the "Learn More" page. */
  learnMoreUrl: string | null
  /** Associated product ID, if any. */
  productId: number | null
  /** Monthly price, if applicable. */
  monthlyPrice: number | null
  /** Annual price, if applicable. */
  annualPrice: number | null
  /** Resolved translated title. */
  title: string
  /** Short tagline text. */
  tagline: string | null
  /** Longer description text. */
  description: string | null
  /** Feature bullet points. */
  features: string[] | null
  /** Call-to-action button text. */
  ctaText: string | null
  /** Call-to-action button URL. */
  ctaUrl: string | null
}
