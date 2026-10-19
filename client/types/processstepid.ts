/**
 * Identifier of a process step.
 *
 * @module types/processstepid
 */

/** Identifier of a process step; doubles as the key of its copy in the locale files. */
export enum ProcessStepId {
  /** Understanding the client's needs; first step. */
  Discovery = 'discovery',
  /** Designing the solution. */
  Design = 'design',
  /** Building the solution. */
  Development = 'development',
  /** Releasing the result; last step. */
  Launch = 'launch',
}
