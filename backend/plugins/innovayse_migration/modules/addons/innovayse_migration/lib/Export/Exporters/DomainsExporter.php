<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Export\ClientOwnedExporter;
use HostpanelMigration\Export\SourceDefaults;
use HostpanelMigration\Mapping\IsoDate;

/**
 * Exports registered domains (`domains`).
 */
final class DomainsExporter extends ClientOwnedExporter
{
    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Domains;
    }

    /**
     * {@inheritDoc}
     */
    protected function record(object $row, string $clientEmail): ?array
    {
        return [
            'clientEmail'        => $clientEmail,
            'domainName'         => $row->domain,
            'registeredAt'       => IsoDate::from($row->registrationdate ?? null),
            'expiresAt'          => IsoDate::from($row->expirydate ?? null),
            'nextDueDate'        => IsoDate::from($row->nextduedate ?? null),
            'registrar'          => $row->registrar ?: null,
            'nameservers'        => self::nameservers($row),
            'status'             => $row->status ?: SourceDefaults::STATUS,
            'autoRenew'          => !$row->donotrenew,
            'recurringAmount'    => (float) ($row->recurringamount ?? 0),
            'firstPaymentAmount' => (float) ($row->firstpaymentamount ?? 0),
            'paymentMethod'      => $row->paymentmethod ?: null,
            'subscriptionId'     => $row->subscriptionid ?: null,
            'adminNotes'         => $row->notes ?: null,
        ];
    }

    /**
     * The domain's nameservers, without the empty slots.
     *
     * @param object $row The domain row.
     *
     * @return list<mixed> The nameservers, in slot order.
     */
    private static function nameservers(object $row): array
    {
        return array_values(array_filter([$row->ns1, $row->ns2, $row->ns3, $row->ns4]));
    }
}
