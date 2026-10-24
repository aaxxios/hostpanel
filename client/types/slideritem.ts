/** One home-page slide shaped for the slider template. */
export interface SliderItem {
  /** Slide identifier as a string, used in test ids and keys. */
  id: string
  /** Slide headline. */
  name: string
  /** Subtitle under the headline; empty when there is none. */
  tagline: string
  /** Body text; empty when there is none. */
  description: string
  /** Feature chips. */
  features: string[]
  /** MDI icon name without the `mdi:` prefix. */
  icon: string
  /** Hex brand colour of the slide. */
  color: string
  /** Uploaded cover picture URL; empty when the slide has none. */
  image: string
  /** External demo link; empty when there is none. */
  demoUrl: string
  /** Page the primary button opens. */
  learnMoreUrl: string
}
