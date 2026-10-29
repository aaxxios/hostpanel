<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Export\ClientOwnedExporter;

/**
 * Exports client sub-contacts (`contacts`).
 */
final class ContactsExporter extends ClientOwnedExporter
{
    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Contacts;
    }

    /**
     * {@inheritDoc}
     */
    protected function record(object $row, string $clientEmail): ?array
    {
        return [
            'clientEmail' => $clientEmail,
            'firstName'   => $row->firstname ?? '',
            'lastName'    => $row->lastname ?? '',
            'company'     => $row->companyname ?: null,
            'email'       => $row->email ?? '',
            'phone'       => $row->phonenumber ?: null,
            'address'     => $row->address1 ?: null,
            'city'        => $row->city ?: null,
            'state'       => $row->state ?: null,
            'postCode'    => $row->postcode ?: null,
            'country'     => $row->country ?: null,
        ];
    }
}
