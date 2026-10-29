<?php

declare(strict_types=1);

namespace HostpanelMigration\Auth;

/**
 * The failed key attempts one address made in its current window.
 */
final class AttemptWindow
{
    /**
     * Creates a window.
     *
     * @param int $failures  The wrong keys counted in the window.
     * @param int $startedAt When the window's first failure happened (Unix time).
     */
    public function __construct(
        /** The wrong keys counted in the window. */
        public readonly int $failures,
        /** When the window's first failure happened (Unix time). */
        public readonly int $startedAt,
    ) {
    }
}
