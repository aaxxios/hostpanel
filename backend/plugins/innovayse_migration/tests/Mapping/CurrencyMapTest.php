<?php

declare(strict_types=1);

namespace HostpanelMigration\Tests\Mapping;

use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Mapping\CurrencyMap;
use HostpanelMigration\Tests\Fixtures\InMemoryDatabase;
use Illuminate\Database\Schema\Blueprint;
use PHPUnit\Framework\TestCase;

/**
 * Currency ids to ISO codes, as the source configures them.
 */
final class CurrencyMapTest extends TestCase
{
    /**
     * A configured id maps to its code, whether the row holds it as a number or as text.
     */
    public function testConfiguredIdMapsToItsCode(): void
    {
        $map = new CurrencyMap([1 => 'USD', 2 => 'EUR']);

        self::assertSame('USD', $map->codeOf(1));
        self::assertSame('EUR', $map->codeOf('2'));
    }

    /**
     * Zero, an empty value and an id no currency has map to no code.
     */
    public function testUnknownOrEmptyIdMapsToNoCode(): void
    {
        $map = new CurrencyMap([1 => 'USD']);

        self::assertNull($map->codeOf(0));
        self::assertNull($map->codeOf(null));
        self::assertNull($map->codeOf(''));
        self::assertNull($map->codeOf(9));
    }

    /**
     * The codes are read from the source's currency table, and a source without one has none.
     */
    public function testCodesAreReadFromTheSource(): void
    {
        $connection = InMemoryDatabase::connection();
        $database = new SourceDatabase($connection);

        self::assertSame([], $database->currencyCodes());

        $connection->getSchemaBuilder()->create('tblcurrencies', static function (Blueprint $table): void {
            $table->increments('id');
            $table->string('code');
        });
        $connection->table('tblcurrencies')->insert([['code' => 'USD'], ['code' => 'AMD']]);

        self::assertSame([1 => 'USD', 2 => 'AMD'], (new SourceDatabase($connection))->currencyCodes());
    }
}
