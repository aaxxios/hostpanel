<?php

declare(strict_types=1);

namespace HostpanelMigration\Auth;

use RuntimeException;

/**
 * The failed attempt log cannot be read or written. The throttle fails closed on it: no key is
 * checked while the log is unavailable.
 */
final class AttemptLogUnavailable extends RuntimeException
{
}
