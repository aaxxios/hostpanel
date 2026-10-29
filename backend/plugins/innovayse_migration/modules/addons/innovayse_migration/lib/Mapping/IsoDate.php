<?php

declare(strict_types=1);

namespace HostpanelMigration\Mapping;

/**
 * Turns a source date into the ISO 8601 text the wire carries.
 */
final class IsoDate
{
    /** How the source writes "no date": a zero date, alone or followed by a zero time. */
    private const ZERO_DATE_PREFIX = '0000-00-00';

    /** The wire format: ISO 8601 with the offset, in the server's time zone. */
    private const WIRE_FORMAT = 'c';

    /**
     * Holds static helpers only; never instantiated.
     */
    private function __construct()
    {
    }

    /**
     * A source date as ISO 8601, or null when the source holds none: empty, a zero date, or text
     * that is not a date. Never the current time; a missing date stays missing.
     *
     * @param string|null $value The source value.
     *
     * @return string|null The ISO 8601 text, or null.
     */
    public static function from(?string $value): ?string
    {
        if ($value === null || $value === '' || str_starts_with($value, self::ZERO_DATE_PREFIX)) {
            return null;
        }

        $timestamp = strtotime($value);
        if ($timestamp === false) {
            return null;
        }

        return date(self::WIRE_FORMAT, $timestamp);
    }
}
