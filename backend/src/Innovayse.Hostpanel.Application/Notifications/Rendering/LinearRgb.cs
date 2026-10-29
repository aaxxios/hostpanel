namespace Innovayse.Hostpanel.Application.Notifications.Rendering;

/// <summary>A colour as linear-light red, green and blue channels, possibly outside 0 to 1.</summary>
/// <param name="R">Linear red.</param>
/// <param name="G">Linear green.</param>
/// <param name="B">Linear blue.</param>
internal sealed record LinearRgb(double R, double G, double B);
