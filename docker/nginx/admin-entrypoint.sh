#!/bin/sh
# Runs from /docker-entrypoint.d before nginx starts: turns ADMIN_BASE_URL_PATH into the SPA's
# runtime config.js and the <base href> of index.html, so one image serves any base path.
set -eu

: "${ADMIN_BASE_URL_PATH:?ADMIN_BASE_URL_PATH must be set}"

if ! printf '%s' "$ADMIN_BASE_URL_PATH" | grep -Eq '^/[A-Za-z0-9._~/-]*$'; then
  echo "ADMIN_BASE_URL_PATH must start with / and contain only letters, digits and ._~/-" >&2
  exit 1
fi

root=/usr/share/nginx/html
base=$(printf '%s' "$ADMIN_BASE_URL_PATH" | sed 's|/*$||')
if [ -z "$base" ]; then
  base=/
  href=/
else
  href="$base/"
fi

ADMIN_BASE_URL_PATH="$base" envsubst '${ADMIN_BASE_URL_PATH}' < /etc/admin/config.js.template > "$root/config.js"
sed -i "s|<base href=\"[^\"]*\">|<base href=\"$href\">|" "$root/index.html"
