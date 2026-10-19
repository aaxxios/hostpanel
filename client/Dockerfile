# Context: client/ (compose: context ../client). Targets: dev (yarn dev), build, prod (node on .output).
FROM node:22-alpine AS dev

RUN corepack enable && corepack prepare yarn@1.22.22 --activate

WORKDIR /app

COPY package.json yarn.lock ./
RUN yarn install --frozen-lockfile

EXPOSE 3000

CMD ["yarn", "dev", "--host", "0.0.0.0"]

FROM node:22-alpine AS build

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

FROM node:22-alpine AS prod

WORKDIR /app

COPY --from=build /app/.output .output/

EXPOSE 3000

CMD ["node", ".output/server/index.mjs"]
