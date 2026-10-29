<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Database\SourceDatabase;
use HostpanelMigration\Export\ClientOwnedExporter;
use HostpanelMigration\Export\SourceDefaults;
use HostpanelMigration\Mapping\CurrencyMap;
use HostpanelMigration\Mapping\IsoDate;

/**
 * Exports ledger rows that move money (`transactions`): payments and refunds.
 */
final class TransactionsExporter extends ClientOwnedExporter
{
    /**
     * Creates the exporter.
     *
     * @param SourceDatabase $database   The source database, for the client.
     * @param CurrencyMap    $currencies Currency id to ISO code.
     */
    public function __construct(
        SourceDatabase $database,
        /** Currency id to ISO code. */
        private readonly CurrencyMap $currencies,
    ) {
        parent::__construct($database);
    }

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Transactions;
    }

    /**
     * {@inheritDoc}
     */
    protected function record(object $row, string $clientEmail): ?array
    {
        $amountIn = (float) ($row->amountin ?? 0);
        $amountOut = (float) ($row->amountout ?? 0);
        // A row that moves no money is not a payment or a refund, so it is not sent; the dataset
        // filter leaves it out of the count for the same reason.
        if ($amountIn <= 0 && $amountOut <= 0) {
            return null;
        }

        return [
            'clientEmail'   => $clientEmail,
            'externalId'    => (string) $row->id,
            'date'          => IsoDate::from($row->date ?? null),
            'description'   => $row->description ?? '',
            // The gateway reference, or the row's own id when the gateway left none.
            'transactionId' => ($row->transid ?? '') ?: (string) $row->id,
            'invoiceId'     => $row->invoiceid ? (string) $row->invoiceid : null,
            'paymentMethod' => $row->gateway ?: SourceDefaults::PAYMENT_METHOD,
            'amountIn'      => $amountIn,
            'amountOut'     => $amountOut,
            'fees'          => (float) ($row->fees ?? 0),
            // The row stores a currency id; zero means the client's own currency.
            'currency'      => $this->currencies->codeOf($row->currency ?? 0),
        ];
    }
}
