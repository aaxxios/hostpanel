<?php

declare(strict_types=1);

namespace HostpanelMigration\Mapping;

/**
 * Currency id to ISO code, as configured in the source. Rows store a currency id; zero means the
 * client's own currency and maps to no code.
 */
final class CurrencyMap
{
    /**
     * Creates the map.
     *
     * @param array<int, string> $codes The ISO codes by currency id.
     */
    public function __construct(
        /** @var array<int, string> The ISO codes by currency id. */
        private readonly array $codes,
    ) {
    }

    /**
     * The ISO code of a currency id as a row holds it.
     *
     * @param mixed $currencyId The id; null, empty or not numeric reads as 0.
     *
     * @return string|null The code, or null when the id names no configured currency.
     */
    public function codeOf(mixed $currencyId): ?string
    {
        return $this->codes[(int) $currencyId] ?? null;
    }
}
