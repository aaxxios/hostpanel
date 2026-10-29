<?php

declare(strict_types=1);

namespace HostpanelMigration\Auth;

/**
 * Checks the key a caller presents against the key configured in the module settings, in
 * constant time, behind the failed attempt throttle.
 */
final class ApiKeyAuthenticator
{
    /**
     * Creates the authenticator.
     *
     * @param string                $configuredKey The key configured in the module settings; empty when none is.
     * @param FailedAttemptThrottle $throttle      Locks out an address that sent too many wrong keys.
     */
    public function __construct(
        /** The key configured in the module settings; empty when none is. */
        private readonly string $configuredKey,
        /** Locks out an address that sent too many wrong keys. */
        private readonly FailedAttemptThrottle $throttle,
    ) {
    }

    /**
     * Judges a presented key. A locked out address is refused before the key is compared; an
     * empty key never matches, not even an unconfigured one.
     *
     * @param string $presentedKey The key the caller sent, trimmed.
     * @param string $address      The caller's address.
     * @param int    $now          The current Unix time.
     *
     * @return AuthResult The judgement.
     *
     * @throws AttemptLogUnavailable When the throttle's log cannot be read or written.
     */
    public function authenticate(string $presentedKey, string $address, int $now): AuthResult
    {
        if ($this->throttle->isLocked($address, $now)) {
            return AuthResult::Throttled;
        }

        if ($presentedKey === '' || $this->configuredKey === '' || !hash_equals($this->configuredKey, $presentedKey)) {
            $this->throttle->recordFailure($address, $now);

            return AuthResult::Rejected;
        }

        $this->throttle->clear($address);

        return AuthResult::Accepted;
    }
}
