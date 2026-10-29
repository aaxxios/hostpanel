namespace Innovayse.Hostpanel.Application.Notifications.Rendering;

/// <summary>A colour in OKLab: perceptual lightness and the two opponent-colour axes.</summary>
/// <param name="L">Perceptual lightness.</param>
/// <param name="A">Green-red axis.</param>
/// <param name="B">Blue-yellow axis.</param>
internal sealed record OklabColor(double L, double A, double B);
