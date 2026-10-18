# HostPanel

Open-source hosting-business platform: client portal, admin panel and API. ASP.NET Core 9, PostgreSQL, Nuxt 4 and Vue 3.
Payment gateways, registrars and provisioning panels are plugins behind small interfaces.

Source, docs and issues: https://github.com/innovayse/hostpanel

## Tags

Everything is published to one repository, `innovayse/hostpanel`. The component is part of the tag. Images are multi-arch (`amd64`, `arm64`) and built with `AUTH_MODE=local`.

| Component | Pinned to a release | Moving |
|---|---|---|
| API | `innovayse/hostpanel:api-<version>` | `innovayse/hostpanel:api-latest` |
| Client portal | `innovayse/hostpanel:client-<version>` | `innovayse/hostpanel:client-latest` |
| Admin panel | `innovayse/hostpanel:admin-<version>` | `innovayse/hostpanel:admin-latest` |

`<version>` has no leading `v` (`1.2.3`).

## Install

```bash
curl -fsSL https://raw.githubusercontent.com/innovayse/hostpanel/main/install.sh | bash
```

The installer sets up the compose stack, secrets and HTTPS for you.

## Links

- [Quick start](https://github.com/innovayse/hostpanel#quick-start-with-docker)
- [Using the Docker Hub images](https://github.com/innovayse/hostpanel#using-the-docker-hub-images)
- [Configuration](https://github.com/innovayse/hostpanel#configuration)
- [Contributing](https://github.com/innovayse/hostpanel/blob/main/CONTRIBUTING.md)
- [Report an issue](https://github.com/innovayse/hostpanel/issues/new/choose)
