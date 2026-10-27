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

- Recurring billing engine: a renewal invoice is raised 14 days before a service or domain falls due (`billing.renewal.invoice_days_before`), reminders go out 7, 3 and 1 days before (`billing.renewal.reminder_days`), and paying the invoice is what extends the service or renews the domain, exactly once. Credit is applied to renewal invoices automatically (`billing.renewal.auto_apply_credit`). The next period is raised only after the previous one is paid, at most once per daily run; a period that already started (a back-dated renewal date) gets the normal payment window, due 14 days after it is raised, instead of being born overdue.
- Overdue suspension: any overdue invoice -- renewal or not -- gets a grace period after its due date (`billing.renewal.suspend_after_overdue_days`, default 3), then only the services on its lines are suspended. An overdue invoice with no service lines now suspends nothing (it used to suspend every service of the client). Paying lifts a suspension made for an overdue invoice once no other overdue invoice covers the service, and the daily renewal run lifts any that were missed; a suspension made by an operator, or for an expired domain, is never lifted by a payment.
- An operator who unsuspends a service that is still on an invoice overdue past the grace sees it suspended again at the next 09:00 run, unless the service has `OverrideAutoSuspend` set or a `SuspendUntil` date in the future.
- Cancelling a renewal invoice does not stop the renewal: the period is invoiced again on the next daily run while it is due. To stop a service renewing, set `AutoTerminateEndOfCycle` or open a cancellation request until the admin auto-renew toggle lands. A service skipped that way whose renewal date has already passed is running unbilled; a Warning says so on every run, and one Warning per run counts auto-renewing services that have no renewal date at all (`NextRenewalAt` empty) and are never billed.
- Domains are no longer renewed for free: auto-renewal now bills the client, and the registrar is called only once the invoice is paid. A failed or unconfirmed registrar renewal alerts staff (`Notifications__AdminEmail`, falling back to `Notifications__ContactEmail`). A client renew order takes the domain off any open renewal invoice first, returning that invoice's credit; an overdue invoice that bills the domain with other items, or a renewal invoice with a payment in progress (PaymentPending) or in Collections, has to be settled first (`RENEWAL_INVOICE_OPEN`, 409). While that renew order is unpaid the daily run does not bill the domain again. An auto-renewing domain still gets the expiry warnings unless its renewal invoice is open and renewal reminders are on.
- Rollout note: on first deploy, (1) any service whose `NextRenewalAt` is already in the past gets a renewal invoice immediately, due 14 days later; (2) at the first 09:00 run every Active service on an invoice -- including invoices raised before this release -- that is already overdue by more than 3 days is suspended. Review those before deploying, and set `OverrideAutoSuspend`/`SuspendUntil` or settle the invoices where a suspension is not wanted:

  ```sql
  SELECT i."Id" AS "InvoiceId", i."ClientId", i."DueDate", it."ClientServiceId", s."OverrideAutoSuspend", s."SuspendUntil"
  FROM "Invoices" i
  JOIN "InvoiceItems" it ON it."InvoiceId" = i."Id"
  JOIN "ClientServices" s ON s."Id" = it."ClientServiceId"
  WHERE i."Status" = 'Overdue'
    AND i."DueDate" < now() - interval '3 days'
    AND s."Status" = 'Active'
  ORDER BY i."ClientId", i."Id";
  ```

  Set `Notifications__AdminEmail` (or rely on `Notifications__ContactEmail`) so renewal alerts reach a person.

  Services suspended before this release have no recorded suspension source, so no payment lifts them automatically (only an overdue suspension is lifted by payment). Review them by hand after deploying (`SELECT "Id", "ClientId" FROM "ClientServices" WHERE "Status" = 'Suspended' AND "SuspensionSource" IS NULL;`). Renewing a domain still lifts the suspension of its linked hosting service, as before. `billing.renewal.invoice_days_before` must be 1 to 90; a stored 0 is read as the default 14, with a warning in the log.
- Hero stats and badge, contact channels and the site name are read from settings (`portal.stats.*`, `portal.hero.badge`, `portal.contact.*`, `portal.site.name`); sections with no content are hidden.
- The demo site name is "Hostpanel Demo".
- The fallback site name is "Hostpanel"; no operator brand is hard-coded.
- Innovayse.Auth is upgraded to 3.1.0.
- Orders, quotes and transactions now record the currency their amounts are in, and the admin lists show each amount with it (#461).
- The admin transactions totals add every currency up in the base currency and no longer show 0 across all clients (#461).

### Removed

- The previous operator's sample storefront content.

### Fixed

- Client portal sessions no longer end when the access token expires. The tokens are kept on the server, in the Redis the stack already runs (`NUXT_SESSION_REDIS`, the same value as `REDIS_CONNECTION`), and the browser holds one small opaque, httpOnly session cookie. A refresh token larger than the 4 KB a browser keeps per cookie used to be dropped silently, which signed people out after about fifteen minutes. Signing out deletes the session on the server, and a back-channel logout ends every portal session of the person from the API (`PortalSessions__RedisConnection`). People signed in before the upgrade sign in once more.
- A failed silent sign-in now shows a sign-in prompt on the page instead of a plain guest page, and an `authuser` address on a link is passed to the identity provider as `login_hint`.
- A domain renewal no longer reports success when nothing was renewed at the registrar. An unconfigured Name.am or Namecheap integration, a Name.am error response (an error body, an unrecognised body or a 4xx), or a Namecheap answer without `Renew="true"` now fails the renewal, so a paid renewal stays unapplied, is retried daily and alerts staff. A domain with no registrar reference is refused (`DOMAIN_NO_REGISTRAR_REF`) instead of being extended locally; a paid renewal of such a domain is closed and staff are alerted to review a refund. When the registrar reports no new expiry, the current expiry is extended, so an early renewal keeps its remaining days.
- The admin menu no longer "thinks" before a page opens on a slow connection. The admin image now compresses its bundle and lets browsers keep content-hashed assets for good; a screen's code starts downloading when the pointer reaches its link (and, on a cold page load, in parallel with the session check, which itself now asks the mode and the session together); and a thin progress bar shows at the top as soon as a navigation has been waiting for its screen.
- A signed-in caller's first order no longer fails with a server error when it races the client record created on registration; the record is now saved once and the order continues with it.
- Focus styling follows `:focus-visible`.
- The billing toggle is fixed.
- Order numbers are issued from a database sequence.

### Security

- The demo runs on its own network and cannot reach production services.
- The client portal now enforces the permissions of linked users on the server: every portal action requires the permission it belongs to, the account owner keeps full access, and a refusal is a 403 with `CLIENT_PERMISSION_DENIED` (#467). The portal reads the real permissions and hides what the user cannot use.

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
