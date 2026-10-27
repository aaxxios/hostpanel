"""Unit tests for hostpanel-ssh-key-agent. Run: python3 -m unittest discover -s ssh-key-agent/tests"""

import base64
import importlib.machinery
import importlib.util
import json
import os
import pwd
import subprocess
import sys
import time
import tempfile
import threading
import unittest

AGENT_PATH = os.path.join(os.path.dirname(__file__), "..", "hostpanel-ssh-key-agent")
_loader = importlib.machinery.SourceFileLoader("agent", AGENT_PATH)
_spec = importlib.util.spec_from_loader("agent", _loader)
agent = importlib.util.module_from_spec(_spec)
_loader.exec_module(agent)


def make_key(seed):
    """A syntactically valid ED25519 public key line for a 32-byte seed value."""
    blob = b"\x00\x00\x00\x0bssh-ed25519" + b"\x00\x00\x00\x20" + bytes([seed]) * 32
    return "ssh-ed25519 " + base64.b64encode(blob).decode() + " someone@laptop"


KEY_ID_A = "11111111-1111-4111-8111-111111111111"
KEY_ID_B = "22222222-2222-4222-8222-222222222222"


class AgentTestCase(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.home_root = os.path.join(self.tmp.name, "home")
        os.makedirs(os.path.join(self.home_root, "alice"))
        self.config = {
            "min_uid": 0,
            "home_root": os.path.realpath(self.home_root),
            "deny_accounts": {"root"},
            "max_keys": 3,
            "audit_log": os.path.join(self.tmp.name, "audit.log"),
            "lock_dir": os.path.join(self.tmp.name, "locks"),
        }
        me = pwd.getpwuid(os.geteuid())
        self.shell = "/bin/bash"
        home = os.path.join(self.home_root, "alice")

        def fake_getpwnam(name):
            if name != "alice":
                raise KeyError(name)
            return pwd.struct_passwd(("alice", "x", me.pw_uid, max(me.pw_gid, 1), "", home, self.shell))

        self._orig = agent.resolve_account
        agent.resolve_account = lambda name, config, getpwnam=fake_getpwnam: self._orig(name, config, fake_getpwnam)

    def tearDown(self):
        agent.resolve_account = self._orig
        self.tmp.cleanup()

    def call(self, **request):
        request.setdefault("v", 1)
        return agent.handle(json.dumps(request), self.config)

    def authorized_keys(self):
        with open(os.path.join(self.home_root, "alice", ".ssh", "authorized_keys")) as handle:
            return handle.read()


class KeyParsingTests(unittest.TestCase):
    def test_valid_key_has_openssh_fingerprint(self):
        _, fingerprint = agent.parse_ed25519(make_key(7))
        self.assertTrue(fingerprint.startswith("SHA256:"))
        self.assertEqual(len(fingerprint), len("SHA256:") + 43)

    def test_rejects_other_algorithms_and_garbage(self):
        for bad in ["ssh-rsa AAAAB3NzaC1yc2E= x", "ssh-ed25519 !!!", "", "ssh-ed25519 " + "A" * 68,
                    make_key(1) + "\nssh-ed25519 injected", "-----BEGIN OPENSSH PRIVATE KEY-----"]:
            with self.assertRaises(agent.AgentError, msg=bad):
                agent.parse_ed25519(bad)


class OperationTests(AgentTestCase):
    def test_add_list_revoke_roundtrip(self):
        added = self.call(op="add", account="alice", key_id=KEY_ID_A, public_key=make_key(1))
        self.assertTrue(added["ok"], added)
        self.assertNotIn("someone@laptop", self.authorized_keys())
        self.assertIn("hostpanel:" + KEY_ID_A, self.authorized_keys())
        listed = self.call(op="list", account="alice")
        self.assertEqual([k["key_id"] for k in listed["keys"]], [KEY_ID_A])
        self.assertEqual(listed["keys"][0]["fingerprint"], added["fingerprint"])
        revoked = self.call(op="revoke", account="alice", key_id=KEY_ID_A)
        self.assertTrue(revoked["changed"])
        self.assertNotIn(KEY_ID_A, self.authorized_keys())

    def test_permissions_are_private(self):
        self.call(op="add", account="alice", key_id=KEY_ID_A, public_key=make_key(1))
        ssh_dir = os.path.join(self.home_root, "alice", ".ssh")
        self.assertEqual(os.stat(ssh_dir).st_mode & 0o777, 0o700)
        self.assertEqual(os.stat(os.path.join(ssh_dir, "authorized_keys")).st_mode & 0o777, 0o600)

    def test_add_is_idempotent_for_same_key_id(self):
        self.call(op="add", account="alice", key_id=KEY_ID_A, public_key=make_key(1))
        again = self.call(op="add", account="alice", key_id=KEY_ID_A, public_key=make_key(1))
        self.assertTrue(again["ok"])
        self.assertFalse(again["changed"])
        self.assertEqual(self.authorized_keys().count("ssh-ed25519"), 1)

    def test_duplicate_key_under_new_id_refused(self):
        self.call(op="add", account="alice", key_id=KEY_ID_A, public_key=make_key(1))
        dup = self.call(op="add", account="alice", key_id=KEY_ID_B, public_key=make_key(1))
        self.assertEqual(dup["error"], "duplicate_key")

    def test_unmanaged_keys_are_preserved_and_never_listed(self):
        ssh_dir = os.path.join(self.home_root, "alice", ".ssh")
        os.mkdir(ssh_dir, 0o700)
        with open(os.path.join(ssh_dir, "authorized_keys"), "w") as handle:
            handle.write(make_key(9) + "\n")
        self.call(op="add", account="alice", key_id=KEY_ID_A, public_key=make_key(1))
        self.call(op="revoke", account="alice", key_id=KEY_ID_A)
        listed = self.call(op="list", account="alice")
        self.assertEqual(listed["keys"], [])
        self.assertEqual(listed["unmanaged_keys"], 1)
        self.assertIn("someone@laptop", self.authorized_keys())

    def test_max_keys_enforced(self):
        for i, seed in enumerate([1, 2, 3]):
            self.call(op="add", account="alice", key_id="%08d-1111-4111-8111-111111111111" % i, public_key=make_key(seed))
        over = self.call(op="add", account="alice", key_id=KEY_ID_B, public_key=make_key(4))
        self.assertEqual(over["error"], "too_many_keys")

    def test_revoke_unknown_is_noop(self):
        result = self.call(op="revoke", account="alice", key_id=KEY_ID_B)
        self.assertTrue(result["ok"])
        self.assertFalse(result["changed"])

    def test_shell_disabled_refuses_add_but_reports_status(self):
        self.shell = "/sbin/nologin"
        self.assertEqual(self.call(op="add", account="alice", key_id=KEY_ID_A, public_key=make_key(1))["error"], "ssh_disabled")
        self.assertFalse(self.call(op="status", account="alice")["shell_enabled"])

    def test_audit_log_records_fingerprint_not_key(self):
        self.call(op="add", account="alice", key_id=KEY_ID_A, public_key=make_key(1))
        with open(self.config["audit_log"]) as handle:
            line = json.loads(handle.read().splitlines()[-1])
        self.assertEqual(line["outcome"], "ok")
        self.assertTrue(line["fingerprint"].startswith("SHA256:"))
        self.assertNotIn("public_key", line)

    def test_concurrent_adds_lose_nothing(self):
        ids = ["%08d-2222-4222-8222-222222222222" % i for i in range(3)]
        threads = [threading.Thread(target=self.call, kwargs=dict(op="add", account="alice", key_id=k, public_key=make_key(10 + i)))
                   for i, k in enumerate(ids)]
        [t.start() for t in threads]
        [t.join() for t in threads]
        self.assertEqual(sorted(k["key_id"] for k in self.call(op="list", account="alice")["keys"]), sorted(ids))


class BoundaryTests(AgentTestCase):
    def test_symlinked_ssh_dir_refused(self):
        target = os.path.join(self.tmp.name, "elsewhere")
        os.mkdir(target)
        os.symlink(target, os.path.join(self.home_root, "alice", ".ssh"))
        result = self.call(op="add", account="alice", key_id=KEY_ID_A, public_key=make_key(1))
        self.assertFalse(result["ok"])
        self.assertEqual(os.listdir(target), [])

    def test_symlinked_authorized_keys_refused(self):
        ssh_dir = os.path.join(self.home_root, "alice", ".ssh")
        os.mkdir(ssh_dir, 0o700)
        victim = os.path.join(self.tmp.name, "victim")
        open(victim, "w").close()
        os.symlink(victim, os.path.join(ssh_dir, "authorized_keys"))
        result = self.call(op="add", account="alice", key_id=KEY_ID_A, public_key=make_key(1))
        self.assertEqual(result["error"], "unsafe_path")
        self.assertEqual(os.path.getsize(victim), 0)

    def test_bad_account_names_refused(self):
        for name in ["../etc", "root", "Alice", "a b", "alice;id", "", "x" * 40, "bob"]:
            result = self.call(op="list", account=name)
            self.assertFalse(result["ok"], name)

    def test_unknown_fields_and_ops_refused(self):
        self.assertEqual(self.call(op="list", account="alice", path="/etc")["error"], "invalid_request")
        self.assertEqual(self.call(op="exec", account="alice")["error"], "unknown_operation")
        self.assertEqual(agent.handle(json.dumps({"v": 2, "op": "ping"}), self.config)["error"], "unsupported_version")
        self.assertEqual(agent.handle("not json", self.config)["error"], "invalid_request")

    def test_bad_key_id_refused(self):
        result = self.call(op="revoke", account="alice", key_id="../../x")
        self.assertEqual(result["error"], "invalid_request")

    def test_home_outside_root_refused(self):
        me = pwd.getpwuid(os.geteuid())
        entry = pwd.struct_passwd(("alice", "x", me.pw_uid, 1, "", "/etc", "/bin/bash"))
        with self.assertRaises(agent.AgentError):
            self._orig("alice", self.config, lambda _n: entry)

    def test_ping(self):
        self.assertEqual(self.call(op="ping")["agent_version"], agent.AGENT_VERSION)


class TimeoutTests(unittest.TestCase):
    def test_silent_client_does_not_pin_the_process(self):
        code = ("import importlib.machinery as m, importlib.util as u, sys;"
                "l=m.SourceFileLoader('a', sys.argv[1]); a=u.module_from_spec(u.spec_from_loader('a', l)); l.exec_module(a);"
                "a.STDIN_TIMEOUT_SECONDS=1; sys.exit(a.main())")
        proc = subprocess.Popen([sys.executable, "-c", code, AGENT_PATH], stdin=subprocess.PIPE, stdout=subprocess.PIPE)
        started = time.monotonic()
        out = proc.stdout.read()
        proc.wait(timeout=10)
        self.assertLess(time.monotonic() - started, 5)
        self.assertEqual(json.loads(out)["error"], "timeout")
        proc.stdin.close()


if __name__ == "__main__":
    unittest.main()
