# Changelog

All notable changes to HostPanel are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Storefront content: testimonials, partners, projects and FAQs, with English, Russian and Armenian translations, served from `/api/public/{testimonials,partners,projects,faqs}`.
- Admin pages under "Storefront content" to create, edit and order the storefront content.
- Storefront content ships empty: operators enter testimonials, partners, projects and FAQs in the admin; the repository carries no site content.
- Demo mode seeds a fixed, maximal sample data set that is reset on schedule.

### Changed

- Hero stats and badge, contact channels and the site name are read from settings (`portal.stats.*`, `portal.hero.badge`, `portal.contact.*`, `portal.site.name`); sections with no content are hidden.
- The demo site name is "Hostpanel Demo".
- The fallback site name is "Hostpanel"; no operator brand is hard-coded.
- Innovayse.Auth is upgraded to 3.1.0.
- Orders, quotes and transactions now record the currency their amounts are in, and the admin lists show each amount with it (#461).
- The admin transactions totals add every currency up in the base currency and no longer show 0 across all clients (#461).

### Removed

- The previous operator's sample storefront content.

### Fixed

- The admin menu no longer "thinks" before a page opens on a slow connection. The admin image now compresses its bundle and lets browsers keep content-hashed assets for good; a screen's code starts downloading when the pointer reaches its link (and, on a cold page load, in parallel with the session check, which itself now asks the mode and the session together); and a thin progress bar shows at the top as soon as a navigation has been waiting for its screen.
- A signed-in caller's first order no longer fails with a server error when it races the client record created on registration; the record is now saved once and the order continues with it.
- Focus styling follows `:focus-visible`.
- The billing toggle is fixed.
- Order numbers are issued from a database sequence.

### Security

- The demo runs on its own network and cannot reach production services.

## [0.3.1] - 2026-10-18

### Fixed

- Client image now publishes for linux/arm64 (native arm64 runners).

## [0.3.0] - 2026-10-18

### Added

- One-command install script with automatic HTTPS.
- Docker Hub images, published per surface as `innovayse/hostpanel:api-<version>`,
  `innovayse/hostpanel:client-<version>` and `innovayse/hostpanel:admin-<version>`.
- Demo mode for public showcase instances.
- Backchannel logout, so signing out of the identity provider ends the HostPanel session too.

### Changed

- Docker layout reorganised to follow the repository rules.
- Configuration is read from environment variables only.
- The client and admin frontend images are configurable at runtime, so one image serves any deployment.

### Fixed

- Test-host leaks in the backend test suite.
- The client portal now sends the CSRF header on state-changing requests.

## [0.1.0] - 2026-05-15

### Added

- Initial open-source release under AGPL-3.0: API, client portal and admin panel.
- Issue templates and a pull request template.

### Fixed

- Backend sources were tracked as a submodule; they are now part of the repository.
- Repository URLs in the README and clone instructions.

[Unreleased]: https://github.com/innovayse/hostpanel/compare/v0.3.1...HEAD
[0.3.1]: https://github.com/innovayse/hostpanel/compare/v0.3.0...v0.3.1
[0.3.0]: https://github.com/innovayse/hostpanel/releases/tag/v0.3.0
[0.1.0]: https://github.com/innovayse/hostpanel/releases/tag/v0.1.0
