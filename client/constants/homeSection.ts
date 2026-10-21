/**
 * The toggleable sections of the storefront home page, the setting that switches each one and
 * whether it is shown when the operator has not said.
 *
 * @module constants/homeSection
 */

import { PortalSettingKey } from '~/constants/portalSettingKey'

/** A home-page section the operator can switch on or off; the hero is not one of them. */
export enum HomeSection {
  /** Headline figures strip. */
  Stats = 'stats',
  /** Partner logos. */
  Partners = 'partners',
  /** Why choose us / services blocks. */
  Services = 'services',
  /** How it works. */
  Process = 'process',
  /** Plan cards. */
  Plans = 'plans',
  /** Comparison table. */
  Comparison = 'comparison',
  /** Dashboard preview. */
  Dashboard = 'dashboard',
  /** Use cases. */
  UseCases = 'use_cases',
  /** Migration block. */
  Migration = 'migration',
  /** Portfolio projects. */
  Projects = 'projects',
  /** Customer quotes. */
  Testimonials = 'testimonials',
  /** Questions and answers. */
  Faq = 'faq',
  /** Closing call to action. */
  Cta = 'cta',
  /** Server locations. */
  Locations = 'locations',
  /** Performance figures. */
  Performance = 'performance',
  /** Security protections. */
  Security = 'security',
  /** One-click installer. */
  Apps = 'apps',
  /** Guarantees. */
  Guarantees = 'guarantees',
  /** Support channels. */
  Support = 'support',
  /** Trusted-by logo strip. */
  Trusted = 'trusted',
}

/** The setting that switches each home section. */
export const HOME_SECTION_SETTING: Record<HomeSection, PortalSettingKey> = {
  [HomeSection.Stats]: PortalSettingKey.SectionStats,
  [HomeSection.Partners]: PortalSettingKey.SectionPartners,
  [HomeSection.Services]: PortalSettingKey.SectionServices,
  [HomeSection.Process]: PortalSettingKey.SectionProcess,
  [HomeSection.Plans]: PortalSettingKey.SectionPlans,
  [HomeSection.Comparison]: PortalSettingKey.SectionComparison,
  [HomeSection.Dashboard]: PortalSettingKey.SectionDashboard,
  [HomeSection.UseCases]: PortalSettingKey.SectionUseCases,
  [HomeSection.Migration]: PortalSettingKey.SectionMigration,
  [HomeSection.Projects]: PortalSettingKey.SectionProjects,
  [HomeSection.Testimonials]: PortalSettingKey.SectionTestimonials,
  [HomeSection.Faq]: PortalSettingKey.SectionFaq,
  [HomeSection.Cta]: PortalSettingKey.SectionCta,
  [HomeSection.Locations]: PortalSettingKey.SectionLocations,
  [HomeSection.Performance]: PortalSettingKey.SectionPerformance,
  [HomeSection.Security]: PortalSettingKey.SectionSecurity,
  [HomeSection.Apps]: PortalSettingKey.SectionApps,
  [HomeSection.Guarantees]: PortalSettingKey.SectionGuarantees,
  [HomeSection.Support]: PortalSettingKey.SectionSupport,
  [HomeSection.Trusted]: PortalSettingKey.SectionTrusted,
}

/** Whether each home section is shown when its setting has no value: all on, projects off. */
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
