/**
 * Which kind of person a published demo account signs in as, as `GET /api/public/demo` reports it.
 *
 * A closed set the sign-in page branches on, so it is an enum rather than string literals. The
 * values are the backend's wire values and must not be reworded here.
 */
export enum DemoAccountRole {
  /** A staff account for the admin panel; never offered on the customer sign-in page. */
  Admin = 'Admin',
  /** A customer account for the client area. */
  Client = 'Client'
}
