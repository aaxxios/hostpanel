<?php

declare(strict_types=1);

namespace HostpanelMigration\Tests\Fixtures;

use HostpanelMigration\Auth\AttemptLog;
use HostpanelMigration\Auth\AttemptWindow;

/**
 * An attempt log kept in memory, standing in for the file system in the throttle tests.
 */
final class InMemoryAttemptLog implements AttemptLog
{
    /** @var array<string, AttemptWindow> The windows by address. */
    private array $windows = [];

    /**
     * {@inheritDoc}
     */
    public function read(string $address): ?AttemptWindow
    {
        return $this->windows[$address] ?? null;
    }

    /**
     * {@inheritDoc}
     */
    public function write(string $address, AttemptWindow $window): void
    {
        $this->windows[$address] = $window;
    }

    /**
     * {@inheritDoc}
     */
    public function clear(string $address): void
    {
        unset($this->windows[$address]);
    }
}
