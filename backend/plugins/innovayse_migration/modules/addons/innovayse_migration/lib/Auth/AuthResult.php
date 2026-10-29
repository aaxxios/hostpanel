<?php

declare(strict_types=1);

namespace HostpanelMigration\Auth;

/**
 * How a presented key was judged.
 */
enum AuthResult
{
    /** The key matches the configured one. */
    case Accepted;

    /** The key is missing, unconfigured or wrong. */
    case Rejected;

    /** The address sent too many wrong keys; the key was not compared. */
    case Throttled;
}
