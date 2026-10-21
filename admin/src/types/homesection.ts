/**
 * The sections of the storefront home page an operator can switch on or off, and the settings row
 * that holds each switch. The members are the key suffixes after `portal.sections.`; the hero is
 * not in the list because it cannot be hidden.
 */
import { SettingKey } from '@/types/setting'

/** One toggleable section of the storefront home page, in the order the card lists them. */
export enum HomeSection {
  /** Hero stats strip, or the trust bar of the aurora and nova templates. */
  Stats = 'stats',
  /** Partner logos. */
  Partners = 'partners',
  /** Why choose us, the services blocks. */
  Services = 'services',
  /** How it works. */
  Process = 'process',
  /** Plan cards, products and pricing. */
  Plans = 'plans',
  /** Comparison table of the nova template. */
  Comparison = 'comparison',
  /** Dashboard preview of the nova template. */
  Dashboard = 'dashboard',
  /** Use cases of the nova template. */
  UseCases = 'use_cases',
  /** Migration block. */
  Migration = 'migration',
  /** Portfolio projects. */
  Projects = 'projects',
  /** Testimonials. */
  Testimonials = 'testimonials',
  /** Frequently asked questions. */
  Faq = 'faq',
  /** Closing call to action. */
  Cta = 'cta',
  /** Server locations. */
  Locations = 'locations',
  /** Performance figures. */
  Performance = 'performance',
  /** Security features. */
  Security = 'security',
  /** One-click installer. */
  Apps = 'apps',
  /** Guarantees. */
  Guarantees = 'guarantees',
  /** Support channels. */
  Support = 'support',
  /** Logo strip taken from the partners. */
  Trusted = 'trusted',
}

/** The settings key that holds the switch of each section. */
export const HOME_SECTION_SETTING: Record<HomeSection, SettingKey> = {
  [HomeSection.Stats]: SettingKey.SectionStats,
  [HomeSection.Partners]: SettingKey.SectionPartners,
  [HomeSection.Services]: SettingKey.SectionServices,
  [HomeSection.Process]: SettingKey.SectionProcess,
  [HomeSection.Plans]: SettingKey.SectionPlans,
  [HomeSection.Comparison]: SettingKey.SectionComparison,
  [HomeSection.Dashboard]: SettingKey.SectionDashboard,
  [HomeSection.UseCases]: SettingKey.SectionUseCases,
  [HomeSection.Migration]: SettingKey.SectionMigration,
  [HomeSection.Projects]: SettingKey.SectionProjects,
  [HomeSection.Testimonials]: SettingKey.SectionTestimonials,
  [HomeSection.Faq]: SettingKey.SectionFaq,
  [HomeSection.Cta]: SettingKey.SectionCta,
  [HomeSection.Locations]: SettingKey.SectionLocations,
  [HomeSection.Performance]: SettingKey.SectionPerformance,
  [HomeSection.Security]: SettingKey.SectionSecurity,
  [HomeSection.Apps]: SettingKey.SectionApps,
  [HomeSection.Guarantees]: SettingKey.SectionGuarantees,
  [HomeSection.Support]: SettingKey.SectionSupport,
  [HomeSection.Trusted]: SettingKey.SectionTrusted,
}

/** Whether each section is shown when its row is empty; the portfolio is off until the operator opts in. */
export const HOME_SECTION_DEFAULT: Record<HomeSection, boolean> = {
  [HomeSection.Stats]: true,
  [HomeSection.Partners]: true,
  [HomeSection.Services]: true,
  [HomeSection.Process]: true,
  [HomeSection.Plans]: true,
  [HomeSection.Comparison]: true,
  [HomeSection.Dashboard]: true,
  [HomeSection.UseCases]: true,
  [HomeSection.Migration]: true,
  [HomeSection.Projects]: false,
  [HomeSection.Testimonials]: true,
  [HomeSection.Faq]: true,
  [HomeSection.Cta]: true,
  [HomeSection.Locations]: true,
  [HomeSection.Performance]: true,
  [HomeSection.Security]: true,
  [HomeSection.Apps]: true,
  [HomeSection.Guarantees]: true,
  [HomeSection.Support]: true,
  [HomeSection.Trusted]: true,
}
