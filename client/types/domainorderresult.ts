/** What a domain renewal or transfer order answers with. */
export interface DomainOrderResult {
  /** The new order's id. */
  orderId: number
  /** The invoice raised for the order. */
  invoiceId: number
}
