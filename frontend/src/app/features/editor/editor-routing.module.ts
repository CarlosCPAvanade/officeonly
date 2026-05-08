import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EditorAngularComponent } from './editor-angular.component';
import { EditorExamplesComponent } from './editor-examples.component';
import { EditorComponent } from './editor.component';

const routes: Routes = [
  {
    path: 'examples',
    component: EditorExamplesComponent
  },
  {
    path: 'angular/:id',
    component: EditorAngularComponent
  },
  {
    path: 'manual/:id',
    component: EditorComponent
  },
  {
    path: ':id',
    component: EditorComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EditorRoutingModule {}
