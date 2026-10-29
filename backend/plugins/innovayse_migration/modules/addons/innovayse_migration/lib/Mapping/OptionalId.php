<?php

declare(strict_types=1);

namespace HostpanelMigration\Mapping;

/**
 * Turns a reference column into the id the wire carries. The source writes "no reference" as 0
 * or an empty value.
 */
final class OptionalId
{
    /**
     * Holds static helpers only; never instantiated.
     */
    private function __construct()
    {
    }

    /**
     * The referenced id, or null when the column holds no reference.
     *
     * @param mixed $value The column value.
     *
     * @return int|null The id, or null.
     */
    public static function from(mixed $value): ?int
    {
        return $value ? (int) $value : null;
    }
}
