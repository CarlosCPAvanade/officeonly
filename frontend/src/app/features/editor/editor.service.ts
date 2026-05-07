import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OnlyOfficeConfig } from '../../shared/models/document.models';

@Injectable({ providedIn: 'root' })
export class EditorService {
  constructor(private readonly http: HttpClient) {}

  getConfig(documentId: string): Observable<OnlyOfficeConfig> {
    return this.http.get<OnlyOfficeConfig>(`${environment.apiBaseUrl}/api/documents/${documentId}/config`);
  }
}
