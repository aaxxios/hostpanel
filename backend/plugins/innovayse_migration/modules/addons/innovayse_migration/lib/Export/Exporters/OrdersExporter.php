<?php

declare(strict_types=1);

namespace HostpanelMigration\Export\Exporters;

use HostpanelMigration\Database\Dataset;
use HostpanelMigration\Database\Table;
use HostpanelMigration\Export\ClientOwnedExporter;
use HostpanelMigration\Export\OrderLine;
use HostpanelMigration\Export\SourceDefaults;
use HostpanelMigration\Mapping\IsoDate;
use HostpanelMigration\Mapping\OptionalId;
use Illuminate\Database\Query\Builder;
use Illuminate\Support\Collection;

/**
 * Exports orders with their lines (`orders`).
 *
 * The source keeps no order items table: what an order bought are the services, service addons
 * and domains whose rows name the order. Each of them becomes one {@see OrderLine}, services first,
 * then service addons, then domains, each kind in id order.
 */
final class OrdersExporter extends ClientOwnedExporter
{
    /** The status of an order whose source status is not one hostpanel knows. */
    private const DEFAULT_STATUS = 'Pending';

    /** The name of a service line whose product no longer exists. */
    private const UNKNOWN_PRODUCT = 'Unknown Product';

    /** The name of a service addon line that names no addon and whose addon no longer exists. */
    private const UNKNOWN_ADDON = 'Unknown Addon';

    /** The source's domain order type of a transfer; every other domain line is a registration. */
    private const DOMAIN_TRANSFER = 'transfer';

    /** The name of a domain registration line. */
    private const DOMAIN_REGISTRATION_NAME = 'Domain Registration';

    /** The name of a domain transfer line. */
    private const DOMAIN_TRANSFER_NAME = 'Domain Transfer';

    /** The billing cycle of a domain registered for one year, the period that names no other. */
    private const DOMAIN_YEARLY_CYCLE = 'Annually';

    /** Follows the number of years in the cycle of a domain registered for longer than one year. */
    private const DOMAIN_YEARS_SUFFIX = ' Years';

    /**
     * {@inheritDoc}
     */
    public function dataset(): Dataset
    {
        return Dataset::Orders;
    }

    /**
     * {@inheritDoc}
     */
    protected function record(object $row, string $clientEmail): ?array
    {
        return [
            'clientEmail'   => $clientEmail,
            'orderNumber'   => (string) $row->id,
            'paymentMethod' => $row->paymentmethod ?? SourceDefaults::PAYMENT_METHOD,
            'status'        => self::status((string) ($row->status ?? self::DEFAULT_STATUS)),
            'amount'        => (float) $row->amount,
            'createdAt'     => IsoDate::from($row->date ?? null),
            'items'         => array_map(static fn (OrderLine $line): array => $line->toWire(), $this->lines($row->id)),
        ];
    }

    /**
     * The order's lines.
     *
     * @param mixed $orderId The order id.
     *
     * @return list<OrderLine> The lines.
     */
    private function lines(mixed $orderId): array
    {
        $lines = [];
        foreach ($this->rowsOf(Table::SERVICES, $orderId) as $service) {
            $lines[] = $this->serviceLine($service);
        }

        foreach ($this->rowsOf(Table::SERVICE_ADDONS, $orderId) as $addon) {
            $lines[] = $this->addonLine($addon);
        }

        foreach ($this->rowsOf(Table::DOMAINS, $orderId) as $domain) {
            $lines[] = self::domainLine($domain);
        }

        return $lines;
    }

    /**
     * The one filter every line table is read with: the rows that name the order, in id order.
     *
     * @param string $table   The line table.
     * @param mixed  $orderId The order id.
     *
     * @return Collection The rows; empty when the table does not exist in this source version.
     */
    private function rowsOf(string $table, mixed $orderId): Collection
    {
        return $this->database->rows(
            $table,
            static fn (Builder $query): Collection => $query->where(Table::ORDER_ID, $orderId)->orderBy(Table::ID)->get(),
        );
    }

    /**
     * A service row as a line: its product, cycle and both amounts.
     *
     * @param object $service The service row.
     *
     * @return OrderLine The line.
     */
    private function serviceLine(object $service): OrderLine
    {
        return new OrderLine(
            OptionalId::from($service->packageid),
            $this->database->nameOf(Table::PRODUCTS, $service->packageid) ?? self::UNKNOWN_PRODUCT,
            $service->billingcycle ?: SourceDefaults::BILLING_CYCLE,
            (float) ($service->firstpaymentamount ?? 0),
            (float) ($service->amount ?? 0),
            $service->domain ?: null,
        );
    }

    /**
     * A service addon row as a line. An addon is no product, so the line names none; its first
     * payment is the setup fee and the first recurring charge, as the source bills it.
     *
     * @param object $addon The service addon row.
     *
     * @return OrderLine The line.
     */
    private function addonLine(object $addon): OrderLine
    {
        $recurring = (float) ($addon->recurring ?? 0);

        return new OrderLine(
            null,
            $addon->name ?: ($this->database->nameOf(Table::ADDONS, $addon->addonid) ?? self::UNKNOWN_ADDON),
            $addon->billingcycle ?: SourceDefaults::BILLING_CYCLE,
            (float) ($addon->setupfee ?? 0) + $recurring,
            $recurring,
            null,
        );
    }

    /**
     * A domain row as a line. A domain is no product, so the line names none; it is billed per
     * registration period in years, so a one-year period is the annual cycle and a longer one is
     * sent as its number of years.
     *
     * @param object $domain The domain row.
     *
     * @return OrderLine The line.
     */
    private static function domainLine(object $domain): OrderLine
    {
        $years = (int) ($domain->registrationperiod ?? 1);

        return new OrderLine(
            null,
            strtolower((string) ($domain->type ?? '')) === self::DOMAIN_TRANSFER
                ? self::DOMAIN_TRANSFER_NAME
                : self::DOMAIN_REGISTRATION_NAME,
            $years <= 1 ? self::DOMAIN_YEARLY_CYCLE : $years . self::DOMAIN_YEARS_SUFFIX,
            (float) ($domain->firstpaymentamount ?? 0),
            (float) ($domain->recurringamount ?? 0),
            $domain->domain ?: null,
        );
    }

    /**
     * Normalises an order status to the words hostpanel expects.
     *
     * @param string $status The source status.
     *
     * @return string The hostpanel status.
     */
    private static function status(string $status): string
    {
        return match (strtolower($status)) {
            'active'    => 'Active',
            'cancelled' => 'Cancelled',
            'fraud'     => 'Fraud',
            default     => self::DEFAULT_STATUS,
        };
    }
}
