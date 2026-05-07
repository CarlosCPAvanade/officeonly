import { Component, EventEmitter, Output } from '@angular/core';
import { DocumentsService } from './documents.service';

@Component({
  selector: 'app-document-upload',
  standalone: false,
  templateUrl: './document-upload.component.html',
  styleUrls: ['./document-upload.component.css']
})
export class DocumentUploadComponent {
  @Output() uploaded = new EventEmitter<void>();

  selectedFile: File | null = null;
  uploading = false;
  message = '';
  errorMessage = '';

  constructor(private readonly documentsService: DocumentsService) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
    this.message = '';
    this.errorMessage = '';
  }

  upload(): void {
    if (!this.selectedFile) {
      this.errorMessage = 'Seleccione un archivo DOCX, XLSX o PPTX.';
      return;
    }

    this.uploading = true;
    this.documentsService.upload(this.selectedFile).subscribe({
      next: () => {
        this.uploading = false;
        this.selectedFile = null;
        this.message = 'Documento cargado correctamente.';
        this.errorMessage = '';
        this.uploaded.emit();
      },
      error: (error) => {
        this.uploading = false;
        this.errorMessage = error?.error?.error ?? 'No fue posible subir el archivo.';
      }
    });
  }
}
