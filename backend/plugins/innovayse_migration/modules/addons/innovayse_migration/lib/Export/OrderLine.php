<?php

declare(strict_types=1);

namespace HostpanelMigration\Export;

/**
 * One line of an order, whichever source row it was read from (a service, a service addon or a
 * domain), and the one mapping of a line to the order item wire record hostpanel reads.
 */
final class OrderLine
{
    /**
     * Creates the line.
     *
     * @param int|null    $productId          The source id of the ordered product; null when the line is no product.
     * @param string      $productName        The name the line is ordered under.
     * @param string      $billingCycle       The source's billing cycle word.
     * @param float       $firstPaymentAmount The amount billed on the first invoice.
     * @param float       $recurringAmount    The amount billed on each renewal.
     * @param string|null $domain             The domain the line is for; null when none.
     */
    public function __construct(
        /** The source id of the ordered product; null when the line is no product. */
        public readonly ?int $productId,
        /** The name the line is ordered under. */
        public readonly string $productName,
        /** The source's billing cycle word. */
        public readonly string $billingCycle,
        /** The amount billed on the first invoice. */
        public readonly float $firstPaymentAmount,
        /** The amount billed on each renewal. */
        public readonly float $recurringAmount,
        /** The domain the line is for; null when none. */
        public readonly ?string $domain,
    ) {
    }

    /**
     * The line as the order item wire record.
     *
     * @return array<string, mixed> The wire item.
     */
    public function toWire(): array
    {
        return [
            'productId'          => $this->productId,
            'productName'        => $this->productName,
            'billingCycle'       => $this->billingCycle,
            'firstPaymentAmount' => $this->firstPaymentAmount,
            'recurringAmount'    => $this->recurringAmount,
            'domain'             => $this->domain,
            // The source keeps no host name apart from the domain on any of the rows a line is read from.
            'hostname'           => null,
        ];
    }
}
