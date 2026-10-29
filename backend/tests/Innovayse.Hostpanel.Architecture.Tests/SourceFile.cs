namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>One C# source file as <see cref="SourceLayoutTests"/> reads it.</summary>
/// <param name="Path">The absolute path.</param>
/// <param name="Relative">The path relative to <c>backend/src</c>, with forward slashes, for messages.</param>
/// <param name="ExpectedNamespace">The namespace the file's project and folder imply.</param>
/// <param name="Text">The file's contents.</param>
internal sealed record SourceFile(string Path, string Relative, string ExpectedNamespace, string Text);
