import { NgModule } from '@angular/core';
import { DocumentEditorModule } from '@onlyoffice/document-editor-angular';
import { SharedModule } from '../../shared/shared.module';
import { EditorAngularComponent } from './editor-angular.component';
import { EditorExamplesComponent } from './editor-examples.component';
import { EditorComponent } from './editor.component';
import { EditorRoutingModule } from './editor-routing.module';

@NgModule({
  declarations: [EditorComponent, EditorAngularComponent, EditorExamplesComponent],
  imports: [SharedModule, EditorRoutingModule, DocumentEditorModule]
})
export class EditorModule {}
