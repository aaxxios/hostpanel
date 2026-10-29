<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Export\Exporter;
use HostpanelMigration\Mapping\CurrencyMap;
use HostpanelMigration\Mapping\IsoDate;

/**
 * Exports client accounts (`clients`).
 */
final class ClientsExporter implements Exporter
{
    /** The status of a client whose source status is not one hostpanel knows. */
    private const DEFAULT_STATUS = 'Active';

    /**
     * Creates the exporter.
     *
     * @param CurrencyMap $currencies Currency id to ISO code.
     */
    public function __construct(
        /** Currency id to ISO code. */
        private readonly CurrencyMap $currencies,
    ) {
    }

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Clients;
    }

    /**
     * {@inheritDoc}
     */
    public function map(object $row): ?array
    {
        return [
            'email'         => $row->email,
            'firstName'     => $row->firstname,
            'lastName'      => $row->lastname,
            'company'       => $row->companyname ?: null,
            'phone'         => $row->phonenumber ?: null,
            'address'       => $row->address1 ?: null,
            'city'          => $row->city ?: null,
            'state'         => $row->state ?: null,
            'postCode'      => $row->postcode ?: null,
            'country'       => $row->country ?: null,
            'status'        => self::status((string) $row->status),
            'currency'      => $this->currencies->codeOf($row->currency ?? 0),
            'taxExempt'     => (bool) ($row->taxexempt ?? false),
            'creditBalance' => (float) ($row->credit ?? 0),
            'signupDate'    => IsoDate::from($row->datecreated ?? null),
            'lastLogin'     => IsoDate::from($row->lastlogin ?? null),
        ];
    }

    /**
     * Normalises a client status to the words hostpanel expects.
     *
     * @param string $status The source status.
     *
     * @return string The hostpanel status.
     */
    private static function status(string $status): string
    {
        return match (strtolower($status)) {
            'inactive' => 'Inactive',
            'closed'   => 'Closed',
            default    => self::DEFAULT_STATUS,
        };
    }
}
