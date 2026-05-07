import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { DocumentDetailComponent } from './document-detail.component';
import { DocumentListComponent } from './document-list.component';
import { DocumentUploadComponent } from './document-upload.component';
import { DocumentsRoutingModule } from './documents-routing.module';

@NgModule({
  declarations: [DocumentListComponent, DocumentDetailComponent, DocumentUploadComponent],
  imports: [SharedModule, DocumentsRoutingModule]
})
export class DocumentsModule {}
