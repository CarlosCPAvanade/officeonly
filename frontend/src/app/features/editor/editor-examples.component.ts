import { Component } from '@angular/core';

@Component({
  selector: 'app-editor-examples',
  standalone: false,
  templateUrl: './editor-examples.component.html',
  styleUrls: ['./editor-examples.component.css']
})
export class EditorExamplesComponent {
  readonly angularTemplateExample = `<document-editor
  [id]="'onlyoffice-angular-editor-host'"
  [documentServerUrl]="documentServerUrl"
  [config]="config"
  [width]="'100%'"
  [height]="'100%'"
  [events_onDocumentReady]="onDocumentReady"
  [onLoadComponentError]="onLoadComponentError"
></document-editor>`;

  readonly angularComponentExample = `import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { IConfig } from '@onlyoffice/document-editor-angular';
import { switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EditorService } from './editor.service';

@Component({
  selector: 'app-editor-angular',
  standalone: false,
  templateUrl: './editor-angular.component.html'
})
export class EditorAngularComponent implements OnInit {
  config: IConfig | null = null;
  readonly documentServerUrl = environment.onlyOfficeUrl;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly editorService: EditorService
  ) {}

  ngOnInit(): void {
    this.route.paramMap
      .pipe(switchMap((params) => this.editorService.getConfig(params.get('id') ?? '')))
      .subscribe((config) => {
        this.config = {
          document: config.document as IConfig['document'],
          documentType: config.documentType as IConfig['documentType'],
          editorConfig: config.editorConfig as IConfig['editorConfig'],
          token: config.token,
          type: config.type,
          width: '100%',
          height: '100%'
        };
      });
  }
}`;

  readonly backendConfigExample = `public async Task<OnlyOfficeEditorConfigDto> BuildEditorConfigAsync(Guid documentId, Guid userId)
{
    var downloadToken = _jwtTokenService.GenerateDownloadToken(document.Id, user.Id, expiresAt);
    var documentUrl = $"{_onlyOfficeOptions.InternalApiBaseUrl}/api/documents/{document.Id}/download?accessToken={downloadToken}";
    var callbackUrl = $"{_onlyOfficeOptions.InternalApiBaseUrl}/api/onlyoffice/callback/{document.Id}";

    var payload = new
    {
        document = new
        {
            fileType = fileExtension,
            key = $"{document.Id:N}-v{document.CurrentVersionNumber}",
            title = document.OriginalFileName,
            url = documentUrl,
            permissions = new { edit = permission.CanEdit, download = true }
        },
        documentType = ResolveDocumentType(document.OriginalFileName),
        editorConfig = new
        {
            callbackUrl,
            mode = permission.CanEdit ? "edit" : "view",
            lang = "es",
            user = new { id = user.Id.ToString(), name = user.UserName }
        }
    };

    return new OnlyOfficeEditorConfigDto
    {
        DocumentType = ResolveDocumentType(document.OriginalFileName),
        Type = "desktop",
        Document = payload.document,
        EditorConfig = payload.editorConfig,
        Token = _jwtTokenService.GenerateOnlyOfficeToken(payload)
    };
}`;

  readonly callbackExample = `public async Task<object> ProcessCallbackAsync(Guid documentId, OnlyOfficeCallbackDto request)
{
    ValidateCallbackToken(request, null);

    if (request.Status != 2 && request.Status != 6)
    {
        return new { error = 0 };
    }

    using var response = await client.SendAsync(
        new HttpRequestMessage(HttpMethod.Get, ResolveCallbackDownloadUrl(request.Url))
    );
    response.EnsureSuccessStatusCode();

    await using var responseStream = await response.Content.ReadAsStreamAsync();
    await _fileStorageService.ReplaceAsync(document.CurrentFilePath, responseStream);

    return new { error = 0 };
}`;
}