/**
 * How a deployment signs people in; the values are what `AUTH_MODE` holds.
 *
 * @module types/authmode
 */
export enum AuthMode {
  /** The platform single-sign-on server is the identity provider. */
  Sso = 'sso',
  /** The portal keeps its own accounts and sign-in forms. */
  Local = 'local'
}
