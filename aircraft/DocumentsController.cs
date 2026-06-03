using FormaOnly.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FormaOnly.API.Controllers;

[ApiController]
[Route("api/v1/documents")]
public sealed class DocumentsController : ControllerBase
{
    private readonly DocumentCatalogService _catalogService;
    private static readonly AircraftSampleDefinition[] AircraftSamples =
    [
        new("Airbus A320", "airbus-a320-manual.docx", "docx", documentId => SampleOfficeDocumentFactory.CreateDocx(
            "Manual Airbus A320",
            $"Documento asociado al registro {documentId}.",
            "Revision previa al vuelo.",
            "Comprobar tren de aterrizaje, flaps y combustible.")),
        new("Boeing 737", "boeing-737-checklist.xlsx", "xlsx", _ => SampleOfficeDocumentFactory.CreateXlsx(
            "Checklist 737",
            new (string Aircraft, string Value)[]
            {
                ("Boeing 737", "Motores OK"),
                ("Boeing 737", "Hidraulica OK"),
                ("Boeing 737", "Puertas OK")
            })),
        new("Cessna 172", "cessna-172-parte.docx", "docx", documentId => SampleOfficeDocumentFactory.CreateDocx(
            "Parte Cessna 172",
            $"Documento asociado al registro {documentId}.",
            "Anotar observaciones de mantenimiento.",
            "Confirmar estado de helice e instrumentos."))
    ];

    public DocumentsController(DocumentCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    // Lista el catalogo y opcionalmente incluye la papelera.
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<DocumentSummary>> GetDocuments([FromQuery] bool includeDeleted = false)
    {
        return Ok(_catalogService.GetDocuments(includeDeleted));
    }

    [HttpGet("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<DocumentDetails> GetDocument(Guid documentId)
    {
        var document = _catalogService.GetDocument(documentId);
        return document is null ? NotFound() : Ok(document);
    }

    // Crea un documento nuevo a partir de demo.docx.
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public ActionResult<DocumentDetails> CreateDocument([FromBody] CreateDocumentRequest? request)
    {
        var document = _catalogService.CreateDocument(request?.Name, request?.User);
        return CreatedAtAction(nameof(GetDocument), new { documentId = document.Id }, document);
    }

    // Implementa Guardar como.
    [HttpPost("{documentId:guid}/duplicate")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<DocumentDetails> DuplicateDocument(Guid documentId, [FromBody] DuplicateDocumentRequest? request)
    {
        var document = _catalogService.DuplicateDocument(documentId, request?.Name, request?.User);
        return document is null
            ? NotFound()
            : CreatedAtAction(nameof(GetDocument), new { documentId = document.Id }, document);
    }

    // Sube un binario real y lo convierte en documento nuevo.
    [HttpPost("upload")]
    [RequestSizeLimit(100_000_000)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentDetails>> UploadDocument([FromForm] UploadDocumentRequest request)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest("Debe adjuntar un archivo.");
        }

        await using var stream = request.File.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        var document = _catalogService.CreateDocumentFromUpload(request.File.FileName, memoryStream.ToArray(), request.User);
        return CreatedAtAction(nameof(GetDocument), new { documentId = document.Id }, document);
    }

    [HttpDelete("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteDocument(Guid documentId, [FromQuery] string? user = null)
    {
        return _catalogService.DeleteDocument(documentId, user) ? NoContent() : NotFound();
    }

    [HttpPost("{documentId:guid}/restore")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RestoreDocument(Guid documentId, [FromBody] RestoreDocumentRequest? request)
    {
        return _catalogService.RestoreDocument(documentId, request?.User) ? NoContent() : NotFound();
    }

    [HttpGet("{documentId:guid}/versions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<DocumentVersionDetails>> GetVersions(Guid documentId)
    {
        return Ok(_catalogService.GetVersions(documentId));
    }

    // Sube un nuevo binario como version adicional del documento actual.
    [HttpPost("{documentId:guid}/versions/upload")]
    [RequestSizeLimit(100_000_000)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentVersionDetails>> UploadVersion(Guid documentId, [FromForm] UploadDocumentRequest request)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest("Debe adjuntar un archivo.");
        }

        await using var stream = request.File.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        var version = _catalogService.AddVersionFromUpload(documentId, request.File.FileName, memoryStream.ToArray(), request.User);
        return Ok(version);
    }

    [HttpPost("{documentId:guid}/versions/{versionNumber:int}/restore")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<DocumentVersionDetails> RestoreVersion(Guid documentId, int versionNumber, [FromBody] RestoreDocumentRequest? request)
    {
        var version = _catalogService.RestoreVersion(documentId, versionNumber, request?.User);
        return version is null ? NotFound() : Ok(version);
    }

    [HttpGet("{documentId:guid}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<DocumentHistoryEntry>> GetHistory(Guid documentId)
    {
        return Ok(_catalogService.GetHistory(documentId));
    }

    [HttpGet("aircraft-samples")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<AircraftDocumentSummary>> GetAircraftSamples()
    {
        var existingDocuments = _catalogService.GetDocuments(includeDeleted: true)
            .GroupBy(document => document.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(document => document.UpdatedAtUtc).First(),
                StringComparer.OrdinalIgnoreCase);

        var results = new List<AircraftDocumentSummary>(AircraftSamples.Length);

        foreach (var sample in AircraftSamples)
        {
            if (!existingDocuments.TryGetValue(sample.FileName, out var documentSummary))
            {
                var fileBytes = sample.BuildContent(Guid.NewGuid());
                var document = _catalogService.CreateDocumentFromUpload(sample.FileName, fileBytes, "aircraft-samples");
                documentSummary = new DocumentSummary(
                    document.Id,
                    document.Name,
                    document.CurrentVersionNumber,
                    document.IsDeleted,
                    document.CreatedAtUtc,
                    document.UpdatedAtUtc);
                existingDocuments[document.Name] = documentSummary;
            }

            results.Add(new AircraftDocumentSummary(
                sample.AircraftName,
                documentSummary.Id,
                documentSummary.Name,
                sample.FileType,
                sample.FileType == "xlsx" ? "cell" : "word"));
        }

        return Ok(results);
    }

    private sealed record AircraftSampleDefinition(
        string AircraftName,
        string FileName,
        string FileType,
        Func<Guid, byte[]> BuildContent);
}

public sealed class CreateDocumentRequest
{
    public string? Name { get; set; }
    public string? User { get; set; }
}

public sealed class RestoreDocumentRequest
{
    public string? User { get; set; }
}

public sealed class DuplicateDocumentRequest
{
    public string? Name { get; set; }
    public string? User { get; set; }
}

public sealed class UploadDocumentRequest
{
    public IFormFile? File { get; set; }
    public string? User { get; set; }
}