<?php

declare(strict_types=1);

namespace HostpanelMigration\Tests\Auth;

use HostpanelMigration\Auth\ApiKeyAuthenticator;
use HostpanelMigration\Auth\AuthResult;
use HostpanelMigration\Auth\FailedAttemptThrottle;
use HostpanelMigration\Tests\Fixtures\InMemoryAttemptLog;
use PHPUnit\Framework\TestCase;

/**
 * The key check and the failed attempt throttle in front of it.
 */
final class ApiKeyAuthenticatorTest extends TestCase
{
    /** The key configured in the module settings. */
    private const KEY = '0123456789abcdef0123456789abcdef';

    /** The caller's address. */
    private const ADDRESS = '203.0.113.7';

    /** A fixed moment the attempts happen at. */
    private const NOW = 1_700_000_000;

    /**
     * The configured key is accepted.
     */
    public function testRightKeyIsAccepted(): void
    {
        self::assertSame(AuthResult::Accepted, self::authenticator(self::KEY)->authenticate(self::KEY, self::ADDRESS, self::NOW));
    }

    /**
     * A wrong key, an empty key and any key against an unconfigured module are rejected.
     */
    public function testWrongEmptyOrUnconfiguredKeyIsRejected(): void
    {
        self::assertSame(AuthResult::Rejected, self::authenticator(self::KEY)->authenticate('wrong', self::ADDRESS, self::NOW));
        self::assertSame(AuthResult::Rejected, self::authenticator(self::KEY)->authenticate('', self::ADDRESS, self::NOW));
        self::assertSame(AuthResult::Rejected, self::authenticator('')->authenticate('', self::ADDRESS, self::NOW));
    }

    /**
     * After the allowed wrong keys the address is refused even with the right key, other
     * addresses are not, and the lock lifts once the window closes.
     */
    public function testTooManyWrongKeysLockTheAddressUntilTheWindowCloses(): void
    {
        $authenticator = self::authenticator(self::KEY);
        for ($i = 0; $i < FailedAttemptThrottle::MAX_FAILURES; $i++) {
            $authenticator->authenticate('wrong', self::ADDRESS, self::NOW);
        }

        self::assertSame(AuthResult::Throttled, $authenticator->authenticate(self::KEY, self::ADDRESS, self::NOW + 1));
        self::assertSame(AuthResult::Accepted, $authenticator->authenticate(self::KEY, '198.51.100.1', self::NOW + 1));
        self::assertSame(
            AuthResult::Accepted,
            $authenticator->authenticate(self::KEY, self::ADDRESS, self::NOW + FailedAttemptThrottle::WINDOW_SECONDS),
        );
    }

    /**
     * The right key clears the address's earlier failures.
     */
    public function testRightKeyClearsEarlierFailures(): void
    {
        $authenticator = self::authenticator(self::KEY);
        for ($i = 0; $i < FailedAttemptThrottle::MAX_FAILURES - 1; $i++) {
            $authenticator->authenticate('wrong', self::ADDRESS, self::NOW);
        }

        $authenticator->authenticate(self::KEY, self::ADDRESS, self::NOW);
        $authenticator->authenticate('wrong', self::ADDRESS, self::NOW);

        self::assertSame(AuthResult::Accepted, $authenticator->authenticate(self::KEY, self::ADDRESS, self::NOW));
    }

    /**
     * An authenticator over a fresh, in-memory attempt log.
     *
     * @param string $configuredKey The key configured in the module settings.
     *
     * @return ApiKeyAuthenticator The authenticator.
     */
    private static function authenticator(string $configuredKey): ApiKeyAuthenticator
    {
        return new ApiKeyAuthenticator($configuredKey, new FailedAttemptThrottle(new InMemoryAttemptLog()));
    }
}
