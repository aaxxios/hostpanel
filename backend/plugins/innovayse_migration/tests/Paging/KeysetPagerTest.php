<?php

declare(strict_types=1);

namespace HostpanelMigration\Tests\Paging;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Paging\KeysetPager;
use HostpanelMigration\Paging\Paging;
use HostpanelMigration\Tests\Fixtures\InMemoryDatabase;
use Illuminate\Database\Connection;
use Illuminate\Database\Schema\Blueprint;
use PHPUnit\Framework\TestCase;

/**
 * Windowing a dataset in id order, by page number or by keyset, and counting it with the same
 * filter the pages read with.
 */
final class KeysetPagerTest extends TestCase
{
    /** The clients seeded by {@see self::seedClients()}. */
    private const CLIENT_COUNT = 25;

    /** The database the test reads. */
    private Connection $connection;

    /** The pager under test. */
    private KeysetPager $pager;

    /**
     * Opens a fresh database with clients 1..25 in it.
     */
    protected function setUp(): void
    {
        $this->connection = InMemoryDatabase::connection();
        $this->seedClients();
        $this->pager = new KeysetPager(new SourceDatabase($this->connection));
    }

    /**
     * A numbered page is ordered by id and skips the pages before it.
     */
    public function testNumberedWindowSkipsThePreviousPages(): void
    {
        $query = KeysetPager::window($this->connection->table('tblclients'), Paging::of(3, 10, null));

        self::assertSame('select * from "tblclients" order by "id" asc limit 10 offset 20', $query->toSql());
        self::assertSame([], $query->getBindings());
    }

    /**
     * A keyset page is ordered by id and starts after the given id, without an offset.
     */
    public function testKeysetWindowStartsAfterTheId(): void
    {
        $query = KeysetPager::window($this->connection->table('tblclients'), Paging::of(1, 10, 42));

        self::assertSame('select * from "tblclients" where "id" > ? order by "id" asc limit 10', $query->toSql());
        self::assertSame([42], $query->getBindings());
    }

    /**
     * A full keyset page names the id the next one starts after; the last, short page names none.
     */
    public function testKeysetPagesNameWhereTheNextOneStarts(): void
    {
        $first = $this->pager->read(Dataset::Clients, Paging::of(1, 10, 0));
        $last = $this->pager->read(Dataset::Clients, Paging::of(1, 10, 20));

        self::assertCount(10, $first->rows);
        self::assertSame(10, $first->nextAfterId);
        self::assertSame(3, $first->totalPages);
        self::assertCount(5, $last->rows);
        self::assertSame(21, (int) $last->rows[0]->id);
        self::assertNull($last->nextAfterId);
    }

    /**
     * A caller paging by number never gets a keyset id, even on a full page.
     */
    public function testNumberedPagesNameNoKeysetId(): void
    {
        $page = $this->pager->read(Dataset::Clients, Paging::of(2, 10, null));

        self::assertCount(10, $page->rows);
        self::assertSame(11, (int) $page->rows[0]->id);
        self::assertNull($page->nextAfterId);
    }

    /**
     * A table this source version does not have reads as an empty dataset with no pages.
     */
    public function testMissingTableReadsAsEmpty(): void
    {
        $page = $this->pager->read(Dataset::NetworkIssues, Paging::of(1, 10, 0));

        self::assertSame([], $page->rows);
        self::assertSame(0, $page->totalPages);
        self::assertNull($page->nextAfterId);
        self::assertSame(0, $this->pager->count(Dataset::NetworkIssues));
    }

    /**
     * A row whose client does not exist is left out of the count and the pages alike, so the
     * totals match what the pages hold.
     */
    public function testRowsOfMissingClientsAreLeftOutOfCountAndPages(): void
    {
        $this->connection->getSchemaBuilder()->create('tblinvoices', static function (Blueprint $table): void {
            $table->increments('id');
            $table->integer('userid');
        });
        $this->connection->table('tblinvoices')->insert([['userid' => 1], ['userid' => 999], ['userid' => 2]]);

        $page = $this->pager->read(Dataset::Invoices, Paging::of(1, 10, null));

        self::assertSame(2, $this->pager->count(Dataset::Invoices));
        self::assertSame([1, 3], array_map(static fn (object $row): int => (int) $row->id, $page->rows));
    }

    /**
     * Creates the client table with {@see self::CLIENT_COUNT} clients.
     */
    private function seedClients(): void
    {
        $this->connection->getSchemaBuilder()->create('tblclients', static function (Blueprint $table): void {
            $table->increments('id');
            $table->string('email');
        });

        $rows = [];
        for ($i = 1; $i <= self::CLIENT_COUNT; $i++) {
            $rows[] = ['email' => "client{$i}@example.com"];
        }
        $this->connection->table('tblclients')->insert($rows);
    }
}
