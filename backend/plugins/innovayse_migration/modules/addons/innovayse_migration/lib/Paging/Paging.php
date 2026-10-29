<?php

declare(strict_types=1);

namespace HostpanelMigration\Paging;

/**
 * Which window of a dataset a caller asks for. A caller that sends `afterId` pages by keyset
 * (the rows whose id is greater); one that does not pages by `page` number.
 */
final class Paging
{
    /** The first page number; a smaller one is raised to it. */
    public const FIRST_PAGE = 1;

    /** The page size when the caller sends none. */
    public const DEFAULT_PER_PAGE = 100;

    /** The smallest page size served; a smaller one is raised to it. */
    public const MIN_PER_PAGE = 10;

    /** The largest page size served; a larger one is lowered to it. */
    public const MAX_PER_PAGE = 500;

    /** The smallest keyset id; a negative one is raised to it. */
    private const MIN_AFTER_ID = 0;

    /**
     * Creates a window that is already within bounds; use {@see self::of()} to clamp.
     *
     * @param int      $page    The page number, from 1.
     * @param int      $perPage The page size.
     * @param int|null $afterId The id the page starts after, or null when paging by number.
     */
    private function __construct(
        /** The page number, from 1; ignored when paging by keyset. */
        public readonly int $page,
        /** The most rows the page holds. */
        public readonly int $perPage,
        /** The id the page starts after, or null when the caller pages by number. */
        public readonly ?int $afterId,
    ) {
    }

    /**
     * A window with every value clamped into bounds: the page to at least 1, the size to
     * 10..500 and the keyset id to at least 0.
     *
     * @param int      $page    The requested page number.
     * @param int      $perPage The requested page size.
     * @param int|null $afterId The requested keyset id, or null to page by number.
     *
     * @return self The window.
     */
    public static function of(int $page, int $perPage, ?int $afterId): self
    {
        return new self(
            max(self::FIRST_PAGE, $page),
            max(self::MIN_PER_PAGE, min(self::MAX_PER_PAGE, $perPage)),
            $afterId === null ? null : max(self::MIN_AFTER_ID, $afterId),
        );
    }

    /**
     * Reads the window from a decoded request body. A missing `page` or `perPage` takes its
     * default; `afterId` switches to keyset paging whenever the member is present, even when it
     * is null (which reads as 0, the start of the dataset).
     *
     * @param array<mixed> $body The decoded request body.
     *
     * @return self The window.
     */
    public static function fromBody(array $body): self
    {
        return self::of(
            (int) ($body['page'] ?? self::FIRST_PAGE),
            (int) ($body['perPage'] ?? self::DEFAULT_PER_PAGE),
            array_key_exists('afterId', $body) ? (int) $body['afterId'] : null,
        );
    }

    /**
     * Whether the caller pages by keyset.
     *
     * @return bool True when `afterId` was sent.
     */
    public function isKeyset(): bool
    {
        return $this->afterId !== null;
    }

    /**
     * How many rows a numbered page skips.
     *
     * @return int The offset of the page's first row.
     */
    public function offset(): int
    {
        return ($this->page - 1) * $this->perPage;
    }
}
