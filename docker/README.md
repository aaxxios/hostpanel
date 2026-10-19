# docker: how hostpanel runs, on a laptop and on a server

The compose stacks, the infrastructure config and the operator scripts live here. Each app's
Dockerfile sits beside its code: `backend/Dockerfile` (context `backend/`), `client/Dockerfile`
(context `client/`), `admin/Dockerfile` (context = project root, so its prod stage can copy
`docker/nginx/admin.conf`).

```
docker/
├── docker-compose.shared.yml     hostpanel-db, hostpanel-rabbitmq, hostpanel-redis (one definition for dev and prod)
├── docker-compose.dev.yml        hostpanel-api (dotnet watch), hostpanel-client (nuxt), hostpanel-admin (vite),
│                                 hostpanel-mailhog; host ports and resource caps for the infrastructure
├── docker-compose.prod.yml       the applications by ${IMAGE_TAG} (registry images; a build: block lets a host
│                                 build the same image in place) and the bundled hostpanel-nginx
├── docker-compose.platform.yml   joins the external innovayse_network; parks rabbitmq, redis and nginx
├── docker-compose.demo.yml       demo overlay: project hostpanel-demo, own db/broker/redis/mailhog, demo-mode API env
├── .env.example                  every variable of the stack, each named once; copy to .env (gitignored)
├── nginx/
│   ├── Dockerfile · templates/  the bundled edge proxy (prod, standalone install only)
│   └── admin.conf                the admin SPA's nginx (admin/Dockerfile, prod stage)
└── scripts/manage.sh             local start/stop/logs/db helper over the shared + dev stack
```

App-level templates: `client/.env.example` and `admin/.env.example` (for `yarn dev` / `npm run dev`
run directly in the app).

## Which chain when

| Where | Compose files (all `-f docker/...` from the project root, or plain from `docker/`) |
|---|---|
| laptop, product alone | `shared` + `dev` |
| laptop, inside the workspace | `shared` + `dev` + `platform` (or the workspace's `./dev up hostpanel`) |
| deployed host | `shared` + `prod` + `platform` (stage and production) |
| demo, beside prod on a deployed host | `shared` + `prod` + `platform` + `demo`, project `-p hostpanel-demo` |
| demo, laptop | `shared` + `prod` + `demo` |
| standalone install | `hub` alone, driven by `install.sh` (a single curl-to-bash command) |

Compose reads `docker/.env` because the files sit in `docker/`. Every compose file pins
`name: hostpanel`, and volumes carry fixed names (`hostpanel_pgdata`, ...), so moving the files did
not rename any volume or network.

## Run from Docker Hub

`docker-compose.hub.yml` is the self-hoster stack: the same services as `shared` + `prod`, but the
applications are pulled from `${DOCKERHUB_NAMESPACE}/hostpanel:api-|client-|admin-${IMAGE_TAG}`
instead of the GitLab registry or a local build. The edge is the official `caddy` image (pinned), which obtains Let's Encrypt certificates for `DOMAIN` and `ADMIN_DOMAIN` by itself and reads `./Caddyfile` (`docker/caddy/Caddyfile`); this file, the Caddyfile and `.env` are all that is needed. `EDGE_SCHEME=http://` serves plain HTTP behind an existing proxy.


| Image | Pinned to a release | Moving |
| --- | --- | --- |
| API | `innovayse/hostpanel:api-<version>` | `innovayse/hostpanel:api-latest` |
| Client | `innovayse/hostpanel:client-<version>` | `innovayse/hostpanel:client-latest` |
| Admin | `innovayse/hostpanel:admin-<version>` | `innovayse/hostpanel:admin-latest` |

`<version>` has no leading `v` (`1.2.3`). `IMAGE_TAG` in `.env` is that version or `latest`; the compose file adds the component prefix.

```bash
cp caddy/Caddyfile Caddyfile   # next to docker-compose.hub.yml
cp .env.example .env     # DOCKERHUB_NAMESPACE, IMAGE_TAG (a released version) and the secrets
docker compose -f docker-compose.hub.yml up -d
```

Images are published by `.github/workflows/publish-dockerhub.yml`, the primary publisher. It runs on a `v*` tag or
`workflow_dispatch` (multi-arch, `AUTH_MODE=local`) and uses the secrets `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN`
and the variable `DOCKERHUB_NAMESPACE`. The GitLab `publish:dockerhub` job is a manual fallback; it needs the variables
`DOCKERHUB_NAMESPACE`, `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN` (masked, protected).

The `deploy:prod` job takes its GitLab environment URL from the CI/CD variable `PROD_PUBLIC_URL` (the public
address of the production site, for example `https://panel.example.com`), scoped to `production/hostpanel`; the
address is never written in `.gitlab-ci.yml`.

## Where a value lives

Nothing a deployment could want to change is typed into a compose file, a proxy config, a script or the
pipeline. A value has one name and one home:

- **Compose and the edge proxies** read `docker/.env` (copied from `.env.example`; on a deployed tier the
  deploy job's environment, from GitLab CI/CD variables, wins). The nginx template and the Caddyfile get
  `DOMAIN`, `ADMIN_DOMAIN`, `API_PORT`, `CLIENT_PORT`, `EDGE_MAX_BODY_SIZE` and `EDGE_ADMIN_MAX_BODY_SIZE`
  through their container's environment, so the port or the upload limit is changed in `.env` and nowhere else.
- **The public addresses** are `CLIENT_BASE_URL` and `ADMIN_BASE_URL`: the client's `NUXT_PUBLIC_BASE_URL`,
  `NUXT_PUBLIC_SITE_URL`, the SSO callback and the API's first CORS origin all derive from `CLIENT_BASE_URL`
  (the demo derives them from `DEMO_CLIENT_BASE_URL`). `DOMAIN` is only the edge's `server_name`.
- **Versions** of the third-party images are `POSTGRES_IMAGE`, `RABBITMQ_IMAGE`, `REDIS_IMAGE`,
  `MAILHOG_IMAGE`, `NGINX_IMAGE` and `CADDY_IMAGE`, full references such as `postgres:17-alpine`.
- **Application images** (`prod`) are `${IMAGE_REGISTRY}/{api,client,admin}:${IMAGE_TAG}`; the deploy jobs
  set `IMAGE_REGISTRY` to the project's own registry path. **For manual compose use on a host or laptop, `IMAGE_REGISTRY` must be in the host `docker/.env`** (compose refuses to start without it; `.env.example` carries a placeholder). The public Docker Hub images stay
  `innovayse/hostpanel:{api,client,admin}-<version>` (`DOCKERHUB_NAMESPACE`).
- **Runtime tuning**: `AUTH_APP_NAME`, `API_GC_SERVER`, `API_GC_CONSERVE_MEMORY`,
  `API_GC_HEAP_HARD_LIMIT_PERCENT` (prod and hub); in the dev stack `DEV_FILE_POLLING`, `DEV_NUGET_AUDIT`,
  `DEV_NUGET_SIGNATURE_VERIFICATION`, `TESTCONTAINERS_HOST_OVERRIDE`, `ADMIN_API_BASE`, and the
  `<SERVICE>_CPUS` / `<SERVICE>_MEMORY` caps for `DB`, `RABBITMQ`, `REDIS`, `MAILHOG`, `API`, `CLIENT`, `ADMIN`.
- **Support departments**: `DEPARTMENT_SUPPORT_EMAIL`, `DEPARTMENT_BILLING_EMAIL`, `DEPARTMENT_SALES_EMAIL` and
  `DEPARTMENT_GENERAL_EMAIL` reach the API as `DefaultDepartments__SupportEmail`, `__BillingEmail`, `__SalesEmail`
  and `__GeneralEmail` (dev, prod and hub stacks). They name the mailboxes of the departments created on a
  fresh install and matter only on the first start; after that the departments are edited in the admin
  panel. `.env.example` carries `example.com` placeholders: a real deployment sets its own.
- **Demo**: `DEMO_PUBLISH_ADDRESS` (the address its published ports bind to; required, so it can never be empty and bind every interface) and `DEMO_SMTP_ENCRYPTION`,
  next to the existing `DEMO_*` keys. `DEMO_CONTACT_EMAIL` and `DEMO_CONTACT_PHONE` are optional (`Demo__ContactEmail` /
  `Demo__ContactPhone`): the contact the demo storefront shows in its footer and contact page, written on every seed and
  reset. Empty keeps the fictional `demo@hostpanel-demo.example` / `+1 555 0100`; a set email must be an address or
  startup is refused. They are display only: the demo's mail still goes to its own mailhog.
- **Optional storefront content** (prod, dev and hub client): `NUXT_PUBLIC_PORTAL_CONTACT_ADDRESS`,
  `NUXT_PUBLIC_PORTAL_CONTACT_MAP_URL`, `NUXT_PUBLIC_PORTAL_STATS_CLIENTS`, `NUXT_PUBLIC_PORTAL_STATS_UPTIME`,
  `NUXT_PUBLIC_PORTAL_STATS_REGIONS` and `NUXT_PUBLIC_PORTAL_HERO_BADGE`. Empty is allowed: the matching
  `portal.*` setting in the admin panel wins, and an empty value hides the element.
- **Build-time chat address**: `NUXT_PUBLIC_PORTAL_CHAT_BASE_URL` is also read when the client image is built
  (`nuxt.config.ts` bakes the `/brand-assets/**` proxy target into the output), so `client/Dockerfile` declares it
  as a build `ARG`/`ENV`. The GitLab `.build-image` job passes the CI/CD variable of the same name, and the Docker Hub
  workflow passes the repository variable of the same name; unset means empty, which builds without the proxy.
  A locally built image needs `--build-arg NUXT_PUBLIC_PORTAL_CHAT_BASE_URL=...` for the proxy to exist.
- **The pipeline** (`.gitlab-ci.yml`) keeps its toolchain versions in one top-level `variables:` block
  (`DOTNET_SDK_IMAGE`, `NODE_IMAGE`, `DOCKER_IMAGE`, `BINFMT_IMAGE`, `PLAYWRIGHT_*_IMAGE`, `YARN_VERSION`),
  and takes the GitLab host (`PIPELINE_SERVER_HOST`), the public source address (`PUBLIC_REPOSITORY_URL`)
  and the compose chain (`COMPOSE_FILES_DEPLOYED`) from there too. The deploy jobs need these CI/CD
  variables, scoped per environment, and refuse to run without `DEPLOY_DIR`:

  | Variable | Scope | Meaning |
  | --- | --- | --- |
  | `DEPLOY_DIR` | `staging/hostpanel`, `production/hostpanel`, `demo/hostpanel` | the job's own checkout on its host (the demo's is never production's) |
  | `PROD_PUBLIC_URL` | `production/hostpanel` | the production site's address |
  | `BACKUP_KEEP` | optional, `production/hostpanel` | how many pre-deploy dumps `deploy:prod` keeps (10 in the pipeline) |

- **GitHub workflows** name their toolchain versions once in the workflow-level `env:` (`ci.yml`), read the
  source address of the image labels from the `github` context, and push the mirror to the repository
  variable `GITLAB_MIRROR_URL` (host and path of the GitLab project, no scheme) with the secret
  `GITLAB_MIRROR_TOKEN`.
- **`install.sh`** writes only what differs from the template (the generated secrets, the answers, the
  `--no-tls` ports, the database and broker user); the rest of a new `.env` is `.env.example` as downloaded.

A key added to `.env.example` needs its GitLab variable before the next deploy, or compose refuses to start.

## Rules

- No `${VAR:-x}` defaults and no `env_file:`; every required value (secrets, ports, sizes, resource caps, addresses) is `${VAR:?VAR must be set}`.
- Services talk to each other by container name (`hostpanel-api:${API_PORT}`, `hostpanel-db`), never localhost.
- Parked services (rabbitmq, redis, nginx under the platform overlay) carry no `container_name`.

## Demo

`docker-compose.demo.yml` is an overlay that runs a second, self-contained copy of the stack in demo
mode. It goes last in the chain, renames the project to `hostpanel-demo`, and gives every stateful
piece a name of its own (`hostpanel-demo-db`, `-rabbitmq`, `-redis`, `-mailhog`, `-api`, `-client`,
`-admin`; volumes `hostpanel-demo_*`), so it can share a server with production without touching
its data. Services reach each other by those container names, over a demo-private network
(`hostpanel-demo_default`): the db, broker, redis and mailhog are on it alone. Only the api, client and admin
also join `hostpanel_demo_edge`, a dedicated external network that is **not** `innovayse_network`, as compose
services named `hostpanel-demo-api`, `-client` and `-admin` (the production ones are parked in this overlay).
Keep those names unique: compose adds the service name as an alias on every network it joins, so never rename
them back to `hostpanel-api`.

**The demo cannot see production.** No demo container is on `innovayse_network`, so `hostpanel-db`,
`hostpanel-api`, `main-client`, `sso-api` and every other production container neither resolve by name nor
answer by IP from inside the demo. The only production-side container on `hostpanel_demo_edge` is the edge
proxy `innovayse-edge` (it is on both networks and routes by Host header), which is how the public demo URLs
reach the demo. Create the network once per host, before the first demo `up` and before the proxy is
(re)started with the new `edge-proxy/docker-compose.yml`:

```bash
docker network create --subnet 172.29.0.0/16 hostpanel_demo_edge   # `deploy:demo` does this when it is missing
docker network connect hostpanel_demo_edge innovayse-edge   # attach an already-running proxy, no restart
```

The subnet is **fixed at 172.29.0.0/16** (checked free on the server). The edge proxy routes by `Host` header,
so without a guard a demo container could send `Host: host.innovayse.com` to it and reach production.
`edge-proxy/nginx.conf` therefore carries `deny 172.29.0.0/16;` in the http context: every host answers 403
to that subnet, and only the two demo server blocks opt back in with `allow all;`. Real traffic arrives over
the WireGuard tunnel from a different range. Change the subnet in three places together: the `deploy:demo`
network creation, that `deny` line, and this paragraph. An existing network cannot change subnet in place:
disconnect the edge, remove the network, recreate it, reconnect the edge, then redeploy the demo.

Outbound traffic: the demo needs nothing external (fake payment, registrar and provisioning adapters, mail to
mailhog), and the db, broker, redis and mailhog sit on a bridge with no production peer. The api, client and
admin keep the ordinary bridge egress of `hostpanel_demo_edge`; to cut that too, create the network with
`--internal` and drop the `DEMO_*_HOST_PORT` publishes (Docker cannot publish a port from an internal-only
network). `hostpanel-demo_default` is not `internal` for the same reason: mailhog publishes its UI port.
The overlay publishes the api, client,
admin and mailhog UI on the `DEMO_*_HOST_PORT` ports, bound to `DEMO_PUBLISH_ADDRESS` (required; `127.0.0.1` keeps them off the network, the edge proxy reaches them by name), and never starts the bundled nginx.

Set the `# Demo` block of `.env.example` (copy to `docker/.env`, `AUTH_MODE=local`, a database of its
own in `POSTGRES_DB`; `DEMO_ADMIN_PASSWORD`, `DEMO_CLIENT_PASSWORD`, `DEMO_ADMIN_BASE_URL` and `DEMO_CLIENT_BASE_URL` are required), then from the
project root:

```bash
docker compose -p hostpanel-demo -f docker/docker-compose.shared.yml -f docker/docker-compose.prod.yml -f docker/docker-compose.demo.yml up --build
```

On a deployed host add `-f docker/docker-compose.platform.yml` before the demo file (the `deploy:demo`
CI job does). Demo mode (`Demo__Enabled`) swaps the payment, registrar and provisioning integrations for
fakes, delivers all mail to mailhog (UI on `DEMO_MAILHOG_UI_HOST_PORT`), seeds the two demo accounts and
resets the data every `DEMO_RESET_INTERVAL_HOURS` hours. The seed is fixed and deterministic (the same sample data on
every reset), and the site name is `Hostpanel Demo`.

**The demo needs its own `DEPLOY_DIR`.** `deploy:demo` runs `git checkout -f`; in production's directory that
would rewrite the compose files under the running production stack. Its `DEPLOY_DIR` (the CI/CD variable scoped
to `demo/hostpanel`) is a directory of its own, and the job creates it (`git init`, then a fetch of the
pipeline's commit) when missing.

The demo's GitLab scope is `demo/hostpanel`.
