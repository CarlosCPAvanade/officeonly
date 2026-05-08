import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { IConfig } from '@onlyoffice/document-editor-angular';
import { switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OnlyOfficeConfig } from '../../shared/models/document.models';
import { EditorService } from './editor.service';

@Component({
  selector: 'app-editor-angular',
  standalone: false,
  templateUrl: './editor-angular.component.html',
  styleUrls: ['./editor-angular.component.css']
})
export class EditorAngularComponent implements OnInit {
  loading = true;
  errorMessage = '';
  config: IConfig | null = null;
  readonly documentServerUrl = environment.onlyOfficeUrl;
  readonly editorElementId = 'onlyoffice-angular-editor-host';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly editorService: EditorService
  ) {}

  ngOnInit(): void {
    this.route.paramMap
      .pipe(switchMap((params) => this.editorService.getConfig(params.get('id') ?? '')))
      .subscribe({
        next: (config) => {
          this.config = this.mapConfig(config);
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error?.error?.error ?? 'No fue posible cargar la configuración del editor wrapper.';
          this.loading = false;
        }
      });
  }

  readonly onDocumentReady = (): void => {
    this.loading = false;
  };

  readonly onLoadComponentError = (_errorCode: number, errorDescription: string): void => {
    this.errorMessage = errorDescription || 'No fue posible cargar el wrapper oficial de ONLYOFFICE.';
    this.loading = false;
  };

  private mapConfig(config: OnlyOfficeConfig): IConfig {
    return {
      document: config.document as IConfig['document'],
      documentType: config.documentType as IConfig['documentType'],
      editorConfig: config.editorConfig as IConfig['editorConfig'],
      token: config.token,
      type: config.type,
      width: '100%',
      height: '100%'
    };
  }
}