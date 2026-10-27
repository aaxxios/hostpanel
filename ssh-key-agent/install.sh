#!/usr/bin/env bash
# Installs hostpanel-ssh-key-agent on ONE managed CWP server. Run as root on that server.
#
#   ./install.sh "<HostPanel management public key line>"
#
# Idempotent. It creates a locked-down management user whose single authorized key can only run
# the agent, lets that user run exactly the agent (no arguments) as root through sudo, and
# installs the agent. It does not touch CWP, firewall rules, sshd_config or any customer account.
set -euo pipefail

MGMT_USER="${HOSTPANEL_SSH_MGMT_USER:-hpsshkeys}"
AGENT_SRC="$(cd "$(dirname "$0")" && pwd)/hostpanel-ssh-key-agent"
AGENT_DST="/usr/local/sbin/hostpanel-ssh-key-agent"
SUDOERS="/etc/sudoers.d/hostpanel-ssh-key-agent"
CONF_DIR="/etc/hostpanel-ssh-key-agent"

if [[ $EUID -ne 0 ]]; then echo "Run as root." >&2; exit 1; fi
if [[ $# -ne 1 || "$1" != ssh-ed25519\ * ]]; then
  echo "Usage: $0 \"ssh-ed25519 AAAA... hostpanel\"" >&2; exit 1
fi
command -v python3 >/dev/null || { echo "python3 is required." >&2; exit 1; }

# 1. Management user: no password, a real shell is required for the forced command to run.
if ! id "$MGMT_USER" >/dev/null 2>&1; then
  useradd --system --create-home --home-dir "/var/lib/$MGMT_USER" --shell /bin/sh "$MGMT_USER"
fi
passwd -l "$MGMT_USER" >/dev/null

# 2. The agent itself: root-owned, not writable by anyone else.
install -o root -g root -m 0755 "$AGENT_SRC" "$AGENT_DST"
install -d -o root -g root -m 0755 "$CONF_DIR"
if [[ ! -f "$CONF_DIR/agent.conf" ]]; then
  install -o root -g root -m 0644 "$(dirname "$AGENT_SRC")/agent.conf.example" "$CONF_DIR/agent.conf"
fi

# 3. sudo: exactly this command, with NO arguments (the trailing "" forbids any).
TMP="$(mktemp)"
printf 'Defaults:%s !requiretty, env_reset\n%s ALL=(root) NOPASSWD: %s ""\n' "$MGMT_USER" "$MGMT_USER" "$AGENT_DST" > "$TMP"
visudo -cf "$TMP" >/dev/null
install -o root -g root -m 0440 "$TMP" "$SUDOERS"
rm -f "$TMP"

# 4. The one authorized key, pinned to the agent; `restrict` removes pty, forwarding, agent and X11.
HOME_DIR="$(getent passwd "$MGMT_USER" | cut -d: -f6)"
install -d -o "$MGMT_USER" -g "$MGMT_USER" -m 0700 "$HOME_DIR/.ssh"
printf 'restrict,command="sudo -n %s" %s\n' "$AGENT_DST" "$1" > "$HOME_DIR/.ssh/authorized_keys"
chown "$MGMT_USER:$MGMT_USER" "$HOME_DIR/.ssh/authorized_keys"
chmod 0600 "$HOME_DIR/.ssh/authorized_keys"

echo "Installed. Host key fingerprint to pin in HostPanel:"
for f in /etc/ssh/ssh_host_ed25519_key.pub /etc/ssh/ssh_host_ecdsa_key.pub /etc/ssh/ssh_host_rsa_key.pub; do
  [[ -f $f ]] && ssh-keygen -lf "$f" -E sha256
done
echo "Self-test:"
echo '{"v":1,"op":"ping"}' | sudo -u "$MGMT_USER" sudo -n "$AGENT_DST"
