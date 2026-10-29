#!/usr/bin/env sh
#
# Fails when the Integration test shards of the CI pipelines do not cover the test project exactly.
#
# The Integration suite is split by top-level test folder into shard filters
# (FullyQualifiedName~Integration.Tests.<Folder>.) that live in the pipeline files. A folder named in
# no filter runs nowhere, a folder named twice runs twice, and a name with no folder is a stale entry.
# Every file given on the command line must name each folder exactly once and name nothing else, so
# the GitHub and the GitLab pipelines also stay equal to each other.
#
# Usage, from the repository root:
#   scripts/check-integration-shards.sh .github/workflows/ci.yml .gitlab-ci.yml

set -eu

tests_dir="backend/tests/Innovayse.Hostpanel.Integration.Tests"
status=0

if [ "$#" -eq 0 ]; then
  echo "Usage: $0 <pipeline file> [<pipeline file> ...]" >&2
  exit 2
fi

# Folders that hold no tests and so belong to no shard (Fixtures holds fakes only).
folders=$(
  for dir in "$tests_dir"/*/; do
    name=$(basename "$dir")
    case "$name" in
      bin|obj|Fixtures) ;;
      *) echo "$name" ;;
    esac
  done | sort
)

for file in "$@"; do
  named=$(grep -o 'Integration\.Tests\.[A-Za-z0-9_]*\.' "$file" | sed 's/^Integration\.Tests\.//; s/\.$//' | sort)

  for folder in $folders; do
    count=$(printf '%s\n' "$named" | grep -cx "$folder" || true)
    if [ "$count" -eq 0 ]; then
      echo "ERROR: $file: test folder '$folder' is in no shard, so its tests run nowhere." >&2
      status=1
    elif [ "$count" -gt 1 ]; then
      echo "ERROR: $file: test folder '$folder' is in $count shards, so its tests run more than once." >&2
      status=1
    fi
  done

  for name in $(printf '%s\n' "$named" | sort -u); do
    if ! printf '%s\n' "$folders" | grep -qx "$name"; then
      echo "ERROR: $file: shard entry '$name' has no matching test folder." >&2
      status=1
    fi
  done
done

if [ "$status" -eq 0 ]; then
  echo "Every Integration test folder is in exactly one shard in: $*"
fi
exit "$status"
