import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OnlyOfficeConfig } from '../../shared/models/document.models';
import { EditorService } from './editor.service';

declare global {
  interface Window {
    DocsAPI: {
      DocEditor: new (elementId: string, config: Record<string, unknown>) => { destroyEditor: () => void };
    };
  }
}

@Component({
  selector: 'app-onlyoffice-editor',
  standalone: false,
  templateUrl: './onlyoffice-editor.component.html',
  styleUrls: ['./onlyoffice-editor.component.css']
})
export class OnlyofficeEditorComponent implements OnInit, OnDestroy {
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
    if (window.DocsAPI) {
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
    this.editorInstance?.destroyEditor();
    this.editorHost.nativeElement.id = 'onlyoffice-editor-host';
    this.editorInstance = new window.DocsAPI.DocEditor('onlyoffice-editor-host', {
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
