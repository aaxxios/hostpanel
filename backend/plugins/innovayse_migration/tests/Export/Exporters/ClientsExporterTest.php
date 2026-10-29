<?php

declare(strict_types=1);

namespace HostpanelMigration\Tests\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Export\Exporters\ClientsExporter;
use HostpanelMigration\Mapping\CurrencyMap;
use PHPUnit\Framework\TestCase;

/**
 * A client row on the wire: the member names, their order and their value formats are the
 * contract hostpanel's client record reads.
 */
final class ClientsExporterTest extends TestCase
{
    /** The time zone the server had before the test, restored afterwards. */
    private string $previousTimeZone;

    /** The exporter under test, with USD as currency 1. */
    private ClientsExporter $exporter;

    /**
     * Pins the server time zone and builds the exporter.
     */
    protected function setUp(): void
    {
        $this->previousTimeZone = date_default_timezone_get();
        date_default_timezone_set('UTC');
        $this->exporter = new ClientsExporter(new CurrencyMap([1 => 'USD']));
    }

    /**
     * Restores the server time zone.
     */
    protected function tearDown(): void
    {
        date_default_timezone_set($this->previousTimeZone);
    }

    /**
     * The exporter reads the client dataset.
     */
    public function testReadsTheClientDataset(): void
    {
        self::assertSame(Dataset::Clients, $this->exporter->dataset());
    }

    /**
     * A full row maps to every wire member, in wire order, with typed values.
     */
    public function testRowMapsToTheWireRecord(): void
    {
        $record = $this->exporter->map(self::row([
            'status'      => 'Closed',
            'currency'    => 1,
            'taxexempt'   => 1,
            'credit'      => '12.50',
            'datecreated' => '2024-01-02',
            'lastlogin'   => '2024-05-06 07:08:09',
        ]));

        self::assertSame([
            'email'         => 'ann@example.com',
            'firstName'     => 'Ann',
            'lastName'      => 'Lee',
            'company'       => 'Lee Ltd',
            'phone'         => '+1 555 0100',
            'address'       => '1 Main St',
            'city'          => 'Springfield',
            'state'         => 'IL',
            'postCode'      => '62701',
            'country'       => 'US',
            'status'        => 'Closed',
            'currency'      => 'USD',
            'taxExempt'     => true,
            'creditBalance' => 12.5,
            'signupDate'    => '2024-01-02T00:00:00+00:00',
            'lastLogin'     => '2024-05-06T07:08:09+00:00',
        ], $record);
    }

    /**
     * Empty optional text becomes null, an unknown status becomes Active, currency 0 has no code
     * and a zero date stays missing.
     */
    public function testEmptyValuesMapToTheirDefaults(): void
    {
        $record = $this->exporter->map(self::row([
            'companyname' => '',
            'phonenumber' => '',
            'state'       => '',
            'status'      => 'Something Else',
            'currency'    => 0,
            'taxexempt'   => 0,
            'credit'      => null,
            'datecreated' => '0000-00-00',
            'lastlogin'   => '0000-00-00 00:00:00',
        ]));

        self::assertNotNull($record);
        self::assertNull($record['company']);
        self::assertNull($record['phone']);
        self::assertNull($record['state']);
        self::assertSame('Active', $record['status']);
        self::assertNull($record['currency']);
        self::assertFalse($record['taxExempt']);
        self::assertSame(0.0, $record['creditBalance']);
        self::assertNull($record['signupDate']);
        self::assertNull($record['lastLogin']);
    }

    /**
     * A client row as the source stores it, with the given columns overridden.
     *
     * @param array<string, mixed> $overrides The columns to change.
     *
     * @return object The row.
     */
    private static function row(array $overrides): object
    {
        return (object) array_merge([
            'id'          => 7,
            'email'       => 'ann@example.com',
            'firstname'   => 'Ann',
            'lastname'    => 'Lee',
            'companyname' => 'Lee Ltd',
            'phonenumber' => '+1 555 0100',
            'address1'    => '1 Main St',
            'city'        => 'Springfield',
            'state'       => 'IL',
            'postcode'    => '62701',
            'country'     => 'US',
            'status'      => 'Active',
            'currency'    => 1,
            'taxexempt'   => 0,
            'credit'      => '0.00',
            'datecreated' => '2024-01-02',
            'lastlogin'   => null,
        ], $overrides);
    }
}
