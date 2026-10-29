<?php

declare(strict_types=1);

namespace HostpanelMigration\Auth;

/**
 * Where the failed key attempts of each address are kept between requests.
 */
interface AttemptLog
{
    /**
     * The address's current window.
     *
     * @param string $address The caller's address.
     *
     * @return AttemptWindow|null The window, or null when the address has no failures on record.
     *
     * @throws AttemptLogUnavailable When the log cannot be read.
     */
    public function read(string $address): ?AttemptWindow;

    /**
     * Stores the address's window.
     *
     * @param string        $address The caller's address.
     * @param AttemptWindow $window  The window to store.
     *
     * @throws AttemptLogUnavailable When the log cannot be written.
     */
    public function write(string $address, AttemptWindow $window): void;

    /**
     * Forgets the address's failures.
     *
     * @param string $address The caller's address.
     *
     * @throws AttemptLogUnavailable When the log cannot be written.
     */
    public function clear(string $address): void;
}
