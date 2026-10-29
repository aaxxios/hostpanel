/** The shapes a Nitro route parameter may have; `requireRouteParam` validates against one of them. */
export enum RouteParamKind {
  /** A positive 32-bit integer primary key, e.g. a service, invoice or ticket id. */
  Int = 'int',
  /** A GUID in its hyphenated form, e.g. an SSH key id. */
  Guid = 'guid',
  /** An opaque identifier of letters, digits, underscores and hyphens, up to 128 characters, e.g. a payment-method or person id. */
  Identifier = 'identifier'
}
