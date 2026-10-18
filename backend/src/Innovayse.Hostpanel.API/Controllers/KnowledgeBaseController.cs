using Innovayse.Hostpanel.Application.Support.Commands.CreateKbArticle;
using Innovayse.Hostpanel.Application.Support.Commands.CreateKbCategory;
using Innovayse.Hostpanel.Application.Support.Commands.DeleteKbArticle;
using Innovayse.Hostpanel.Application.Support.Commands.DeleteKbCategory;
using Innovayse.Hostpanel.Application.Support.Commands.PublishKbArticle;
using Innovayse.Hostpanel.Application.Support.Commands.UpdateKbArticle;
using Innovayse.Hostpanel.Application.Support.Commands.UpdateKbCategory;
using Innovayse.Hostpanel.Application.Support.Common;
using Innovayse.Hostpanel.Application.Support.Queries.GetKbArticle;
using Innovayse.Hostpanel.Application.Support.Queries.GetPublishedKbArticle;
using Innovayse.Hostpanel.Application.Support.Queries.ListKbArticles;
using Innovayse.Hostpanel.Application.Support.Queries.ListKbCategories;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Innovayse.Hostpanel.API.Controllers;

/// <summary>
/// The public, anonymous half of the knowledge base: published articles only.
/// </summary>
/// <remarks>
/// A separate controller rather than a relaxed authorization on <see cref="KnowledgeBaseAdminController"/>:
/// that class is admin surface end to end, and loosening its authorization for one read would put
/// a client-reachable action on the same type as every write.
/// </remarks>
[Route("api/knowledgebase")]
[AllowAnonymous]
public sealed class KnowledgeBaseController : BaseApiController
{
    /// <summary>Wolverine bus every action dispatches through.</summary>
    private readonly IMessageBus _bus;

    /// <summary>Creates the controller.</summary>
    /// <param name="bus">Wolverine bus every action dispatches through.</param>
    public KnowledgeBaseController(IMessageBus bus)
    {
        _bus = bus;
    }

    /// <summary>Returns all published knowledge base articles.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>200 with the published articles.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<KbArticleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPublishedAsync(CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<IReadOnlyList<KbArticleDto>>>(new ListKbArticlesQuery(true), ct);
        return HandleResult(outcome);
    }

    /// <summary>Returns a single published knowledge base article.</summary>
    /// <param name="id">Article primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 with the article; 400 when the id names no row or names an unpublished draft -- the
    /// two are answered alike so an anonymous caller cannot map the unpublished backlog by id.
    /// </returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(KbArticleDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublishedByIdAsync(int id, CancellationToken ct)
    {
        var outcome = await _bus.InvokeAsync<Result<KbArticleDto>>(new GetPublishedKbArticleQuery(id), ct);
        return HandleResult(outcome);
    }
}
