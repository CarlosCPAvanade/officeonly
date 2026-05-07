import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { switchMap } from 'rxjs';
import { DocumentDetail } from '../../shared/models/document.models';
import { DocumentsService } from './documents.service';

@Component({
  selector: 'app-document-detail',
  standalone: false,
  templateUrl: './document-detail.component.html',
  styleUrls: ['./document-detail.component.css']
})
export class DocumentDetailComponent implements OnInit {
  document: DocumentDetail | null = null;
  loading = true;
  errorMessage = '';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly documentsService: DocumentsService
  ) {}

  ngOnInit(): void {
    this.route.paramMap
      .pipe(switchMap((params) => this.documentsService.getDocument(params.get('id') ?? '')))
      .subscribe({
        next: (document) => {
          this.document = document;
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error?.error?.error ?? 'No fue posible cargar el detalle del documento.';
          this.loading = false;
        }
      });
  }

  openEditor(): void {
    if (!this.document) {
      return;
    }

    this.router.navigate(['/editor', this.document.id]);
  }
}
