/** The steps of the hosting setup wizard, in order; the value is the 1-based step number. */
export enum SetupStep {
  /** Choose the domain. */
  Domain = 1,
  /** Choose control-panel credentials. */
  Credentials = 2,
  /** Review and launch. */
  Review = 3
}
