using Api.Extensions;
using Application.DTOs.Documents;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IOnlyOfficeService _onlyOfficeService;

    public DocumentsController(IDocumentService documentService, IOnlyOfficeService onlyOfficeService)
    {
        _documentService = documentService;
        _onlyOfficeService = onlyOfficeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var role = User.GetRequiredRole();
        var documents = await _documentService.GetDocumentsAsync(userId, role, cancellationToken);
        return Ok(documents);
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(UploadDocumentResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var role = User.GetRequiredRole();
        var result = await _documentService.UploadAsync(file, userId, role, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var role = User.GetRequiredRole();
        var result = await _documentService.GetDocumentAsync(id, userId, role, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var role = User.GetRequiredRole();
        await _documentService.DeleteAsync(id, userId, role, cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, [FromQuery] string? accessToken, CancellationToken cancellationToken)
    {
        Guid? userId = null;
        string? role = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = User.GetRequiredUserId();
            role = User.GetRequiredRole();
        }

        var result = await _documentService.DownloadAsync(id, userId, role, accessToken, cancellationToken);
        return File(result.Stream, result.ContentType, result.FileName);
    }

    [HttpGet("{id:guid}/config")]
    public async Task<IActionResult> GetOnlyOfficeConfig(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var role = User.GetRequiredRole();
        var config = await _onlyOfficeService.BuildEditorConfigAsync(id, userId, role, cancellationToken);
        return Ok(config);
    }

    [HttpGet("{id:guid}/versions")]
    public async Task<IActionResult> GetVersions(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var role = User.GetRequiredRole();
        var versions = await _documentService.GetVersionsAsync(id, userId, role, cancellationToken);
        return Ok(versions);
    }
}
