# Context: project root (compose: context ..), so prod can take its nginx config from docker/nginx/.
# Targets: dev (vite), build, prod (nginx).
FROM node:22-alpine AS dev

WORKDIR /app

COPY admin/package.json admin/package-lock.json ./
RUN npm install

EXPOSE 5173

CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]

FROM node:22-alpine AS build

WORKDIR /app

# Nothing environment-specific is baked in: the base path comes from ADMIN_BASE_URL_PATH at start.

COPY admin/package.json admin/package-lock.json ./
RUN npm install

COPY admin/ .
RUN npm run build-only

FROM nginx:alpine AS prod

COPY --from=build /app/dist /usr/share/nginx/html

COPY docker/nginx/admin.conf /etc/nginx/conf.d/default.conf

# At start, /docker-entrypoint.d/40-admin-runtime-config.sh writes config.js and <base href>
# from ADMIN_BASE_URL_PATH (required; the script refuses to start without it).
COPY docker/nginx/admin-config.js.template /etc/admin/config.js.template
COPY docker/nginx/admin-entrypoint.sh /docker-entrypoint.d/40-admin-runtime-config.sh
RUN chmod +x /docker-entrypoint.d/40-admin-runtime-config.sh

EXPOSE 80 5173

CMD ["nginx", "-g", "daemon off;"]
