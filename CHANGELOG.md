# Changelog

All notable changes to HostPanel are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/innovayse/hostpanel/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/innovayse/hostpanel/releases/tag/v0.1.0
