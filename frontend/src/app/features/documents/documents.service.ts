import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DocumentDetail, DocumentItem, DocumentVersion, UploadResult } from '../../shared/models/document.models';

@Injectable({ providedIn: 'root' })
export class DocumentsService {
  private readonly baseUrl = `${environment.apiBaseUrl}/api/documents`;

  constructor(private readonly http: HttpClient) {}

  getDocuments(): Observable<DocumentItem[]> {
    return this.http.get<DocumentItem[]>(this.baseUrl);
  }

  getDocument(id: string): Observable<DocumentDetail> {
    return this.http.get<DocumentDetail>(`${this.baseUrl}/${id}`);
  }

  upload(file: File): Observable<UploadResult> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<UploadResult>(`${this.baseUrl}/upload`, formData);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  download(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/download`, {
      responseType: 'blob'
    });
  }

  getVersions(id: string): Observable<DocumentVersion[]> {
    return this.http.get<DocumentVersion[]>(`${this.baseUrl}/${id}/versions`);
  }
}
