# hostpanel — Project Rules

> ## ⚠ REQUIRED FOR EVERYTHING: DOCUMENTATION
>
> **Doc comments are mandatory on every file, type, member, component and composable — no
> exceptions.** XML docs in C# ([xml-docs.md](xml-docs.md)), JSDoc on the frontend
> ([jsdoc.md](jsdoc.md)), plus inline comments wherever the *reason* for the code is not
> visible from the code. Undocumented code is unfinished code.
>
> `backend/src/Directory.Build.props` sets `GenerateDocumentationFile` **and**
> `TreatWarningsAsErrors`, so an undocumented public member under `src/` is a build failure,
> not a review comment. `backend/tests/` inherits neither — documentation there is a rule the
> compiler is not enforcing.

**What it is:** the self-hosted hosting-management platform — open-source hosting billing and
automation. Clients order services, manage domains, pay invoices and open tickets; staff run
products, billing, provisioning and integrations. Production is `host.innovayse.com`. A public demo
is the same build with `Demo__Enabled=true` (`deploy:demo`); its sign-in page shows the demo accounts,
served by `GET /api/public/demo` — they are not written in `README.md`.

**This is a public, separately licensed repository.** AGPL-3.0-or-later. `origin` is **github.com/innovayse/hostpanel**, not GitLab. Anything written here
is read by people who have never seen the rest of the workspace, and the repo must build from
a clone of itself alone — that is why `Innovayse.Auth` is consumed as an optional package (only in an
`InnovayseSso=true` build, see `backend/Directory.Build.props`) rather than a project reference, and why `backend/nuget.config` restricts `auditSources` to nuget.org.

**Read first:** the six rules files beside this one — [clean-architecture.md](clean-architecture.md),
[csharp-style.md](csharp-style.md), [dotnet-format.md](dotnet-format.md),
[xml-docs.md](xml-docs.md), [jsdoc.md](jsdoc.md), [vuejs-style.md](vuejs-style.md) — then this
file. This repository is a submodule and keeps its **own** copies; the workspace's `rules/`
folder is not vendored here, and editing anything under it means committing in another
repository.

## Where this project's rules differ from the workspace — read this before your first review

After the conformance pass (A1–A7) this backend is on the workspace shape. The old list of
deliberate departures (primary constructors, `*Request` records, exception-only refusals,
feature-local `Interfaces/` folders, repositories) is **gone** — do not resurrect any of it from
memory. What remains is short:

- **`LocalAuthController` is the one controller outside the shape, and the architecture tests
  say so.** It derives from `ControllerBase`, keeps a primary constructor, resolves
  `IUserService` from an injected `IServiceProvider`, and takes six `*Request` bodies from
  `API/Auth/Requests/` — `ConfirmEmailRequest`, `ForgotPasswordRequest`, `LoginRequest`,
  `RegisterRequest`, `ResetPasswordRequest`, `TwoFactorLoginRequest`. Each is a named allow-list
  entry (`ControllerBaseTests`, `ExplicitConstructorTests`, `NoRequestTypesTests`); the entry is
  removed when the controller is converted. Why it stays: its actions call
  `IUserService`/`IJwtService` and issue the session cookie directly, and none dispatches one
  command, so there is no command for a body to bind to; the token/cookie contract on `login`,
  `2fa-login` and `logout` is what the admin SPA's refresh code and `LocalSessionCookie` depend
  on. `IUserService` is registered only in the `if (ownsItsUsers)` branch of
  `Infrastructure/DependencyInjection.cs`, so a constructor parameter would fail every route under
  `sso` mode (including the ones that answer 404 on purpose). Converting it is its own reviewed
  change, after the login flow is re-verified end to end. `HealthController` is **not** an
  exception: it derives from `BaseApiController` with `[AllowAnonymous]`.
- **Anonymous public storefront feeds.** `/api/public/{faqs,partners,projects,testimonials}` (like `/api/public/slides` and
  `/api/public/demo` before them) are `[AllowAnonymous]` read-only queries: the storefront renders them before anyone has
  signed in, so they cannot carry the usual `[Authorize]` on every action. They stay on `BaseApiController`, dispatch one
  query, return only active, already-localised records and never expose an admin field; writes remain behind `Policies.Admin`.
- **Handler and validator naming is `<UseCase>Handler` / `<UseCase>Validator`; the product rule wins over
  workspace `cqrs.md`.** Use-case names are unique across modules (`SuspendHostedAccount` in Provisioning,
  `SuspendService` in Services), and a DTO two use cases share lives once in `<Module>/Common/` (one
  `ProductPriceDto`, one `FileDownloadDto` in `Application/Common/` for every export and download).
- **No `*Request` types in Application, API (bar the six auth bodies) or Domain.** Provider inputs in Domain
  are named for what they carry (`ChargeInstruction`, `DomainRegistrationOrder`, `DomainRenewalOrder`,
  `DomainTransferOrder`, `ProvisionAccountOrder`, `SuspendAccountOrder`, `TerminateAccountOrder`);
  `NoRequestTypesTests` covers all three layers (the `CancellationRequest` entity is exempt). The wire models inside
  `Innovayse.Providers.*` and the SDK's `PaymentRequest` are third-party-facing contracts and keep their names.
- **Our own API contracts carry no `[JsonPropertyName]`**: the wire name is the property name in camelCase.
  A route id is overlaid on the bound command with `with { ... }`; there are no `*Body` types. The SDK ports are
  vendor-free too (`IHostingPanelApiClient`, `IHostingPanelV7ApiClient`; `NoVendorNamesTests` covers the SDK).
- **Errors are one `ErrorResponse` body, `application/problem+json`**
  (`Type`/`Title`/`Status`/`Detail`/`Errors`, camelCase on the wire), written by one serializer,
  `Shared/Infrastructure/ErrorBody.cs`, for every surface: `BaseApiController`,
  `ExceptionHandlingMiddleware`, the model-state factory, the auth-event writers and the rate
  limiter. The body carries only those members: the old `error`/`code` pair is gone, and both
  frontends read `type`/`detail`. Do not add the pair back.
- **The API sends the finished sentence, in the caller's language; the frontend does not
  translate codes.** Sentences live in `Application/Resources/ErrorMessages*.resx` (looked up by
  error code through `IErrorMessageResolver`) and `Shared/Resources/ValidationMessages*.resx` (middleware, binders) and
  `Application/Resources/ApplicationValidationMessages*.resx` (validator sentences; the marker is named differently from the
  Shared one so the two resource sets can never share a base name),
  in en/ru/hy, with `ResourceCoverageTests` failing on a code with no sentence or a sentence with
  no code. The marker type's full name is the resource base name — moving either half makes every
  lookup answer with the key. Culture comes from the `X-Language` header only
  (`PlatformLocalizationExtensions` clears the other providers), through
  `app.UseRequestLocalization(...)` registered **above** `ExceptionHandlingMiddleware` in
  `Program.cs`. Both frontends stamp it from the active i18n locale on every call
  (`client/composables/useApi.ts`, `admin/src/composables/useApi.ts`); the Nuxt BFF reads it from
  the browser request, validates it against `AppLocale` (`client/utils/transportLanguage.ts`) and
  forwards it on every backend call, the post-refresh retry included. `client/lib/errorHandler.ts`
  (Nuxt) and `admin/src/lib/errorHandler.ts` (admin) are the frontend readers. Only two sentences
  are written in the frontend: the offline fallback (`errors.noAnswer`), and
  `client.acceptInvite.signInRequired` (refused by the Nuxt BFF before any API call exists).

## Architecture — Clean Architecture, four layers plus a plugin SDK

`backend/Innovayse.Hostpanel.sln`, 15 projects:

```
backend/src/
  SharedKernel/                      BaseEntity (DomainEvents), AuditableEntity, DomainException, IDomainEvent
  Shared/                            BaseApiController + StatusMapping, Result/Result<T>, ErrorResponse,
                                     ErrorBody, middleware, validation policy, resources, options
  Innovayse.Hostpanel.Domain/        entities, events, Enums/ per area. References the kernel only.
  Innovayse.Hostpanel.Application/   <Feature>/{Commands,Queries}/<UseCase>/, <Feature>/Common/,
                                     Common/Interfaces/ (every port), Common/Errors/, Resources/
  Innovayse.Hostpanel.Infrastructure/ AppDbContext, migrations, <Capability>/ adapters, Integrations/
  Innovayse.Hostpanel.API/           controllers, Program.cs, composition root
  Innovayse.SDK/                     the plugin contract third parties compile against
  Innovayse.Providers.CWP/ CWP7/ Inecobank/   provisioning and payment providers (Inecobank is net8.0)
backend/tests/                       Domain, Integration, Architecture, CWP, Inecobank
client/  admin/                    Nuxt 4 portal, Vite admin SPA — each owns its Dockerfile (backend/Dockerfile too)
docker/                              compose set (shared·dev·prod·platform), nginx/, scripts/, .env.example — docker/README.md
```

- **No repositories.** Handlers use `IHostpanelDbContext` (`Application/Common/Interfaces/`)
  directly; `IUnitOfWork` is a port in `Application/Common/Interfaces/`. Domain has no `Interfaces/` folders.
- **Every application port is in `Application/Common/Interfaces/`** (`IPaymentGateway`,
  `IProvisioningProvider`, `IEmailSender`, `IIdentityProvider`, `IUserService`, …) and
  `PortPlacementTests` fails on one anywhere else. Adapters sit in Infrastructure under a
  capability folder (`Storage/`, `Email/`, `Integrations/<Provider>/`, `Auth/`); EF configurations sit in `Persistence/Configurations/`.
- **"Test Connection" runs a live probe where a provider has one.** `IIntegrationConnectionProbe`
  (a port in `Application/Common/Interfaces/`, keyed by integration slug) is implemented per
  integration in `Infrastructure/Integrations/<Provider>/` (today `NameAmConnectionProbe`, which
  logs in with the saved credentials) and every implementation is registered in
  `DependencyInjection`; `TestIntegrationConnectionHandler` picks the one whose `Slug` matches once
  every required field is stored. A probe failure is logged and answered with a fixed sentence —
  the provider's own text never reaches the response. `NameAmClient` reads the e-mail, password,
  API URL and test mode saved on the admin Integrations page over the bound options
  (`IsConfiguredAsync`), so the page now configures what requests actually use.
- **Enums live in `Enums/` namespaces** (`Domain/<Area>/Enums/`, `SharedKernel/Domain/Enums/`,
  `Infrastructure/Email/Enums/`); `EnumPlacementTests` enforces it.
- **Domain refusals are `DomainException(code, message)`** (abstract, in the kernel); the exception
  middleware turns an uncaught one into a body whose status comes from `StatusMapping.For(code)`
  and whose sentence is the localised one for that code. A handler that can foresee a refusal returns
  `Result.Failure` instead.
- **Plugins are loaded reflectively, not linked.** `PluginLoader.DiscoverAndRegister` scans
  `AppContext.BaseDirectory/plugins`, so `Innovayse.Hostpanel.API.csproj` references
  `Innovayse.Providers.Inecobank` with `ReferenceOutputAssembly=false` purely for build order and
  stages exactly two files — the provider assembly and its `plugin.json`. Never widen that to a
  `**\*.*` glob: the provider's `bin` also holds a build-time copy of `Innovayse.SDK.dll`, and
  `Assembly.LoadFrom` over it would give the process two distinct `IPaymentPlugin` types with the
  same name, after which every cast in `PaymentPluginResolver` fails. The CWP providers are
  deliberately **not** staged, so they cannot activate at runtime by accident.

## CQRS — Wolverine, conforming

Wolverine 5.41.0. `Extensions/MessagingRegistration.cs` registers discovery over the API and
Application assemblies; handlers are never registered by hand and never called directly. One
folder per use case under `Application/<Feature>/{Commands,Queries}/<UseCase>/`.

- **Handlers return `Result` / `Result<T>`** (`Shared/Models`); controllers derive from
  `BaseApiController` and return through `HandleResult`, which maps the failure code to a status
  with `StatusMapping`. every controller but the two named above derives from it (`ControllerBaseTests`); handlers and
  controllers use explicit `private readonly` fields and a written constructor
  (`ExplicitConstructorTests`).
- **Validation is a Wolverine policy.** `opts.Policies.Add<ValidationPolicy>()` puts
  `ValidationMiddleware` at position 0 of every chain; validators are registered Scoped with
  `AddValidatorsFromAssemblyContaining<AcceptInvitationCommand>()` (Scoped is load-bearing:
  validators take `ICurrentUserService`). A failure is a 400 `VALIDATION_ERROR` with the
  field errors grouped under `errors`. Validators exist on both commands and queries.
- **No identity on a message** (`NoIdentityOnMessagesTests`); handlers read it from
  `ICurrentUserService` (`Shared/MultiTenancy/`).
- **Architecture tests** (`Architecture.Tests`): `ControllerBase`, `ExplicitConstructor`,
  `NoRequestTypes`, `NoIdentityOnMessages`, `PortPlacement`, `EnumPlacement`, `NoVendorNames`,
  plus resource coverage, kernel purity, SDK contract and use-case shape. Application types carry
  no vendor names (`ICardPaymentGateway`, `GetControlPanelSsoUrl`,
  `GetHostingPanelServerInfo`/`V7`).
- **A test per use case, enforced.** `UseCaseShapeTests` fails when
  `Application/<Feature>/{Commands,Queries}/<UseCase>/` has no
  `Integration.Tests/<Feature>/<UseCase>Tests.cs` (crons and event handlers under `Commands`
  count as use cases).
- **Domain events are dispatched.** `BaseEntity.DomainEvents` collects them and
  `Infrastructure/Persistence/DomainEventDispatchInterceptor.cs` publishes them through Wolverine
  after `SaveChanges`, then clears them. `IDomainEventGate` suppresses dispatch for bulk writers
  (`MigrationPullWorker`, `DevDataSeeder`; tests use `Fixtures/SuppressedDomainEventGate`).
  `BaseEntity.AddDomainEvent(Func<IDomainEvent>)` defers building an event that needs the id
  until after the save. `ClientService.Suspend/Unsuspend/Terminate` take `hostingAlreadyUpdated`
  (handlers pass `true` after the panel call) so the event handler does not repeat it. The
  `PaymentReceived`/`InvoiceRefunded` transaction handlers are deleted; only notification
  handlers remain.

## Demo mode

`Demo__Enabled=true` turns a deployment into the public demo; nothing is a default, so
`Demo__ResetIntervalHours`, `Demo__AdminEmail`, `Demo__AdminPassword`, `Demo__ClientEmail` and
`Demo__ClientPassword` must all be set or startup is refused (`DemoOptionsValidator`). It needs
`AUTH_MODE=local`. What it does, and where:

- **Fakes, chosen once in DI.** `Infrastructure/Demo/DemoServiceRegistration` replaces every outward
  port (payment, registrar, provisioning, rates, contact notifier, monitoring) with an `internal`
  offline fake. Mail is not replaced. No adapter or handler asks whether it is a demo.
- **Guard.** A command that must not run on a demo carries `IDemoRestricted` (`Shared/Contracts/Interfaces`);
  `DemoGuardPolicy` puts `DemoGuardMiddleware` on exactly those chains and it answers 403
  `DEMO_ACTION_DISABLED`. A new dangerous command is covered by adding the marker, never an `if`.
  Sign-in in `LocalAuthController` has no command, so the guard does not see it. The demo accounts' own
  credentials are protected by one rule, `IDemoAccountGuard` (Application port, `Infrastructure/Demo/DemoAccountGuard`):
  with demo on, the email and password of `Demo__AdminEmail` / `Demo__ClientEmail` cannot change; password
  reset (`ResetPasswordCommand`) and `UpdateClientHandler`'s email change ask it and answer 403 `DEMO_ACTION_DISABLED`.
- **Currency.** The demo payment plugin accepts every currency stored (`IPaymentPlugin.AcceptsCurrency`), not only USD.
- **Data.** `IDemoDataManager` seeds idempotently at startup (instead of `DevDataSeeder`) and
  `ResetDemoDataCommand` truncates every table except the identity roles and reseeds in one
  transaction every `ResetIntervalHours`; `GET /api/public/demo` publishes the accounts and the next
  reset. Seed text names no outside product.
- **Seed.** The demo seed is fixed and deterministic: `DemoCatalogueSeeder`, `DemoPeopleSeeder` and `DemoClientProfile`
  produce the same sample data on every reset, and it is maximal, with every table populated enough to show each
  feature. The site name is `Hostpanel Demo`. The storefront contact is the one operator-set exception: optional
  `Demo__ContactEmail` / `Demo__ContactPhone` (`DEMO_CONTACT_*`) replace the fictional `.example` / `+1 555 0100` pair
  on every seed and reset; they are display only, and demo mail still goes to mailhog.
- **Network.** The demo's services sit on a demo-private network and join `innovayse_network` only under their
  `hostpanel-demo-*` names, so the demo cannot reach production's services. See the configuration section below.
- **Deploy scope.** The demo is deployed through GitLab scope `demo/hostpanel`, with its own `DEPLOY_DIR`.

## Stack

.NET 9 · Wolverine 5.41.0 · EF Core 9 + Npgsql 9 · PostgreSQL 17 · ASP.NET Identity (local
mode) + JWT bearer · `Innovayse.Auth` (cookie session for the admin SPA, sso builds only) · SkiaSharp (branding images) ·
Serilog · Scalar + Swashbuckle · Mapster · Stripe.net · MailKit · Otp.NET · Fluid.Core ·
DnsClient · Redis (sessions + data-protection key ring) · RabbitMQ · xUnit + FluentAssertions +
Moq + Testcontainers.PostgreSql 4.0.0 · Nuxt 4.3 / Vue 3.5 / Pinia / Tailwind / vitest (client,
**yarn**) · Vite + Vue 3.5 / Tailwind 4 / vitest (admin, **npm**).

Central package management: versions live once in `backend/Directory.Packages.props`; a
`.csproj` carries `<PackageReference Include="…" />` with no `Version`.

## Build, test, run

**`Innovayse.Auth` is optional at build time.** A plain clone has no credential for the private
registry, so `backend/Directory.Build.props` defines `InnovayseSso`: true when
`NuGetPackageSourceCredentials_innovayse` is set (CI, Innovayse's own deployments), false
otherwise, and forceable with `-p:InnovayseSso=true|false`. When true the package is referenced
(`Innovayse.Hostpanel.API` and `Innovayse.Hostpanel.Infrastructure`) and `INNOVAYSE_SSO` is
defined; when false nothing private is restored and the build is `AUTH_MODE=local` only — a
standalone build started with `AUTH_MODE=sso` throws at startup from `AddHostpanelAuthentication`
naming the property. Every file that touches the package is inside `#if INNOVAYSE_SSO`:
`API/Program.cs` (`UseInnovayseAuth`/`MapInnovayseAuth`), `API/Extensions/AuthenticationRegistration.cs`
(`AddInnovayseAuth`, the claims transformation), `Infrastructure/Auth/SsoSessionClaimsTransformation.cs`
and `SsoModeAuthenticationSchemeTests`. The cookie scheme name is the one constant
`AuthenticationRegistration.SsoCookieScheme` (a placeholder in a standalone build), which
`PolicyRegistration` reads; do not name `Innovayse.Auth.CookieSessionHandler` anywhere else.
`backend/Dockerfile` passes `-p:InnovayseSso=true` to restore and publish when `AUTH_MODE=sso`
is the build arg, so a missing credential fails the build rather than yielding an image that
refuses to boot. The GitHub workflow `.github/workflows/ci.yml` builds the standalone flavour and
runs the Domain, Architecture, CWP, Inecobank and Integration suites.

```bash
./dev up hostpanel                # from the workspace root, when working inside the workspace
```

To build without touching the dev stack — **run docker through PowerShell, not Git Bash**,
which rewrites `-w /src` into `C:/Program Files/Git/src` and exits 125:

```powershell
docker run --rm `
  -v "C:\Users\Dell\Desktop\Projects\innovayse\innovayse-workspace\hostpanel\backend:/src" `
  -v "C:\Users\Dell\.nuget\packages:/root/.nuget/packages" `
  -w /src mcr.microsoft.com/dotnet/sdk:9.0 bash -lc "dotnet build Innovayse.Hostpanel.sln"
```

**A cold full solution build takes about eight minutes** — 7m43s measured, 14 projects, 0
warnings. It prints nothing for minutes at a stretch and **appears to stop after
`Innovayse.Hostpanel.Infrastructure`**; that project is simply the slow one, and `Innovayse.Hostpanel.API`, the
test projects and the rest land minutes later. Only treat it as hung well past ten minutes.
Three attempts in a row abandoned this build as stalled and fell back to a narrower one, losing
the cross-project signal it exists for.

The host package cache covers the private feed for a local build: `nuget.config`'s
`packageSourceMapping` sends only `Innovayse.Auth` to the GitLab registry, and that package is
already cached. Where a credential *is* needed it is
`NuGetPackageSourceCredentials_innovayse=Username=<gitlab-account>;Password=<PAT>` — the exact
spelling NuGet reads; `gitlab-ci-token` as the username is for CI job tokens only and answers
401 with a personal token. The feed is addressed by **numeric project id**, not by the encoded
namespace path, which answers 400 to everything and reports as NU1301.

**`Innovayse.Hostpanel.Integration.Tests` needs Testcontainers**, and that means the docker socket plus
both `-e TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal` and
`--add-host=host.docker.internal:host-gateway`. Without them the resource reaper cannot start
and every test in the suite fails with a stack trace that looks nothing like a networking
problem. **Do not pass `--no-build` to the integration suite** — it silently produces no output
and exits 0 whether or not the assembly is there. The Domain, Architecture, CWP and Inecobank suites are fine with it.

Containers: `hostpanel-api` (5148), `hostpanel-client` (3000, published 3001),
`hostpanel-admin`, `hostpanel-db`, plus `hostpanel-redis`, `hostpanel-rabbitmq`,
`hostpanel-mailhog` locally. Services address each other by **network alias**
(`hostpanel-api`), never by the bare `api` — that name also belongs to `innovayse-main`'s
backend on the shared network, and Docker's DNS round-robins between the two.

## Operational scripts at the repo root (`scripts/`)

- `scripts/check-client-owner-ids.sh [container]` — read-only check of what `clients.UserId` points at
  (the local `AspNetUsers.Id` or the SSO subject), run before and after the SSO cut-over. Defaults to the
  `hostpanel-db` container; over ssh as `ssh prod 'sh -s' < scripts/check-client-owner-ids.sh`.
- `scripts/pack-cwp-plugin.sh` — publishes `Innovayse.Providers.CWP` (Release, net8.0) on the host, strips
  PDB/XML and framework DLLs, adds `plugin.json` and writes `dist/innovayse-cwp.zip`.

Both are `*.sh`, which `.gitattributes` (`*.sh text eol=lf`) pins to LF: the ssh pipe above fails with
"Illegal option" on a CRLF copy.

## CI is on GitLab. Pull requests are on GitHub. Both halves are load-bearing.

`origin` is GitHub; merges happen there. `.github/workflows/mirror-to-gitlab.yml` pushes
**only `main` and `develop`** to `gitlab.com/innovayse/hostpanel`, whose `.gitlab-ci.yml`
starts the pipeline. The consequences are not obvious and cost time every few months:

- **A feature branch never gets a pipeline.** Nothing is mirrored, so nothing builds or tests
  it. A pull request into `develop` shows no checks because there are none to show, not
  because they passed.
- The one GitHub job that *does* run on a pull request is
  `.github/workflows/branch-flow.yml`, a required status check on `main` that refuses any
  source branch other than `develop` or `hotfix/*`. It enforces what neither platform has a
  setting for.
- Tests, image builds and both deploys live in `.gitlab-ci.yml`. `deploy:staging` runs on
  `develop` against LocalServer; `deploy:prod` is **manual** on `main` against ProdServer.
- `.gitlab-ci.yml` inlines its templates. This repo cannot see the workspace's shared
  `ci/templates.yml`, so the hidden jobs are copied in. Change one here and the workspace
  copy does not move with it.

**`deploy:prod` builds the images on the production host** — it consumes no registry images
and declares `needs: []`, because the build jobs want a `builder` runner this project's prod
host does not have. It dumps the database with `pg_dump -Fc` into `backups/` **before**
anything is rebuilt and fails the deploy if the dump fails: the API applies pending EF
migrations on startup, so `up -d` is the moment the schema changes, and a deploy that cannot be
undone should not start. The last ten dumps are kept.

`nginx` is deliberately left out of both deploys' service lists. It publishes 80/443, which on
these hosts belong to the shared Nginx Proxy Manager; naming it would fail the whole deploy.
Its own nginx exists for a standalone install.

## Configuration and the two switches that decide what this product *is*

- **`AUTH_MODE` (`sso` | `local`).** `sso` authenticates against innovayse-sso and reads every
  account out of it through `/api/service/*` with `SSO_SERVICE_KEY`; `local` is the standalone
  open-source path where hostpanel owns its own user table and registers no SSO scheme at all.
  Only this product implements both, and most "why is there a second way to do this" in the
  auth code is this switch. Under `sso` the API refuses to start without
  `CLIENT_HOSTPANEL_SECRET` rather than come up and hand every visitor an anonymous session.
- **`ASPNETCORE_ENVIRONMENT` is set by the deploy job, never by a host's `.env`.**
  `docker/docker-compose.dev.yml` passes it through from `docker/.env`: `Development` is right for a laptop and was what
  deployed hosts silently inherited. A Development API serves developer exception pages and
  treats an absent `EncryptionKey` as allowed rather than fatal — which is how server
  credentials and client-service passwords reached the database in plain text.
  Every deployed tier (staging, prod, demo) sets `Production`, per `rules/environments.md`; the tier
  is named by the job's `environment:`, never by `ASPNETCORE_ENVIRONMENT`.
- **Demo mode (`Demo__Enabled`).** Set through `DEMO_*` env in `docker/docker-compose.demo.yml`, an overlay
  placed last in the chain (`shared` + `prod` + `platform` + `demo`, project `hostpanel-demo`). It swaps the
  payment, registrar and provisioning adapters for fakes, sends mail to the demo's own mailhog and resets
  data every `Demo__ResetIntervalHours`. Everything stateful is `hostpanel-demo-*` so it can sit beside prod; its db, broker, redis and mailhog live on a demo-private network and only the api, client and admin (compose services `hostpanel-demo-*`, the names the edge proxy uses) also join the dedicated external network `hostpanel_demo_edge`, **never** `innovayse_network`, so the demo can neither resolve nor reach any production container (the edge proxy `innovayse-edge` is the one production-side container on both networks; `deploy:demo` creates the network when missing, with the **fixed subnet 172.29.0.0/16**, which the edge's `nginx.conf` denies on every host except the two demo ones, so a demo container cannot reach production by sending a production `Host` header; the subnet lives in `deploy:demo`, `edge-proxy/nginx.conf` and `docker/README.md`, and changes in all three together); `deploy:demo` uses its own `DEPLOY_DIR` (a CI/CD variable scoped to `demo/hostpanel`), never prod's;
  the demo passwords are `:?`-required like any secret. Deployed by the manual `deploy:demo` job; it needs
  `AUTH_MODE=local`. Fake adapters live behind the existing ports, never in a code path guarded by a literal.
- **Settings bind into a sealed `*Options` class naming its own `SectionName`** —
  `BillingOptions`, `CPanelOptions`, `NamecheapOptions`, `NameAmOptions`, `StripeOptions`, each
  in an `Options/` folder inside the feature or integration that owns it.

## Storefront content — in the database, not in the code

Testimonials, partners, projects and FAQs are rows in the database, each with English, Russian and Armenian
translations, managed in the admin under **Storefront content**. The public endpoints are
`/api/public/{testimonials,partners,projects,faqs}`. The hero stats and badge read `portal.stats.*` and
`portal.hero.badge`; the contact channels read `portal.contact.*`; the site name reads `portal.site.name`. The
client hides any section that is empty, so an empty table is a valid state, not an error. Production runs a
one-time import of the previous static content; the demo does not, because its seed is fixed.

## The frontend: three templates, one live header, and one env-var trap

`client/` is Nuxt 4 with SSR on the public site and `ssr: false` route rules for `/client/**`,
`/cart/**` and `/checkout/**`.

**The header you are looking at is almost certainly not `components/layout/Header.vue`.**
`layouts/default.vue` renders `<component :is="header" />`, and `header` comes from
`useTemplate().slot('header')`, which resolves through `templates/registry.ts`. There are three
templates — `aurora`, `nova`, `classic` — and the active one is decided by, in order: the
operator's `portal.template` admin setting, then `NUXT_PUBLIC_PORTAL_TEMPLATE`, then the
built-in default **`aurora`**; anything unrecognised falls back to `aurora` rather than
rendering nothing. `components/layout/Header.vue` is reached only through
`templates/classic/layout/Header.vue`, a two-line wrapper — so on a default deployment it is
live code that nothing renders. Editing it and reloading changes nothing, and that has cost
real time. Start from `templates/<active>/layout/Header.vue`.

`nova` deliberately reuses `aurora`'s `Domains.vue` and `Checkout.vue` — the checkout is the
ordering path and a second copy is a second place for a payment bug to live. If a shared aurora
page grows a new section import, add it to `AURORA_SHARED_COMMERCE` in `nuxt.config.ts` or nova
starts downloading the whole `template-aurora` chunk.

**Runtime config in a production image.** A production Nitro build freezes `runtimeConfig`
defaults — `nuxt.config.ts` is not re-evaluated at container start as it is in dev. Nitro's own
escape hatch is reading `NUXT_<KEY>` / `NUXT_PUBLIC_<KEY>` at runtime and overriding the frozen
default, so `docker/docker-compose.prod.yml` spells every one of them that way. **The plain names used
in `docker/docker-compose.dev.yml` are silently ignored on a deployed host** — that is how
`NUXT_PUBLIC_PORTAL_TEMPLATE` and the apps launcher stayed permanently at their build-time
values on production with no `.env` edit able to change them.

**The client image has no build args.** The app-launcher `<script src="…/widget/header.js">` tag is
added by `plugins/header-widget.ts` from `runtimeConfig.public.mainUrl`, so `NUXT_PUBLIC_MAIN_URL`
is plain runtime env. `site.url` is read at runtime by nuxt-site-config from `NUXT_PUBLIC_SITE_URL`
(set in `docker-compose.prod.yml`); the esbuild `drop` depends only on `NODE_ENV`, not on a
deployment. Do not add a `process.env` read outside `runtimeConfig` that differs per deployment.

That widget is served by **innovayse-main**, not by this product. It must be HTTPS on an HTTPS
deployment or the browser blocks it as mixed content and the client dashboard's hydration
hangs.

**The `dark` / `light` class on `<html>` has exactly one writer: `client/plugins/color-mode.ts`.**
It used to have three — a client-only plugin that forced `dark` on every public page, a blocking
script in `app.vue` that did the same, and `useAppColorMode()` restoring a `localStorage` value
on mount — which is why the operator could not ship a light storefront and a light-mode visitor
got a dark flash on every load. The resolution rule is in `client/utils/colorMode.ts` and reads,
in order: the visitor's `color-mode` **cookie**, then the operator's `portal.theme.default`
(`light` | `dark` | `system`), then the device. A cookie or a fixed operator mode is rendered
into the HTML on the server; only `system` with no cookie emits an inline head script, because
both template stylesheets declare their dark palette on bare `:root` and a class-less page is
dark until something says otherwise. Do not add `document.documentElement.classList` calls
anywhere, and do not read the mode from anything but `useAppColorMode()`.
`portal.theme.user_toggle = false` hides `UiThemeToggle` and makes `toggle()` a no-op.

**The logo a template renders comes from `useBrandLogo()`, and nowhere else — and a header
renders both wordmarks, swapped by CSS, never one picked in JavaScript.** `light` and `dark` are
the two wordmarks with their fallbacks (each falls back to the other); a header renders
`<img :src="light" class="dark:hidden">` beside `<img :src="dark" class="hidden dark:block">`
when `hasDarkVariant`. The reason is hydration: under the `system` default the server does not
know the mode and renders as dark, and **production Vue does not patch an attribute that
disagrees after hydration** — a `:src` chosen from `isDark` left a light-device visitor looking
at the dark logo, and a `v-if` on the toggle's icon left them looking at the sun. Both were seen
in a browser before the swap. `logoOnDark` is for surfaces that are always dark whatever the
page mode (the sign-in pages, classic's header and footer); `mark` is `portal.logo.mark` with the
same fallback chain; `logo` is the mode-aware pick for browser-only code; and `alt` is
`portal.site.name`, because the logo *is* the site name and the `alt=""` the headers shipped
with told a screen reader the brand was decorative. The rule itself is
`client/utils/brandLogo.ts`, unit-tested. `useSiteIdentity()` is the only source of the site
name; the literal `Hostpanel` survives as its fallback and in the locale copy, and nowhere else
in a `.vue` or `.ts` file.

**Every colour a template ships is a `var()` with the template's literal as its fallback —
a literal on its own is a colour the operator cannot change.** `client/plugins/brand-tokens.ts`
renders one `<style id="brand-tokens">` into `<head>` from `portal.brand.primary` / `.accent`
and the two typefaces: the eleven-step `--brand-primary-N` / `--brand-accent-N` scales (as
`r g b`, for Tailwind's `<alpha-value>`), `--brand-on-primary` / `--brand-on-tint` /
`--brand-on-accent(-tint)` (white or near-black, decided per colour), and
`--font-heading` / `--font-body`. It emits **nothing** when nothing is set. Tailwind's
`primary`/`secondary` scales, the `brand` gradient, aurora's `--ac1/--ac2` and nova's
`--n-brand/--n-accent/--n-on-*` all read those variables with today's literal as the fallback,
which is what makes "unset renders exactly what it did" true; check it by reading the computed
tokens with the rows empty before touching any of them. Text on a filled brand surface is
`text-on-primary` / `text-on-tint` / `text-nova-on-brand` / `text-nova-on-accent`, never
`text-white` or a literal — a brand yellow needs dark text. The ramp lives in
`client/utils/brandPalette.ts` (OKLCH, gamut-mapped by reducing chroma, the picked colour pinned
at 500); `admin/src/lib/brandPalette.ts` is a byte-for-byte copy below its header because the
admin cannot import across the repository (`brandPalette.shared.test.ts` fails on drift), and
`Application/Notifications/Rendering/BrandPalette.cs` is the e-mail renderer's port with the same
reference numbers in `BrandPaletteTests`. Change the client file first; then the other two.

**Third-party scripts come from settings and never from a literal.** The tag-manager container
(`portal.analytics.gtm_id`) loads in `plugins/tracking.client.ts` only when set **and** the
visitor's `cookie-consent` is `all` — `useCookieConsent()` is the one owner of that cookie, the
banner writes it, the loader watches it. The live-chat widget (`portal.chat.provider`,
`.base_url`, `.website_token[.ru|.hy]`) loads in `plugins/live-chat.ts` when all three are set,
without waiting for consent, because it is how a visitor reaches a person. `app.vue` adds no
script at all; the vendor's own ids that used to live there are gone from the repository and
belong in the vendor's deployment `.env`.

**Every e-mail renders with `brand.*` and `site.*` in scope.** `TemplateRenderer` adds them
beside the caller's model (the caller's own member of the same exact name wins), read on each
render by `SettingsEmailBrandingProvider` from the same `portal.*` rows, with the logo made
absolute against `ClientBaseUrl`. A stored template that wants the brand uses
`{{ brand.primary }}`, `{{ brand.primary_dark }}`, `{{ brand.on_primary }}`,
`{{ brand.accent }}`, `{{ brand.logo_url }}` and `{{ site.name }}`; the seeded password-reset
layout does, but a row seeded before this existed keeps its literals until an operator re-saves it.

**`PortalSettingKeys` (Domain) is the backend's one list of `portal.*` keys** — the seeder's
`Defaults` and the anonymous endpoint's `Public` set are derived from the same array, and
`AllowedValues` is the vocabulary `UpdateSettingHandler` enforces (`INVALID_SETTING_VALUE`, 400).
The Nuxt BFF's `PUBLIC_KEYS` in `client/server/api/portal/public/settings.get.ts` is a **manual
twin** of that list; it cannot import the class, so a new key is added in both places and in
`nuxt.config.ts`'s `runtimeConfig.public` **and** both compose files' `NUXT_PUBLIC_*` lines.
`PortalSettingKeysTests` fails if `Defaults` and `Public` drift; nothing tests the BFF twin.

### The client's shape (Nuxt exception, conformance C complete)

`client/` is the Nuxt exception the workspace `frontend.md` allows: it has public, crawlable pages
(SEO, SSR) beside the session-gated `/client/**` area, so it builds on Nuxt where the other
products are Vite SPAs. Everything else in `rules/frontend.md` holds in full; the Nuxt specifics
are these, and **no part of the client is exempt any more — the ratchet that once scoped the
rules is deleted and the ESLint conformance block applies to every `.vue` and `.ts` file.**

- **Data flow, as everywhere:** `.vue` → `stores/<area>.ts` → `composables/apis/use<Area>Api.ts` →
  `composables/useApi.ts` (and `apiFetch`) → Nitro BFF (`server/api/portal/**`, every route through
  `internalApiCall`) → the C# API. A `.vue` file never calls `useApi`, `apiFetch`, `$fetch`,
  `useFetch` or an api composable, holds no `catch` and never imports `lib/errorHandler`;
  server-rendered data is `await useAsyncData(key, () => store.loadX(), { watch: [locale] })`.
  A store is the only caller of `apis/`, and exposes `error` and `fieldErrors` for what it refused.
  `useApi.ts` imports no store. The gates: ESLint `no-restricted-imports` and
  `scripts/check-layers.mjs` (auto-imports hide the calls from import rules, so the script reads
  the source for them too).
- **Two hops, one language header.** The browser stamps `X-Language` (from the i18n locale,
  validated by `utils/transportLanguage.ts`) on every call to the BFF; the BFF forwards it to the
  API, and an unknown value becomes `en`. `Accept-Language` is not used anywhere.
- **Errors are chosen in one place.** `lib/errorHandler.ts` (`apiErrorMessage`, `apiFieldErrors`,
  `apiErrorCode`) orders `errors[field]`, then `detail`, then `title`, then the client's own
  `errors.noAnswer`. The BFF writes no sentence (`scripts/` and `tests/server/contract.test.ts`
  fail on a literal `statusMessage:`); a missing route parameter is a coded 400
  (`requireRouteParam`). `scripts/check-no-legacy-error-fields.mjs` rejects a read of the old
  `error`/`code` pair outside the one allow-listed OAuth call.
- **The kit is `components/ui/` (`Ui*`), and screens are composed from it.** `pages/**`, `layouts/**`
  and `templates/*/{pages,sections,layout}/**` carry no raw tag at all; an area's own components
  (`components/<area>/`) compose the kit and may not hold a card-styled `div` or an interactive
  raw element. A new visual need is a new `Ui*` first. Styling is Tailwind classes written out
  literally and theme tokens (`assets/css/ui.css`, `tailwind.config.ts`); per-item colours that
  are data go through `UiAccent`. No `<style>`, no `style=`, no `bg-[#...]`, no `v-html` —
  operator HTML is rendered by `UiRichText` (`utils/ui/richText.ts`, an allow-list tokenizer that
  builds nodes with `h()`; it has an XSS-corpus test). Every field has a `placeholder`, every
  interactive element a `data-testid` (`<area>-<element>`).
- **Types and closed sets.** One concept per file in `types/`, imported with `import type`;
  every closed value set is an enum (`AppLocale`, `TemplateName`, `TicketStatus`, ...), never a
  string union or a repeated literal. The language list is `APP_LOCALES` (`constants/appLocales.ts`),
  derived from `AppLocale`; nothing spells `['en','ru','hy']` out. Server and client share
  `types/` and `utils/transportLanguage.ts` only.
- **Site identity.** The site name is `useSiteIdentity().name` (the operator's `portal.site.name`);
  `Innovayse` survives as its fallback and in the locale copy, not in page titles, footers or alt
  text. The billing portal's address is `BILLING_PORTAL_URL` (`NUXT_PUBLIC_BILLING_PORTAL_URL` in
  the production compose file, `runtimeConfig.public.billingPortalUrl`); every link built from it
  is guarded for the unset case.
- **Locales.** `locales/{en,ru,hy}/<area>.json`, all three in the same change, loaded by
  `plugins/i18n.ts`. `yarn lint:i18n` fails on a key missing from or extra in any language, an
  unused key, or a literal `t('...')` with no `en` entry. A server-decided sentence is never
  copied into a locale file.
- **Tests mirror the source.** `tests/` mirrors `components/ stores/ composables/ lib/ server/utils/`
  one test per file (`tests/coverage-shape.test.ts` fails on a missing mirror; its `KNOWN_GAPS`
  list may only shrink), plus `tests/pages`, `tests/layouts`, `tests/templates`,
  `tests/locales/parity.test.ts`. Real kit and real Pinia (`tests/helpers/mountWithKit.ts`,
  `stubServer.ts`/`stubTransport.ts` for a real HTTP stub under `$fetch`); Nuxt auto-imports are
  stubbed once in `vitest.setup.ts`. `e2e/` is Playwright, one spec per flow, `data-testid`
  selectors only, against the production build with the C# API replaced by
  `e2e/support/stub-backend.mjs` (fixtures in `fixtures.mjs`; `POST /__e2e/script` scripts a
  refusal, `/__e2e/settings` switches the template, `/__e2e/requests` proves what the backend
  received, including `X-Language`).
- **Gates, in `.gitlab-ci.yml` `client:test` then `client:e2e` (all blocking):** `yarn lint`
  (ESLint conformance block + `check-layers` + legacy-field gate), `yarn lint:i18n`,
  `yarn audit:conformance --strict` (every metric zero), `yarn typecheck` (`nuxt typecheck`),
  `yarn test --maxWorkers=2`, `yarn build`, then `yarn e2e`. On this laptop vitest needs
  `--maxWorkers=2`.

### The admin's shape (Vite SPA, conformance B complete)

`admin/` is the workspace's standard Vite SPA (`rules/frontend.md`) with no exception: the operator
back office is session-gated, so it has no crawlable pages. **No part of it is exempt — the ratchet
that once scoped the rules is deleted, and `strictRules` in `eslint.config.ts` applies to every `.vue` and
`.ts` file under `src/`** (the one carve-out is `lib/brandPalette.ts`, a generated byte-for-byte copy).

- **Flat layout, no `modules/`.** `src/views/<Page>View.vue` (routed screens), `src/layouts/`,
  `src/components/ui/` (the kit), `src/components/<area>/` (screen-specific parts composed from the
  kit), `src/stores/<area>.ts`, `src/composables/apis/use<Area>Api.ts`, `src/composables/useApi.ts`,
  `src/composables/use*.ts`, `src/types/<concept>.ts`, `src/lib/` (pure helpers: `errorHandler`,
  `runAction`, `translate`, `format`, `money`, `tones`, `icons`, `reports*`), `src/utils/ui/` (TypeScript
  the kit shares), `src/middleware/auth.ts` (the router guard), `src/locales/`. The API is
  same-origin (`/api/...`), so the only per-deployment value is the base path, and it is runtime:
  Vite builds with a relative `base`, `public/config.js` sets `window.__APP_CONFIG__.basePath`
  (read through `lib/runtimeConfig.ts`), and the prod image's `docker/nginx/admin-entrypoint.sh`
  rewrites `config.js` and `<base href>` from `ADMIN_BASE_URL_PATH` at start. No `VITE_BASE_URL`.
- **Data flow, as everywhere:** view → `stores/<area>.ts` → `composables/apis/use<Area>Api.ts` →
  `composables/useApi.ts` → HTTP. A view never calls `useApi`, an api composable or `lib/errorHandler`
  and holds no `catch`; a store is the only caller of `apis/`, runs every request through
  `runAction(error, task, fieldErrors?)` and exposes `error` and `fieldErrors`. `useApi` is the only
  module that knows transport: base path, CSRF header, `X-Language` (from the active i18n locale, on
  every call), `ApiError` carrying the problem+json body, `buildQuery`. It imports no store, and the
  SSO redirect and end-session address live in `useAuthApi`, not in `useApi`. ESLint
  `no-restricted-imports` enforces the layering.
- **Errors are chosen in one place.** `lib/errorHandler.ts` returns `detail`, then `title`, then the
  client's own `errors.noAnswer`; `errors.signInLoop` is the second client-authored sentence. There
  is no status-to-sentence table. The admin is localised in en/ru/hy like the client — it is no
  longer an English-only panel.
- **The kit is `components/ui/` (`Ui*`), and screens are composed from it.** Views, layouts and area
  components carry no raw `button/input/select/textarea/table/form/dialog/a` (`vue/no-restricted-html-elements`);
  a new visual need is a new `Ui*` with its test first. No `<style>`, no `style=`, no `v-html`
  (`UiIcon` builds SVG nodes with `h()` from a whitelist); Tailwind 4 classes are literals and colours
  are theme tokens in `assets/main.css`. Every input-like has a `placeholder` (`local/require-placeholder`)
  and every interactive element a `data-testid` (`<area>-<element>[-<id>]`). chart.js is only used
  behind `UiChart`; reports share one scaffold (`ReportPage`, `ReportTable`, `ReportQueryPage`).
- **Closed sets are enums or `as const` objects with a derived union** (`AuthMode`, `TicketStatus`,
  `InvoiceBulkAction`, `CancellationType`, `UiTone`, ...); tones for API status words live in
  `lib/tones.ts`. A screen whose feature is not built (affiliates, addons, disputes, offline card
  processing) renders `ComingSoonPage` — never placeholder records.
- **Locales.** `src/locales/{en,ru,hy}/<area>.json`, one file per area, the file name being the first
  key segment (`nav.json` holds `nav.*`); `locales/messages.ts` assembles them with
  `import.meta.glob` and `i18n.ts` loads the result. All three languages change together.
  `npm run lint:i18n` fails on a key missing from or extra in any language, an unused key (a
  `` t(`area.${x}`) `` prefix counts as use of the keys under it) and a literal `t('...')` /
  `translate('...')` with no `en` entry; `tests/locales/parity.test.ts` also checks values and
  `{placeholders}`. A server-decided sentence is never copied into a locale file.
- **Tests mirror the source.** `tests/` mirrors `src/` (components, views, layouts, stores,
  composables, `composables/apis`, lib, middleware, utils), with a real kit, real Pinia and the API
  stubbed at the network level (`tests/helpers/{fetch,routes,mount}.ts`), never a mocked store or
  `useApi`. `tests/coverage-shape.test.ts` fails when a component, layout, view, store, composable,
  lib module or guard has no test importing it. `tests/lint/strictRules.test.ts` pins the lint rules.
  On this laptop vitest needs `--maxWorkers=2`.
- **e2e.** `admin/e2e/`: Playwright, one spec per flow, `data-testid` selectors only (`fieldOf` for
  inputs), against `vite preview` of a production build with the API replaced by `page.route`
  (`e2e/support/stubApi.ts`: stateful session endpoints, recorded calls, `unhandled` for anything
  nobody stubbed; fixtures typed from `src/types/` in `e2e/fixtures/`). Flows cover sign-in (password,
  SSO, sign-out), dashboard, clients (list, create, detail), billing (invoices, quotes, transactions),
  orders/services/domains, servers, support (tickets, announcements, knowledgebase), settings,
  currencies, slides (including the unsaved-changes guard), integrations, plugins, migration,
  reports, an API outage and the language switch.
- **Gates, in `.gitlab-ci.yml` `admin:test` then `admin:e2e` (blocking):** `npm run lint:ci`
  (oxlint + ESLint, non-fixing), `npm run lint:i18n`, `npm run typecheck` (`vue-tsc -b --force`),
  `npm test -- --maxWorkers=2`, `npm run build-only`, then `npm run e2e`. `npm run audit`
  (`scripts/audit.mjs`) prints the raw-element, `function`-declaration, `style=`, `v-html`, `any`
  and missing-placeholder counts, which must be zero.

## Tests

`backend/tests/` has five projects: `Domain.Tests`, `Integration.Tests`, `Architecture.Tests`,
`Innovayse.CWP.Tests`, `Innovayse.Inecobank.Tests`. **`Application.Tests` and
`Infrastructure.Tests` no longer exist** — use-case tests are `Integration.Tests/<Feature>/<UseCase>Tests.cs`
(Testcontainers Postgres, fakes in `Fixtures/` for external ports only), and adapter tests live in
`Integration.Tests/Adapters/{Auth,Billing,Integrations}`. Do not recreate either project.
`Integration.Tests/Security/LeakGuardTests` guards against leaking secrets/internals in
responses; `Fixtures/StubHttpServer` is the in-process HTTP fake for external APIs. The admin SPA
has vitest (`npm test`, `admin/tests/`) and Playwright (`npm run e2e`, `admin/e2e/`) — see "The admin's shape".
`Integration.Tests/Persistence/ModelDiffBaselineTests` is the schema guard (the spec's "snapshot unchanged" test): the EF differ's operations between the migrations snapshot and the live model must equal `Fixtures/model-diff-baseline.txt` (`(no differences)` since the ConformanceFixes migration), so an entity change without a migration fails.

**The Integration suite runs in four shards, one at a time, because one `dotnet test` host holds
the EF model and a Postgres container for every test class and grows to several gigabytes, and the
12 GB builder is shared with other products.** Two half-suite shards running together were
OOM-killed on every attempt. The shards (the same split as `.github/workflows/ci.yml`) are
`backend:test:integration:a` to `:d`, share a `resource_group` so only one runs at a time, and split
by namespace with `--filter "FullyQualifiedName~Integration.Tests.<Ns>."`: A is Adapters, Admin,
Audit, Auth, Clients, Common, Demo, Health, Identity, Security, Shared; B is Billing, Domains; C is
Reports, Slides, Support, Faqs, Partners, Projects, Testimonials; D is Migration, Notifications, Orders, Persistence, Products,
Provisioning, Services. **A new top-level folder in `Integration.Tests/` must be added to one
shard in `.gitlab-ci.yml` and in `ci.yml`, or its tests run nowhere.**

## Packages worth knowing

- **Branding images are rendered with SkiaSharp (MIT)**, in
  `Infrastructure/ImageProcessing/SkiaBrandingImageProcessor.cs`, behind the
  `IBrandingImageProcessor` port. It replaced `SixLabors.ImageSharp`: 2.1.x was the last Apache-2.0
  line, stopped receiving fixes and carried high-severity advisories with no 2.x patch, and 3.x is a
  split licence this public AGPL repository cannot hand to downstream users. The ImageSharp
  `NuGetAuditSuppress` block is gone with it, and `.github/dependabot.yml` ignores that package.
  `SkiaSharp` and `SkiaSharp.NativeAssets.Linux.NoDependencies` (so the Alpine image needs no apk
  packages) must stay on one version. What the safety of an upload still rests on is unchanged: the
  processor sniffs magic bytes first (PNG/JPEG/GIF/WebP/BMP only — Skia would otherwise also try
  ICO, WBMP and raw codecs), reads the header to enforce the edge and pixel ceilings before decoding
  any pixels, refuses SVG, and encodes PNG only. Keep that true. Downscales use mipmapped
  (trilinear) sampling on purpose — Skia's cubic filter aliases a 1024px mark reduced to a 16px
  favicon. A refusal is a `Result` failure (`BRANDING_IMAGE_INVALID` / `BRANDING_IMAGE_TOO_LARGE`);
  the precise cause is logged, never returned.

## Debt — recorded, not endorsed

Never spread one of these, and never restructure as a side effect of a feature.

- **`LocalAuthController`** — see the top of this file.
- **Vendor names that remain, on purpose.** Integration slugs (`stripe`, `cwp`, ...) in
  `IntegrationSlugs` / `BuiltInPaymentModules` and the `Integrations/<Vendor>/` adapter folders are
  plugin identifiers: they are stored in settings rows, sent in URLs (`/api/admin/integrations/stripe`)
  and chosen in the admin UI, so renaming them would break stored configuration. They are the only
  place a vendor is named. Persisted *data* is neutral: `Client.CardGatewayCustomerId` and the ledger
  payment method `Card` (the `ConformanceFixes` migration renamed the column and rewrote old
  `Stripe` ledger rows).
- **Table names are snake_case (`ticket_tags`, `invoices`, ...) except `Orders`/`OrderItems`.**
  Snake_case is the overwhelming existing convention and the migrations carry it; moving to
  PascalCase is a whole-schema rename of ~50 tables and is its own reviewed change, so a new table
  follows its neighbours until then.
- **`ClientRegisteredIntegrationEvent` carries five parameters nothing fills.**
  `LocalAuthController.RegisterAsync` publishes four arguments and stops, so `IpAddress`,
  `UserAgent`, `DeviceType`, `OperatingSystem` and `Browser` arrive null and `Client.Create`
  writes five mapped, migrated columns from them. Finish it by capturing all five from the
  request, not by trimming the record. Wolverine runs with discovery only — no broker — so the
  event never leaves the process.
- **`MigrationPullWorker` is a very large file in the Application layer.** Its transport is
  already behind `IMigrationSource`, so a fake can drive it without a web server; what is left is
  size. Splitting it is its own piece of work.
- **The migration job model is slim.** `ImportBatch` and `PingMigrationJob` are removed;
  `CreateMigrationJobCommand` selects entities with `MigrationEntitySelectionDto`.
- **`Infrastructure/Migration/` cannot exist.** A namespace `Innovayse.Hostpanel.Infrastructure.Migration`
  shadows EF Core's `Migration` base class for every file in `Infrastructure.Migrations` (CS0118).
  The migration subsystem's infrastructure lives in `Infrastructure/Integrations/Migration/`;
  do not "correct" it.
- **Validator messages are English literals** (`WithMessage(...)`), so the response carries one
  localised sentence for a rejected form and the field errors as written. Per-field localisation
  needs a resource key per message and a base validator that resolves it.

## Where things go

| Kind | Path |
|---|---|
| HTTP endpoint | `backend/src/Innovayse.Hostpanel.API/Controllers/<Name>Controller.cs` (or the feature folder that already holds its siblings) — derives `BaseApiController` |
| Command / query + handler + validator | `backend/src/Innovayse.Hostpanel.Application/<Feature>/{Commands,Queries}/<UseCase>/` |
| DTO shared by several use cases in a feature | `.../Application/<Feature>/Common/` |
| DTO used by exactly one use case | that use case's own folder |
| Port | `.../Application/Common/Interfaces/I<Port>.cs` — nowhere else |
| Error code → status | `backend/src/Shared/Controllers/StatusMapping.cs` |
| Sentence a person reads for a refusal | `.../Application/Resources/ErrorMessages*.resx` (en, ru, hy together) |
| Entity, domain event, enum | `backend/src/Innovayse.Hostpanel.Domain/<Area>/` (`Events/`, `Enums/`) |
| EF configuration, migration, adapter | `backend/src/Innovayse.Hostpanel.Infrastructure/` |
| External service client | `.../Infrastructure/Integrations/<Provider>/` |
| Use-case test | `backend/tests/Innovayse.Hostpanel.Integration.Tests/<Feature>/<UseCase>Tests.cs` |
| Options class | `.../<Feature or Integration>/Options/<Name>Options.cs` |
| Portal page / section | `client/templates/<template>/pages/`, `.../sections/` |
| Shared component | `client/components/`, `client/components/ui/` |
| API call | `client/composables/apis/` |
| Nitro BFF route | `client/server/api/portal/` |
| Exported TS type | `client/types/` |
| Admin screen | `admin/src/views/<Page>View.vue` (+ `admin/src/components/<area>/`) |
| Admin store / API call | `admin/src/stores/<area>.ts` / `admin/src/composables/apis/use<Area>Api.ts` |
| Admin e2e flow | `admin/e2e/<flow>.spec.ts` (fixtures in `admin/e2e/fixtures/`) |
| Plan / spec | `docs/superpowers/plans/`, `docs/superpowers/specs/` |

## Gotchas

- **`dotnet watch` in `hostpanel-api` dies with "An item with the same key has already been
  added".** `DOTNET_USE_POLLING_FILE_WATCHER=1` makes the container enumerate the bind mount on
  a loop, and enumerating a directory being written to occasionally returns an entry twice over
  the Docker Desktop mount. It is a mount artefact, not your build. Recover with
  `stop` then `up -d`, never `restart`. The API itself serves fine; only hot reload is lost.
  This is also why `Innovayse.Hostpanel.API.csproj` carries `<Watch Remove="logs/**/*" />` — and
  `<Content Remove>`, so a publish does not ship yesterday's logs.
- **Wolverine logs `Invocation of {Message} failed!` at Error with a stack trace for every
  handler exception**, before its own policies get a say, and 5.31.0 has no knob for it. A
  staff identity with no client row is not a fault, and four such traces per dashboard load
  made a healthy platform read as a failing one. `ControlFlowExceptionLogFilter` drops exactly
  those lines; extend the filter rather than removing it.
- **Migrations are applied on startup in every environment except `Testing`.** This used to be
  Development-only, which quietly made Development the only environment whose schema followed
  the code.
- **Role seeding is skipped when this deployment does not own its users.** Under `sso` mode
  Identity is not registered, so `GetRequiredService<RoleManager<…>>` threw before the API
  finished starting and the container never came up.
- **`Innovayse.Providers.Inecobank` targets `net8.0`** while everything else targets `net9.0`;
  the MSBuild staging target pins that TFM in its copy paths. Retargeting the provider means
  editing those paths.
