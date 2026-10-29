namespace Innovayse.Hostpanel.Shared.Contracts.Interfaces;

/// <summary>
/// Marker for the command of a recurring job: a message its handler runs through
/// <c>RecurringJobs</c> and then schedules again. A pass loops over every item the job owns, so it
/// may run far longer than Wolverine's default execution timeout; the recurring-job timeout policy
/// gives exactly the chains whose message carries this marker the timeout of the job lease instead,
/// and <c>RecurringJobs</c> accepts only a command that carries it, so a new job cannot be missed.
/// </summary>
public interface IRecurringJob
{
}
