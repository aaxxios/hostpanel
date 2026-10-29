# Context: client/ (compose: context ../client). Targets: dev (yarn dev), build, prod (node on .output).
FROM node:22.23.4-alpine3.24@sha256:fe4f6d74e20370acb04ad889077d14d0f64a9c8ea6a42bc60cddf1e844699914 AS dev

RUN corepack enable && corepack prepare yarn@1.22.22 --activate

WORKDIR /app

COPY package.json yarn.lock ./
RUN yarn install --frozen-lockfile

EXPOSE 3000

CMD ["yarn", "dev", "--host", "0.0.0.0"]

FROM node:22.23.4-alpine3.24@sha256:fe4f6d74e20370acb04ad889077d14d0f64a9c8ea6a42bc60cddf1e844699914 AS build

RUN corepack enable && corepack prepare yarn@1.22.22 --activate

WORKDIR /app

COPY package.json yarn.lock ./
RUN yarn install --frozen-lockfile

# Nitro bakes the /brand-assets/** proxy target into the build (nuxt.config.ts reads it at build time),
# so the chat server address has to arrive here as well as at runtime. Empty means no proxy.
ARG NUXT_PUBLIC_PORTAL_CHAT_BASE_URL=""
ENV NUXT_PUBLIC_PORTAL_CHAT_BASE_URL=$NUXT_PUBLIC_PORTAL_CHAT_BASE_URL

COPY . .
RUN yarn build

FROM node:22.23.4-alpine3.24@sha256:fe4f6d74e20370acb04ad889077d14d0f64a9c8ea6a42bc60cddf1e844699914 AS prod

WORKDIR /app

COPY --from=build /app/.output .output/

# The node image ships an unprivileged `node` user; nothing in .output is written at run time.
USER node

EXPOSE 3000

# NITRO_PORT is set by compose. robots.txt is a static-cost route of this server, so it probes the process without rendering a page.
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD wget -q -O /dev/null "http://127.0.0.1:${NITRO_PORT}/robots.txt" || exit 1

CMD ["node", ".output/server/index.mjs"]
