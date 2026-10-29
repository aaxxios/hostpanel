namespace Innovayse.Hostpanel.Application.Reports.Common.Enums;

/// <summary>How long an unpaid invoice has been past its due date, in display order.</summary>
public enum AgingBucket
{
    /// <summary>Up to thirty days past due.</summary>
    Days0To30,

    /// <summary>Thirty-one to sixty days past due.</summary>
    Days30To60,

    /// <summary>Sixty-one to ninety days past due.</summary>
    Days60To90,

    /// <summary>Ninety-one to one hundred twenty days past due.</summary>
    Days90To120,

    /// <summary>More than one hundred twenty days past due.</summary>
    Over120,
}
