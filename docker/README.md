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

## Rules

- No `${VAR:-x}` defaults and no `env_file:`; required secrets are `${VAR:?...}`.
- Services talk to each other by container name (`hostpanel-api:${API_PORT}`, `hostpanel-db`), never localhost.
- Parked services (rabbitmq, redis, nginx under the platform overlay) carry no `container_name`.

## Demo

`docker-compose.demo.yml` is an overlay that runs a second, self-contained copy of the stack in demo
mode. It goes last in the chain, renames the project to `hostpanel-demo`, and gives every stateful
piece a name of its own (`hostpanel-demo-db`, `-rabbitmq`, `-redis`, `-mailhog`, `-api`, `-client`,
`-admin`; volumes `hostpanel-demo_*`), so it can share a server with production without touching
its data. Services reach each other by those container names. The overlay publishes the api, client,
admin and mailhog UI on the `DEMO_*_HOST_PORT` ports and never starts the bundled nginx.

Set the `# Demo` block of `.env.example` (copy to `docker/.env`, `AUTH_MODE=local`, a database of its
own in `POSTGRES_DB`; `DEMO_ADMIN_PASSWORD`, `DEMO_CLIENT_PASSWORD`, `DEMO_ADMIN_BASE_URL` and `DEMO_CLIENT_BASE_URL` are required), then from the
project root:

```bash
docker compose -p hostpanel-demo -f docker/docker-compose.shared.yml -f docker/docker-compose.prod.yml -f docker/docker-compose.demo.yml up --build
```

On a deployed host add `-f docker/docker-compose.platform.yml` before the demo file (the `deploy:demo`
CI job does). Demo mode (`Demo__Enabled`) swaps the payment, registrar and provisioning integrations for
fakes, delivers all mail to mailhog (UI on `DEMO_MAILHOG_UI_HOST_PORT`), seeds the two demo accounts and
resets the data every `DEMO_RESET_INTERVAL_HOURS` hours.
