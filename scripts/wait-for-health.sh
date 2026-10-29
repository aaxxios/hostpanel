#!/usr/bin/env sh
#
# Waits for a deployed stack to answer its health endpoint, and fails when it does not in time.
#
# A deploy job that only starts containers reports success even when the API crash-loops on a bad
# migration or configuration. Run this as the last step of a deploy so the job goes red instead.
#
# Usage:
#   scripts/wait-for-health.sh <health url> [timeout seconds]
#
# The timeout defaults to 180 seconds. Exit status: 0 healthy, 1 no healthy answer in time, 2 usage.

set -eu

url="${1:-}"
timeout="${2:-180}"

if [ -z "$url" ]; then
  echo "Usage: $0 <health url> [timeout seconds]" >&2
  exit 2
fi

case "$timeout" in
  ''|*[!0-9]*)
    echo "The timeout must be a whole number of seconds, got '$timeout'." >&2
    exit 2
    ;;
esac

deadline=$(( $(date +%s) + timeout ))

while :; do
  if curl --fail --silent --show-error --output /dev/null --max-time 5 "$url"; then
    echo "Healthy: $url"
    exit 0
  fi
  if [ "$(date +%s)" -ge "$deadline" ]; then
    echo "ERROR: $url did not answer healthy within ${timeout} seconds." >&2
    exit 1
  fi
  sleep 5
done
