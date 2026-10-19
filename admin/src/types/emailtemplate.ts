/** Represents an email template. */
export interface EmailTemplate {
  /** Unique template identifier. */
  id: number
  /** Template slug, the API's identifier of the template (the API sends no separate display name). */
  slug: string
  /** Email subject line. */
  subject: string
  /** HTML body content. */
  body: string
  /** What the template is for, null when the API has none. */
  description: string | null
  /** Whether the template is in use. */
  isActive: boolean
}
