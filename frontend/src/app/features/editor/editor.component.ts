import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OnlyOfficeConfig } from '../../shared/models/document.models';
import { EditorService } from './editor.service';

type DocsApiWindow = Window & {
  DocsAPI?: {
    DocEditor: new (elementId: string, config: Record<string, unknown>) => { destroyEditor: () => void };
  };
};

@Component({
  selector: 'app-editor',
  standalone: false,
  templateUrl: './editor.component.html',
  styleUrls: ['./editor.component.css']
})
export class EditorComponent implements OnInit, OnDestroy {
  @ViewChild('editorHost', { static: true }) editorHost!: ElementRef<HTMLDivElement>;

  loading = true;
  errorMessage = '';
  private editorInstance: { destroyEditor: () => void } | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly editorService: EditorService
  ) {}

  ngOnInit(): void {
    this.route.paramMap
      .pipe(switchMap((params) => this.editorService.getConfig(params.get('id') ?? '')))
      .subscribe({
        next: async (config) => {
          await this.ensureScriptLoaded();
          this.mountEditor(config);
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error?.error?.error ?? 'No fue posible cargar la configuración del editor.';
          this.loading = false;
        }
      });
  }

  ngOnDestroy(): void {
    this.editorInstance?.destroyEditor();
  }

  private async ensureScriptLoaded(): Promise<void> {
    if ((window as DocsApiWindow).DocsAPI) {
      return;
    }

    await new Promise<void>((resolve, reject) => {
      const script = document.createElement('script');
      script.src = `${environment.onlyOfficeUrl}/web-apps/apps/api/documents/api.js`;
      script.async = true;
      script.onload = () => resolve();
      script.onerror = () => reject(new Error('No fue posible cargar api.js de ONLYOFFICE.'));
      document.body.appendChild(script);
    });
  }

  private mountEditor(config: OnlyOfficeConfig): void {
    const docsApi = (window as DocsApiWindow).DocsAPI;

    if (!docsApi) {
      this.errorMessage = 'La API global de ONLYOFFICE no está disponible.';
      return;
    }

    this.editorInstance?.destroyEditor();
    this.editorHost.nativeElement.id = 'onlyoffice-editor-host';
    this.editorInstance = new docsApi.DocEditor('onlyoffice-editor-host', {
      documentType: config.documentType,
      type: config.type,
      document: config.document,
      editorConfig: config.editorConfig,
      token: config.token,
      height: '100%',
      width: '100%'
    });
  }
}
