<?php

declare(strict_types=1);

namespace HostpanelMigration\Tests\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Export\Exporters\OrdersExporter;
use HostpanelMigration\Tests\Fixtures\InMemoryDatabase;
use Illuminate\Database\Connection;
use Illuminate\Database\Schema\Blueprint;
use PHPUnit\Framework\TestCase;

/**
 * An order row on the wire, with its lines built from the services, service addons and domains
 * that name the order: the source keeps no order items table.
 */
final class OrdersExporterTest extends TestCase
{
    /** The order every test maps. */
    private const ORDER_ID = 40;

    /** Another order, whose rows never become lines of {@see self::ORDER_ID}. */
    private const OTHER_ORDER_ID = 41;

    /** The client who placed the order. */
    private const CLIENT_ID = 7;

    /** The time zone the server had before the test, restored afterwards. */
    private string $previousTimeZone;

    /** The database the test reads. */
    private Connection $connection;

    /** The exporter under test. */
    private OrdersExporter $exporter;

    /**
     * Pins the server time zone and opens a fresh database with the client, a product and an
     * addon in it, and empty line tables.
     */
    protected function setUp(): void
    {
        $this->previousTimeZone = date_default_timezone_get();
        date_default_timezone_set('UTC');
        $this->connection = InMemoryDatabase::connection();
        $this->exporter = new OrdersExporter(new SourceDatabase($this->connection));

        $schema = $this->connection->getSchemaBuilder();
        $schema->create('tblclients', static function (Blueprint $table): void {
            $table->increments('id');
            $table->string('email');
        });
        $schema->create('tblproducts', static function (Blueprint $table): void {
            $table->increments('id');
            $table->string('name');
        });
        $schema->create('tbladdons', static function (Blueprint $table): void {
            $table->increments('id');
            $table->string('name');
        });
        $schema->create('tblhosting', static function (Blueprint $table): void {
            $table->increments('id');
            $table->integer('orderid');
            $table->integer('packageid');
            $table->string('domain');
            $table->string('billingcycle');
            $table->decimal('firstpaymentamount', 16, 2);
            $table->decimal('amount', 16, 2);
        });
        $schema->create('tblhostingaddons', static function (Blueprint $table): void {
            $table->increments('id');
            $table->integer('orderid');
            $table->integer('addonid');
            $table->string('name');
            $table->string('billingcycle');
            $table->decimal('setupfee', 16, 2);
            $table->decimal('recurring', 16, 2);
        });
        $schema->create('tbldomains', static function (Blueprint $table): void {
            $table->increments('id');
            $table->integer('orderid');
            $table->string('type');
            $table->string('domain');
            $table->integer('registrationperiod');
            $table->decimal('firstpaymentamount', 16, 2);
            $table->decimal('recurringamount', 16, 2);
        });

        $this->connection->table('tblclients')->insert(['id' => self::CLIENT_ID, 'email' => 'ann@example.com']);
        $this->connection->table('tblproducts')->insert(['id' => 5, 'name' => 'Starter']);
        $this->connection->table('tbladdons')->insert(['id' => 9, 'name' => 'Daily Backups']);
    }

    /**
     * Restores the server time zone.
     */
    protected function tearDown(): void
    {
        date_default_timezone_set($this->previousTimeZone);
    }

    /**
     * The exporter reads the order dataset.
     */
    public function testReadsTheOrderDataset(): void
    {
        self::assertSame(Dataset::Orders, $this->exporter->dataset());
    }

    /**
     * Every service, service addon and domain of the order becomes one line, in that order, and
     * the rows of another order do not.
     */
    public function testOrderLinesAreItsServicesAddonsAndDomains(): void
    {
        $this->connection->table('tblhosting')->insert([
            ['orderid' => self::ORDER_ID, 'packageid' => 5, 'domain' => 'ann.example', 'billingcycle' => 'Monthly', 'firstpaymentamount' => '12.00', 'amount' => '10.00'],
            ['orderid' => self::OTHER_ORDER_ID, 'packageid' => 5, 'domain' => 'other.example', 'billingcycle' => 'Monthly', 'firstpaymentamount' => '1.00', 'amount' => '1.00'],
        ]);
        $this->connection->table('tblhostingaddons')->insert([
            ['orderid' => self::ORDER_ID, 'addonid' => 9, 'name' => '', 'billingcycle' => 'Annually', 'setupfee' => '5.00', 'recurring' => '20.00'],
        ]);
        $this->connection->table('tbldomains')->insert([
            ['orderid' => self::ORDER_ID, 'type' => 'Register', 'domain' => 'ann.example', 'registrationperiod' => 1, 'firstpaymentamount' => '8.00', 'recurringamount' => '11.00'],
            ['orderid' => self::ORDER_ID, 'type' => 'Transfer', 'domain' => 'lee.example', 'registrationperiod' => 2, 'firstpaymentamount' => '9.00', 'recurringamount' => '18.00'],
        ]);

        $record = $this->exporter->map(self::row());

        self::assertSame([
            'clientEmail'   => 'ann@example.com',
            'orderNumber'   => (string) self::ORDER_ID,
            'paymentMethod' => 'banktransfer',
            'status'        => 'Active',
            'amount'        => 61.0,
            'createdAt'     => '2024-03-04T05:06:07+00:00',
            'items'         => [
                self::item(5, 'Starter', 'Monthly', 12.0, 10.0, 'ann.example'),
                self::item(null, 'Daily Backups', 'Annually', 25.0, 20.0, null),
                self::item(null, 'Domain Registration', 'Annually', 8.0, 11.0, 'ann.example'),
                self::item(null, 'Domain Transfer', '2 Years', 9.0, 18.0, 'lee.example'),
            ],
        ], $record);
    }

    /**
     * A service whose product is gone and an addon with neither its own name nor an existing
     * addon still become lines, under the unknown names.
     */
    public function testLinesWithoutTheirProductOrAddonKeepAnUnknownName(): void
    {
        $this->connection->table('tblhosting')->insert([
            ['orderid' => self::ORDER_ID, 'packageid' => 99, 'domain' => '', 'billingcycle' => '', 'firstpaymentamount' => '0.00', 'amount' => '0.00'],
        ]);
        $this->connection->table('tblhostingaddons')->insert([
            ['orderid' => self::ORDER_ID, 'addonid' => 0, 'name' => '', 'billingcycle' => 'Monthly', 'setupfee' => '0.00', 'recurring' => '3.00'],
        ]);

        $record = $this->exporter->map(self::row());

        self::assertNotNull($record);
        self::assertSame([
            self::item(99, 'Unknown Product', 'monthly', 0.0, 0.0, null),
            self::item(null, 'Unknown Addon', 'Monthly', 3.0, 3.0, null),
        ], $record['items']);
    }

    /**
     * A line table this source version does not have contributes no lines; an order with no
     * lines at all is still sent.
     */
    public function testMissingLineTablesContributeNoLines(): void
    {
        $schema = $this->connection->getSchemaBuilder();
        $schema->drop('tblhostingaddons');
        $schema->drop('tbldomains');

        $record = $this->exporter->map(self::row());

        self::assertNotNull($record);
        self::assertSame([], $record['items']);
    }

    /**
     * An order row as the source stores it.
     *
     * @return object The row.
     */
    private static function row(): object
    {
        return (object) [
            'id'            => self::ORDER_ID,
            'userid'        => self::CLIENT_ID,
            'paymentmethod' => 'banktransfer',
            'status'        => 'Active',
            'amount'        => '61.00',
            'date'          => '2024-03-04 05:06:07',
        ];
    }

    /**
     * One expected wire item.
     *
     * @param int|null    $productId          The product id.
     * @param string      $productName        The line name.
     * @param string      $billingCycle       The billing cycle word.
     * @param float       $firstPaymentAmount The first payment.
     * @param float       $recurringAmount    The recurring amount.
     * @param string|null $domain             The domain.
     *
     * @return array<string, mixed> The wire item.
     */
    private static function item(
        ?int $productId,
        string $productName,
        string $billingCycle,
        float $firstPaymentAmount,
        float $recurringAmount,
        ?string $domain,
    ): array {
        return [
            'productId'          => $productId,
            'productName'        => $productName,
            'billingCycle'       => $billingCycle,
            'firstPaymentAmount' => $firstPaymentAmount,
            'recurringAmount'    => $recurringAmount,
            'domain'             => $domain,
            'hostname'           => null,
        ];
    }
}
