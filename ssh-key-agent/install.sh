#!/usr/bin/env bash
# Installs hostpanel-ssh-key-agent on ONE managed server. Run as root on that server.
#
#   ./install.sh [--config FILE] [--with-cwp-check] [--dry-run] "<management public key line>"
#
#   --config FILE      agent.conf to install (default: agent.conf.example). An existing
#                      /etc/hostpanel-ssh-key-agent/agent.conf is never overwritten.
#   --with-cwp-check   also install cwp/cwp-account-check (needed by a CWP agent.conf).
#   --dry-run          print what would change and exit without changing anything.
#
# Idempotent: re-running converges to the same state. It creates a locked-down management user
# whose single authorized key can only run the agent, lets that user run exactly the agent (no
# arguments) as root through sudo, and installs the agent. It does not touch sshd, the firewall,
# the VPN, Jailkit or any customer account. Undo with uninstall.sh.
set -euo pipefail

MGMT_USER="${HOSTPANEL_SSH_MGMT_USER:-hpsshkeys}"
HERE="$(cd "$(dirname "$0")" && pwd)"
AGENT_DST="/usr/local/sbin/hostpanel-ssh-key-agent"
LIBEXEC="/usr/local/libexec/hostpanel-ssh-key-agent"
SUDOERS="/etc/sudoers.d/hostpanel-ssh-key-agent"
CONF_DIR="/etc/hostpanel-ssh-key-agent"
CONFIG_SRC="$HERE/agent.conf.example"
WITH_CWP=0
DRY=0
KEY=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --config) CONFIG_SRC="$2"; shift 2 ;;
    --with-cwp-check) WITH_CWP=1; shift ;;
    --dry-run) DRY=1; shift ;;
    ssh-ed25519\ *) KEY="$1"; shift ;;
    *) echo "Unknown argument: $1" >&2; exit 1 ;;
  esac
done

[[ $EUID -eq 0 ]] || { echo "Run as root." >&2; exit 1; }
[[ -n "$KEY" ]] || { echo "Usage: $0 [--config FILE] [--with-cwp-check] [--dry-run] \"ssh-ed25519 AAAA... comment\"" >&2; exit 1; }
[[ -f "$CONFIG_SRC" ]] || { echo "No config file $CONFIG_SRC" >&2; exit 1; }
command -v python3 >/dev/null || { echo "python3 is required." >&2; exit 1; }
printf '%s\n' "$KEY" | ssh-keygen -l -f /dev/stdin >/dev/null 2>&1 || { echo "Management key is not a valid public key." >&2; exit 1; }

run() { if [[ $DRY -eq 1 ]]; then echo "would: $*"; else "$@"; fi; }

# 1. Management user: system account, locked password, /bin/sh only so the forced command can run.
if ! id "$MGMT_USER" >/dev/null 2>&1; then
  run useradd --system --create-home --home-dir "/var/lib/$MGMT_USER" --shell /bin/sh --comment "HostPanel SSH key agent" "$MGMT_USER"
fi
run usermod -L "$MGMT_USER"

# 2. The agent and (optionally) the panel account check: root-owned, not writable by anyone else.
run install -o root -g root -m 0755 "$HERE/hostpanel-ssh-key-agent" "$AGENT_DST"
if [[ $WITH_CWP -eq 1 ]]; then
  run install -d -o root -g root -m 0755 "$LIBEXEC"
  run install -o root -g root -m 0755 "$HERE/cwp/cwp-account-check" "$LIBEXEC/cwp-account-check"
fi
run install -d -o root -g root -m 0755 "$CONF_DIR"
if [[ ! -f "$CONF_DIR/agent.conf" ]]; then
  run install -o root -g root -m 0644 "$CONFIG_SRC" "$CONF_DIR/agent.conf"
fi

# 3. sudo: exactly this command, with NO arguments (the trailing "" forbids any), no tty needed,
#    environment reset. Validated with visudo before it is put in place.
TMP="$(mktemp)"
trap 'rm -f "$TMP"' EXIT
printf 'Defaults:%s !requiretty, env_reset\n%s ALL=(root) NOPASSWD: %s ""\n' "$MGMT_USER" "$MGMT_USER" "$AGENT_DST" > "$TMP"
visudo -cf "$TMP" >/dev/null
run install -o root -g root -m 0440 "$TMP" "$SUDOERS"
[[ $DRY -eq 1 ]] || visudo -c >/dev/null

# 4. The one authorized key, pinned to the agent; `restrict` removes pty, port/agent/X11
#    forwarding and user rc. The previous key (if any) is replaced, never appended to.
if [[ $DRY -eq 1 ]]; then
  echo "would: write /var/lib/$MGMT_USER/.ssh/authorized_keys (restrict,command=\"sudo -n $AGENT_DST\")"
else
  HOME_DIR="$(getent passwd "$MGMT_USER" | cut -d: -f6)"
  install -d -o "$MGMT_USER" -g "$MGMT_USER" -m 0700 "$HOME_DIR/.ssh"
  printf 'restrict,command="sudo -n %s" %s\n' "$AGENT_DST" "$KEY" > "$TMP"
  install -o "$MGMT_USER" -g "$MGMT_USER" -m 0600 "$TMP" "$HOME_DIR/.ssh/authorized_keys"
fi

[[ $DRY -eq 1 ]] && exit 0

echo "Installed. Host key fingerprints to verify and pin in HostPanel:"
for f in /etc/ssh/ssh_host_ed25519_key.pub /etc/ssh/ssh_host_ecdsa_key.pub /etc/ssh/ssh_host_rsa_key.pub; do
  [[ -f $f ]] && ssh-keygen -lf "$f" -E sha256
done
echo "Self-test:"
echo '{"v":1,"op":"ping"}' | sudo -u "$MGMT_USER" sudo -n "$AGENT_DST"
