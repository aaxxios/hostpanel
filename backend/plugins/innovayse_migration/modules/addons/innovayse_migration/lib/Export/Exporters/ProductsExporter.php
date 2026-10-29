<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Database\Table;
use HostpanelMigration\Export\Exporter;
use Illuminate\Database\Query\Builder;

/**
 * Exports products (`products`) with their monthly and annual prices.
 *
 * The source keeps no price on the product row: prices sit in its pricing table, one row per
 * product and currency, one column per billing cycle. The record carries the prices in the
 * source's default currency, the one hostpanel imports into its own base currency.
 */
final class ProductsExporter implements Exporter
{
    /** The type of a product that names none. */
    private const DEFAULT_TYPE = 'hostingaccount';

    /** The pricing table's type of a product's price row. */
    private const PRICE_TYPE = 'product';

    /** The payment type of a product the source gives away. */
    private const FREE_PAY_TYPE = 'free';

    /**
     * The payment type of a product the source bills once. Its price sits in the monthly column,
     * but hostpanel has no one-time cycle, so neither cycle carries it.
     */
    private const ONE_TIME_PAY_TYPE = 'onetime';

    /**
     * Creates the exporter.
     *
     * @param SourceDatabase $database   The source database, for the price rows.
     * @param int|null       $currencyId The source's default currency id; null when it marks none,
     *                                   and then the price row of the lowest currency id is read.
     */
    public function __construct(
        /** The source database, for the price rows. */
        private readonly SourceDatabase $database,
        /** The source's default currency id; null when it marks none. */
        private readonly ?int $currencyId,
    ) {
    }

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Products;
    }

    /**
     * {@inheritDoc}
     */
    public function map(object $row): ?array
    {
        [$monthly, $annual] = $this->cyclePrices($row);

        return [
            'id'           => $row->id,
            'groupId'      => $row->gid,
            'name'         => $row->name,
            'description'  => $row->description ?: null,
            'type'         => $row->type ?? self::DEFAULT_TYPE,
            'monthlyPrice' => $monthly,
            'annualPrice'  => $annual,
        ];
    }

    /**
     * The product's monthly and annual prices by its payment type: a free product is sold monthly
     * at zero, a one-time product on neither cycle, any other at its default currency price row.
     *
     * @param object $row The product row.
     *
     * @return array{0: float|null, 1: float|null} The monthly and the annual price; null for a
     *                                             cycle the product is not sold on.
     */
    private function cyclePrices(object $row): array
    {
        return match (strtolower((string) ($row->paytype ?? ''))) {
            self::FREE_PAY_TYPE     => [0.0, null],
            self::ONE_TIME_PAY_TYPE => [null, null],
            default                 => $this->listedPrices($row),
        };
    }

    /**
     * The monthly and annual prices of the product's price row in the default currency.
     *
     * @param object $row The product row.
     *
     * @return array{0: float|null, 1: float|null} The monthly and the annual price; both null when
     *                                             the product has no price row or the source has no
     *                                             pricing table.
     */
    private function listedPrices(object $row): array
    {
        $prices = $this->database->first(Table::PRICING, function (Builder $query) use ($row): ?object {
            $query->where('type', self::PRICE_TYPE)->where('relid', $row->id);
            if ($this->currencyId !== null) {
                $query->where('currency', $this->currencyId);
            }

            return $query->orderBy('currency')->first(['monthly', 'annually']);
        });

        return [self::price($prices?->monthly), self::price($prices?->annually)];
    }

    /**
     * One cycle's price on the wire.
     *
     * @param mixed $value The pricing column; the source writes a negative amount (-1) for a cycle
     *                     it does not sell.
     *
     * @return float|null The price, or null when the product is not sold on the cycle.
     */
    private static function price(mixed $value): ?float
    {
        if ($value === null) {
            return null;
        }

        $amount = (float) $value;

        return $amount < 0 ? null : $amount;
    }
}
