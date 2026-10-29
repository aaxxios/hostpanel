<?php

declare(strict_types=1);

namespace HostpanelMigration\Export;

/**
 * The values a record carries when its source row leaves a field empty, where more than one
 * record kind shares the default.
 */
final class SourceDefaults
{
    /** The billing cycle of a service or order item that names none. */
    public const BILLING_CYCLE = 'monthly';

    /** The payment method of an order or transaction that names none. */
    public const PAYMENT_METHOD = 'bank_transfer';

    /** The status of a service, domain or network issue that names none. */
    public const STATUS = 'Active';

    /** The priority of a ticket or network issue that names none. */
    public const PRIORITY = 'Medium';

    /**
     * Holds constants only; never instantiated.
     */
    private function __construct()
    {
    }
}
