<?php

declare(strict_types=1);

namespace HostpanelMigration\Tests\Mapping;

use HostpanelMigration\Mapping\IsoDate;
use PHPUnit\Framework\TestCase;

/**
 * Source dates on the wire: ISO 8601, or null when the source holds none.
 */
final class IsoDateTest extends TestCase
{
    /** The time zone the server had before the test, restored afterwards. */
    private string $previousTimeZone;

    /**
     * Pins the server time zone, so the expected offsets are fixed.
     */
    protected function setUp(): void
    {
        $this->previousTimeZone = date_default_timezone_get();
        date_default_timezone_set('UTC');
    }

    /**
     * Restores the server time zone.
     */
    protected function tearDown(): void
    {
        date_default_timezone_set($this->previousTimeZone);
    }

    /**
     * A date, with or without a time, is written in ISO 8601 with its offset.
     */
    public function testDateIsWrittenInIso8601(): void
    {
        self::assertSame('2024-03-05T00:00:00+00:00', IsoDate::from('2024-03-05'));
        self::assertSame('2024-03-05T14:30:09+00:00', IsoDate::from('2024-03-05 14:30:09'));
    }

    /**
     * No value, an empty one, a zero date and text that is not a date are all null; never the
     * current time.
     */
    public function testMissingDateIsNull(): void
    {
        self::assertNull(IsoDate::from(null));
        self::assertNull(IsoDate::from(''));
        self::assertNull(IsoDate::from('0000-00-00'));
        self::assertNull(IsoDate::from('0000-00-00 00:00:00'));
        self::assertNull(IsoDate::from('not a date'));
    }
}
