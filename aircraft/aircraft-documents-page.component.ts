import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { type IConfig } from '@onlyoffice/document-editor-angular';

@Component({
  selector: 'app-aircraft-documents-page',
  templateUrl: './aircraft-documents-page.component.html',
  styleUrl: './aircraft-documents-page.component.scss',
  standalone: false
})
export class AircraftDocumentsPageComponent implements OnInit {
  protected readonly apiBaseUrl = 'http://localhost:5033';
  protected readonly documentServerUrl = 'http://localhost:8083';

  protected aircraftRows: AircraftDocumentRow[] = [];
  protected selectedRow: AircraftDocumentRow | null = null;
  protected editorConfig: IConfig | null = null;
  protected isGridLoading = true;
  protected isEditorLoading = false;
  protected isSaving = false;
  protected statusMessage = '';
  protected errorMessage = '';
  protected editorReady = false;

  constructor(private readonly http: HttpClient) {}

  ngOnInit(): void {
    this.loadAircraftRows();
  }

  protected openDocument(row: AircraftDocumentRow): void {
    this.selectedRow = row;
    this.editorConfig = null;
    this.errorMessage = '';
    this.statusMessage = `Abriendo ${row.fileName}...`;
    this.editorReady = false;
    this.isEditorLoading = true;

    this.http
      .get<IConfig>(`${this.apiBaseUrl}/api/v1/onlyoffice/config/${row.documentId}`)
      .subscribe({
        next: (config) => {
          if (this.selectedRow?.documentId !== row.documentId) {
            return;
          }

          this.editorConfig = this.decorateConfig(config);
          this.isEditorLoading = false;
          this.statusMessage = `${row.aircraftName} listo en ONLYOFFICE.`;
        },
        error: () => {
          if (this.selectedRow?.documentId !== row.documentId) {
            return;
          }

          this.isEditorLoading = false;
          this.errorMessage = 'No se pudo obtener la configuracion de ONLYOFFICE para el documento seleccionado.';
          this.statusMessage = '';
        }
      });
  }

  protected saveSelectedDocument(): void {
    if (!this.selectedRow) {
      return;
    }

    const editor = this.getEditorInstance();
    if (!editor) {
      this.errorMessage = 'El editor todavia no esta listo para guardar.';
      return;
    }

    this.errorMessage = '';
    this.isSaving = true;
    this.statusMessage = `Solicitando guardado de ${this.selectedRow.fileName}...`;
    editor.serviceCommand('forcesave', true);
  }

  protected trackByDocumentId(_: number, row: AircraftDocumentRow): string {
    return row.documentId;
  }

  protected readonly onDocumentReady = (): void => {
    this.editorReady = true;
    this.errorMessage = '';
  };

  protected readonly onDocumentStateChange = (event: { data?: boolean }): void => {
    if (event.data === false) {
      this.isSaving = false;
      this.statusMessage = this.selectedRow
        ? `${this.selectedRow.fileName} se ha sincronizado con el backend.`
        : 'Los cambios ya se han sincronizado con el backend.';
    }
  };

  protected readonly onLoadComponentError = (errorCode: number, errorDescription: string): void => {
    this.errorMessage = `No se pudo cargar el componente de ONLYOFFICE (${errorCode}): ${errorDescription}`;
    this.isEditorLoading = false;
  };

  protected readonly onEditorError = (event: { data?: { errorDescription?: string } }): void => {
    this.errorMessage = event.data?.errorDescription ?? 'ONLYOFFICE devolvio un error en tiempo de ejecucion.';
    this.isSaving = false;
  };

  private loadAircraftRows(): void {
    this.isGridLoading = true;
    this.errorMessage = '';

    this.http
      .get<AircraftDocumentRow[]>(`${this.apiBaseUrl}/api/v1/documents/aircraft-samples`)
      .subscribe({
        next: (rows) => {
          this.aircraftRows = rows;
          this.isGridLoading = false;

          if (rows.length > 0) {
            this.openDocument(rows[0]);
          }
        },
        error: () => {
          this.isGridLoading = false;
          this.errorMessage = 'No se pudo cargar la lista de aeronaves.';
        }
      });
  }

  private decorateConfig(config: IConfig): IConfig {
    return {
      ...config,
      editorConfig: {
        ...config.editorConfig,
        mode: 'edit',
        customization: {
          ...config.editorConfig?.customization,
          autosave: true,
          forcesave: true
        }
      }
    };
  }

  private getEditorInstance(): OnlyOfficeEditorInstance | null {
    const editorId = `aircraftEditor-${this.selectedRow?.documentId ?? 'none'}`;
    return (globalThis as { DocEditor?: { instances?: Record<string, OnlyOfficeEditorInstance | undefined> } }).DocEditor?.instances?.[editorId] ?? null;
  }
}

interface AircraftDocumentRow {
  aircraftName: string;
  documentId: string;
  fileName: string;
  fileType: string;
  documentType: 'word' | 'cell' | 'slide';
}

interface OnlyOfficeEditorInstance {
  serviceCommand(command: string, data?: unknown): void;
}