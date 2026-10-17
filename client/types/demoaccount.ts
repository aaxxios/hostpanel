/**
 * The published demo account type.
 *
 * @module types/demoaccount
 */

import type { DemoAccountRole } from '~/types/demoaccountrole'

/**
 * One published demo account: public credentials a visitor may use to try the product.
 */
export interface DemoAccount {
  /** Which area the account signs in to. */
  role: DemoAccountRole
  /** Sign-in email address. */
  email: string
  /** Sign-in password, published on purpose because the data resets. */
  password: string
}
