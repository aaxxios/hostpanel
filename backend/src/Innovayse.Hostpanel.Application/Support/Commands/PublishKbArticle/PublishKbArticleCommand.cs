namespace Innovayse.Hostpanel.Application.Support.Commands.PublishKbArticle;

/// <summary>Command to publish or unpublish a knowledge base article.</summary>
/// <param name="Id">The article primary key; nullable so the route id can overlay the body.</param>
/// <param name="Publish"><see langword="true"/> to publish; <see langword="false"/> to unpublish.</param>
public sealed record PublishKbArticleCommand(int? Id, bool Publish);
