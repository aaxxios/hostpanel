<?php

declare(strict_types=1);

namespace HostpanelMigration\Paging;

/**
 * One window of a dataset as read from the database, before its rows are mapped.
 */
final class Page
{
    /**
     * Creates a page.
     *
     * @param list<object> $rows        The rows read, in id order.
     * @param int          $totalPages  How many pages of this size the dataset holds.
     * @param int|null     $nextAfterId The id the next keyset page starts after.
     */
    public function __construct(
        /** @var list<object> The rows read, in id order. */
        public readonly array $rows,
        /** How many pages of this size the dataset holds; 0 when it is empty. */
        public readonly int $totalPages,
        /**
         * The id the next keyset page starts after: null on the last page, and always null when
         * the caller pages by number.
         */
        public readonly ?int $nextAfterId,
    ) {
    }
}
