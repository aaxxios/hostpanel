<?php

declare(strict_types=1);

namespace HostpanelMigration\Auth;

/**
 * Locks an address out after too many wrong keys. The window is fixed: it opens with the first
 * failure and lasts {@see self::WINDOW_SECONDS}; an address that reached
 * {@see self::MAX_FAILURES} within it is refused until it closes.
 */
final class FailedAttemptThrottle
{
    /** The wrong keys an address may send within one window. */
    public const MAX_FAILURES = 10;

    /** How long a window lasts, in seconds. */
    public const WINDOW_SECONDS = 900;

    /**
     * Creates the throttle.
     *
     * @param AttemptLog $log Where the failures are kept between requests.
     */
    public function __construct(
        /** Where the failures are kept between requests. */
        private readonly AttemptLog $log,
    ) {
    }

    /**
     * Whether the address is locked out.
     *
     * @param string $address The caller's address.
     * @param int    $now     The current Unix time.
     *
     * @return bool True when the address used up its failures and the window is still open.
     *
     * @throws AttemptLogUnavailable When the log cannot be read.
     */
    public function isLocked(string $address, int $now): bool
    {
        $window = $this->log->read($address);

        return $window !== null
            && self::isOpen($window, $now)
            && $window->failures >= self::MAX_FAILURES;
    }

    /**
     * Counts a wrong key; a closed window is replaced by a new one.
     *
     * @param string $address The caller's address.
     * @param int    $now     The current Unix time.
     *
     * @throws AttemptLogUnavailable When the log cannot be read or written.
     */
    public function recordFailure(string $address, int $now): void
    {
        $window = $this->log->read($address);
        $this->log->write(
            $address,
            $window !== null && self::isOpen($window, $now)
                ? new AttemptWindow($window->failures + 1, $window->startedAt)
                : new AttemptWindow(1, $now),
        );
    }

    /**
     * Forgets the address's failures after it presented the right key.
     *
     * @param string $address The caller's address.
     *
     * @throws AttemptLogUnavailable When the log cannot be written.
     */
    public function clear(string $address): void
    {
        $this->log->clear($address);
    }

    /**
     * Whether a window is still open.
     *
     * @param AttemptWindow $window The window.
     * @param int           $now    The current Unix time.
     *
     * @return bool True while the window lasts.
     */
    private static function isOpen(AttemptWindow $window, int $now): bool
    {
        return $now - $window->startedAt < self::WINDOW_SECONDS;
    }
}
