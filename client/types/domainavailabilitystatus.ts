/** Whether a checked domain name can be registered, as the API writes it (by name). */
export enum DomainAvailabilityStatus {
  /** Nobody holds the name; it can be registered. */
  Available = 'Available',
  /** The name is already registered. */
  Taken = 'Taken'
}
