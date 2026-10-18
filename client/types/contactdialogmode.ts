/**
 * Whether the contact dialog creates or edits.
 *
 * @module types/contactdialogmode
 */

/** The direction the contact dialog was opened in. */
export enum ContactDialogMode {
  /** A new contact. */
  Add = 'add',
  /** An existing contact. */
  Edit = 'edit'
}
