namespace Innovayse.Hostpanel.Application.Notifications.Rendering;

/// <summary>A colour as 8-bit sRGB channels.</summary>
/// <param name="R">Red, 0 to 255.</param>
/// <param name="G">Green, 0 to 255.</param>
/// <param name="B">Blue, 0 to 255.</param>
public sealed record RgbColor(int R, int G, int B);
