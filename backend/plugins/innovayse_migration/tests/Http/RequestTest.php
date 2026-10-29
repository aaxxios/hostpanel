<?php

declare(strict_types=1);

namespace HostpanelMigration\Tests\Http;

use HostpanelMigration\Http\Action;
use HostpanelMigration\Http\Request;
use HostpanelMigration\Paging\Paging;
use PHPUnit\Framework\TestCase;

/**
 * Reading a request body: the key, the action and the clamped paging window.
 */
final class RequestTest extends TestCase
{
    /**
     * An empty body has no key and no action, and pages by number from page 1 with 100 rows.
     */
    public function testEmptyBodyTakesTheDefaults(): void
    {
        $request = Request::fromBody([]);

        self::assertSame('', $request->key);
        self::assertNull($request->action);
        self::assertSame(Paging::FIRST_PAGE, $request->paging->page);
        self::assertSame(Paging::DEFAULT_PER_PAGE, $request->paging->perPage);
        self::assertNull($request->paging->afterId);
        self::assertFalse($request->paging->isKeyset());
    }

    /**
     * A body that is not a JSON object reads as an empty one.
     */
    public function testBodyThatIsNotAnObjectReadsAsEmpty(): void
    {
        foreach (['', 'not json', '"text"', '42'] as $raw) {
            $request = Request::fromJson($raw);

            self::assertSame('', $request->key, $raw);
            self::assertNull($request->action, $raw);
        }
    }

    /**
     * The key is trimmed and a known action is recognised.
     */
    public function testKeyIsTrimmedAndActionRecognised(): void
    {
        $request = Request::fromJson('{"key":"  abc123  ","action":"ticket_replies"}');

        self::assertSame('abc123', $request->key);
        self::assertSame(Action::TicketReplies, $request->action);
    }

    /**
     * An action the API does not know, or one that is not a string, reads as none.
     */
    public function testUnknownOrNonStringActionReadsAsNone(): void
    {
        self::assertNull(Request::fromBody(['action' => 'drop_tables'])->action);
        self::assertNull(Request::fromBody(['action' => 'Clients'])->action);
        self::assertNull(Request::fromBody(['action' => true])->action);
        self::assertNull(Request::fromBody(['action' => ['clients']])->action);
    }

    /**
     * A key that is not text reads as none, so it fails the key check.
     */
    public function testNonScalarKeyReadsAsNone(): void
    {
        self::assertSame('', Request::fromBody(['key' => ['secret']])->key);
    }

    /**
     * The page is raised to 1 and the page size is kept within 10..500.
     */
    public function testPagingIsClamped(): void
    {
        $low = Request::fromBody(['page' => 0, 'perPage' => 5])->paging;
        $high = Request::fromBody(['page' => -3, 'perPage' => 1000])->paging;
        $text = Request::fromBody(['page' => 'abc', 'perPage' => '50'])->paging;

        self::assertSame(1, $low->page);
        self::assertSame(Paging::MIN_PER_PAGE, $low->perPage);
        self::assertSame(1, $high->page);
        self::assertSame(Paging::MAX_PER_PAGE, $high->perPage);
        self::assertSame(1, $text->page);
        self::assertSame(50, $text->perPage);
    }

    /**
     * `afterId` switches to keyset paging whenever it is present: a negative or null one reads
     * as 0, the start of the dataset.
     */
    public function testAfterIdSwitchesToKeysetPaging(): void
    {
        self::assertSame(42, Request::fromBody(['afterId' => 42])->paging->afterId);
        self::assertSame(0, Request::fromBody(['afterId' => -7])->paging->afterId);
        self::assertSame(0, Request::fromBody(['afterId' => null])->paging->afterId);
        self::assertTrue(Request::fromBody(['afterId' => null])->paging->isKeyset());
    }

    /**
     * A numbered page skips the pages before it.
     */
    public function testOffsetSkipsThePreviousPages(): void
    {
        self::assertSame(0, Paging::of(1, 20, null)->offset());
        self::assertSame(40, Paging::of(3, 20, null)->offset());
    }
}
