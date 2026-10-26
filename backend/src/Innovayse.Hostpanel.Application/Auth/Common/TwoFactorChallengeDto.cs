namespace Innovayse.Hostpanel.Application.Auth.Common;

/// <summary>
/// A verified, still-open second-factor challenge: what <c>ITwoFactorChallengeService.TryRead</c>
/// hands back once a pending token has proved to be ours, unexpired and unspent.
/// </summary>
/// <param name="Id">The unique id of this challenge, the key its attempts and its single use are counted under.</param>
/// <param name="UserId">The account whose password step opened the challenge.</param>
public sealed record TwoFactorChallengeDto(string Id, string UserId);
