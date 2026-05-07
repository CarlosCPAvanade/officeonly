import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DocumentDetailComponent } from './document-detail.component';
import { DocumentListComponent } from './document-list.component';

const routes: Routes = [
  {
    path: '',
    component: DocumentListComponent
  },
  {
    path: ':id',
    component: DocumentDetailComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DocumentsRoutingModule {}
