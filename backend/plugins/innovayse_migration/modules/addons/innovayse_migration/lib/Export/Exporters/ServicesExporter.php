<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Database\Table;
use HostpanelMigration\Export\ClientOwnedExporter;
use HostpanelMigration\Export\SourceDefaults;
use HostpanelMigration\Mapping\IsoDate;

/**
 * Exports provisioned client services (`services`).
 */
final class ServicesExporter extends ClientOwnedExporter
{
    /** The product name of a service whose product no longer exists. */
    private const UNKNOWN_PRODUCT = 'Unknown Product';

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Services;
    }

    /**
     * {@inheritDoc}
     */
    protected function record(object $row, string $clientEmail): ?array
    {
        return [
            'externalId'         => (string) $row->id,
            'clientEmail'        => $clientEmail,
            'productName'        => $this->database->nameOf(Table::PRODUCTS, $row->packageid) ?? self::UNKNOWN_PRODUCT,
            'billingCycle'       => $row->billingcycle ?? SourceDefaults::BILLING_CYCLE,
            'status'             => $row->domainstatus ?? SourceDefaults::STATUS,
            'domain'             => $row->domain ?: null,
            'username'           => $row->username ?: null,
            'nextDueDate'        => IsoDate::from($row->nextduedate ?? null),
            'terminatedAt'       => IsoDate::from($row->terminationdate ?? null),
            'subscriptionId'     => $row->subscriptionid ?: null,
            'adminNotes'         => $row->notes ?: null,
            'recurringAmount'    => (float) ($row->amount ?? 0),
            'firstPaymentAmount' => (float) ($row->firstpaymentamount ?? 0),
            'paymentMethod'      => $row->paymentmethod ?: null,
            'registeredAt'       => IsoDate::from($row->regdate ?? null),
        ];
    }
}
