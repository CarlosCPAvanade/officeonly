using Application.DTOs.OnlyOffice;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/onlyoffice")]
public class OnlyOfficeController : ControllerBase
{
    private readonly IOnlyOfficeService _onlyOfficeService;

    public OnlyOfficeController(IOnlyOfficeService onlyOfficeService)
    {
        _onlyOfficeService = onlyOfficeService;
    }

    [HttpPost("callback/{id:guid}")]
    public async Task<IActionResult> Callback(Guid id, [FromBody] OnlyOfficeCallbackDto request, CancellationToken cancellationToken)
    {
        var result = await _onlyOfficeService.ProcessCallbackAsync(id, request, Request.Headers.Authorization.ToString(), cancellationToken);
        return Ok(result);
    }
}
