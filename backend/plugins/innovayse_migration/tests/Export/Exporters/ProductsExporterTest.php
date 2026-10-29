<?php

declare(strict_types=1);

namespace HostpanelMigration\Tests\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Export\Exporters\ProductsExporter;
use HostpanelMigration\Tests\Fixtures\InMemoryDatabase;
use Illuminate\Database\Connection;
use Illuminate\Database\Schema\Blueprint;
use PHPUnit\Framework\TestCase;

/**
 * A product row on the wire, with its monthly and annual prices read from the source's pricing
 * table in the source's default currency.
 */
final class ProductsExporterTest extends TestCase
{
    /** The product every test maps. */
    private const PRODUCT_ID = 5;

    /** The currency the source does not mark as its default. */
    private const OTHER_CURRENCY = 1;

    /** The currency the source marks as its default. */
    private const DEFAULT_CURRENCY = 2;

    /** The database the test reads. */
    private Connection $connection;

    /** The gateway over the database. */
    private SourceDatabase $database;

    /**
     * Opens a fresh database with the currency and pricing tables in it.
     */
    protected function setUp(): void
    {
        $this->connection = InMemoryDatabase::connection();
        $this->database = new SourceDatabase($this->connection);
        $schema = $this->connection->getSchemaBuilder();
        $schema->create('tblcurrencies', static function (Blueprint $table): void {
            $table->increments('id');
            $table->string('code');
            $table->integer('default');
        });
        $schema->create('tblpricing', static function (Blueprint $table): void {
            $table->increments('id');
            $table->string('type');
            $table->integer('currency');
            $table->integer('relid');
            $table->decimal('monthly', 16, 2);
            $table->decimal('annually', 16, 2);
        });
        $this->connection->table('tblcurrencies')->insert([
            ['id' => self::OTHER_CURRENCY, 'code' => 'USD', 'default' => 0],
            ['id' => self::DEFAULT_CURRENCY, 'code' => 'EUR', 'default' => 1],
        ]);
    }

    /**
     * The exporter reads the product dataset.
     */
    public function testReadsTheProductDataset(): void
    {
        self::assertSame(Dataset::Products, $this->exporter()->dataset());
    }

    /**
     * A recurring product maps to every wire member, with the prices of its default currency row;
     * a cycle the source does not sell (-1) is sent as null.
     */
    public function testRecurringProductCarriesItsDefaultCurrencyPrices(): void
    {
        $this->price(self::OTHER_CURRENCY, '1.00', '10.00');
        $this->price(self::DEFAULT_CURRENCY, '9.99', '-1.00');
        $this->price(self::DEFAULT_CURRENCY, '7.00', '70.00', 'addon');

        $record = $this->exporter()->map(self::row('recurring'));

        self::assertSame([
            'id'           => self::PRODUCT_ID,
            'groupId'      => 3,
            'name'         => 'Starter',
            'description'  => null,
            'type'         => 'hostingaccount',
            'monthlyPrice' => 9.99,
            'annualPrice'  => null,
        ], $record);
    }

    /**
     * The default currency id is read from the source's currency table.
     */
    public function testDefaultCurrencyIsTheOneTheSourceMarks(): void
    {
        self::assertSame(self::DEFAULT_CURRENCY, $this->database->defaultCurrencyId());
    }

    /**
     * When the source marks no default currency, the price row of the lowest currency id is read.
     */
    public function testWithoutADefaultCurrencyTheLowestCurrencyRowIsRead(): void
    {
        $this->price(self::DEFAULT_CURRENCY, '9.99', '99.00');
        $this->price(self::OTHER_CURRENCY, '1.00', '10.00');

        $record = (new ProductsExporter($this->database, null))->map(self::row('recurring'));

        self::assertNotNull($record);
        self::assertSame(1.0, $record['monthlyPrice']);
        self::assertSame(10.0, $record['annualPrice']);
    }

    /**
     * A free product is sold monthly at zero and never annually, whatever its price row says.
     */
    public function testFreeProductIsSoldMonthlyAtZero(): void
    {
        $this->price(self::DEFAULT_CURRENCY, '9.99', '99.00');

        $record = $this->exporter()->map(self::row('free'));

        self::assertNotNull($record);
        self::assertSame(0.0, $record['monthlyPrice']);
        self::assertNull($record['annualPrice']);
    }

    /**
     * A one-time product is sold on neither cycle: hostpanel has no one-time cycle to carry it.
     */
    public function testOneTimeProductIsSoldOnNeitherCycle(): void
    {
        $this->price(self::DEFAULT_CURRENCY, '25.00', '-1.00');

        $record = $this->exporter()->map(self::row('onetime'));

        self::assertNotNull($record);
        self::assertNull($record['monthlyPrice']);
        self::assertNull($record['annualPrice']);
    }

    /**
     * A product with no price row is sold on neither cycle.
     */
    public function testProductWithoutAPriceRowIsSoldOnNeitherCycle(): void
    {
        $record = $this->exporter()->map(self::row('recurring'));

        self::assertNotNull($record);
        self::assertNull($record['monthlyPrice']);
        self::assertNull($record['annualPrice']);
    }

    /**
     * A source version without the pricing table sells every product on neither cycle.
     */
    public function testSourceWithoutAPricingTableSellsOnNeitherCycle(): void
    {
        // The gateway remembers which tables exist for the length of an export, so the source
        // without the table is opened after it is dropped, as a real export would see it.
        $this->connection->getSchemaBuilder()->drop('tblpricing');
        $database = new SourceDatabase($this->connection);

        $record = (new ProductsExporter($database, $database->defaultCurrencyId()))->map(self::row('recurring'));

        self::assertNotNull($record);
        self::assertNull($record['monthlyPrice']);
        self::assertNull($record['annualPrice']);
    }

    /**
     * The exporter as the router builds it, with the source's default currency.
     *
     * @return ProductsExporter The exporter.
     */
    private function exporter(): ProductsExporter
    {
        return new ProductsExporter($this->database, $this->database->defaultCurrencyId());
    }

    /**
     * Adds a price row for the product.
     *
     * @param int    $currency The currency id.
     * @param string $monthly  The monthly column.
     * @param string $annually The annual column.
     * @param string $type     The price row's type.
     */
    private function price(int $currency, string $monthly, string $annually, string $type = 'product'): void
    {
        $this->connection->table('tblpricing')->insert([
            'type'     => $type,
            'currency' => $currency,
            'relid'    => self::PRODUCT_ID,
            'monthly'  => $monthly,
            'annually' => $annually,
        ]);
    }

    /**
     * A product row as the source stores it: no price columns.
     *
     * @param string $payType The product's payment type.
     *
     * @return object The row.
     */
    private static function row(string $payType): object
    {
        return (object) [
            'id'          => self::PRODUCT_ID,
            'gid'         => 3,
            'name'        => 'Starter',
            'description' => '',
            'type'        => 'hostingaccount',
            'paytype'     => $payType,
        ];
    }
}
