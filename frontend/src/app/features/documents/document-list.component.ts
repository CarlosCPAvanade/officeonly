import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { EditorMode, EditorModeService } from '../../core/onlyoffice/editor-mode.service';
import { DocumentsService } from './documents.service';
import { DocumentItem } from '../../shared/models/document.models';

@Component({
  selector: 'app-document-list',
  standalone: false,
  templateUrl: './document-list.component.html',
  styleUrls: ['./document-list.component.css']
})
export class DocumentListComponent implements OnInit {
  documents: DocumentItem[] = [];
  loading = false;
  errorMessage = '';
  selectedEditorMode: EditorMode;

  constructor(
    public readonly documentsService: DocumentsService,
    private readonly router: Router,
    private readonly editorModeService: EditorModeService
  ) {
    this.selectedEditorMode = this.editorModeService.getMode();
  }

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.loading = true;
    this.errorMessage = '';
    this.documentsService.getDocuments().subscribe({
      next: (documents) => {
        this.documents = documents;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error?.error?.error ?? 'No fue posible cargar los documentos.';
        this.loading = false;
      }
    });
  }

  openDetail(documentId: string): void {
    this.router.navigate(['/documents', documentId]);
  }

  openEditor(documentId: string): void {
    this.router.navigate(this.editorModeService.buildEditorRoute(documentId, this.selectedEditorMode));
  }

  selectEditorMode(mode: EditorMode): void {
    this.selectedEditorMode = mode;
    this.editorModeService.setMode(mode);
  }

  getSelectedEditorLabel(): string {
    return this.editorModeService.getModeLabel(this.selectedEditorMode);
  }

  download(item: DocumentItem): void {
    this.documentsService.download(item.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = item.originalFileName;
        anchor.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        this.errorMessage = error?.error?.error ?? 'No fue posible descargar el documento.';
      }
    });
  }

  remove(documentId: string): void {
    if (!confirm('¿Desea eliminar el documento?')) {
      return;
    }

    this.documentsService.delete(documentId).subscribe({
      next: () => this.loadDocuments(),
      error: (error) => {
        this.errorMessage = error?.error?.error ?? 'No fue posible eliminar el documento.';
      }
    });
  }
}
