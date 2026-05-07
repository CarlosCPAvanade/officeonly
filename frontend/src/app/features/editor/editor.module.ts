import { NgModule } from '@angular/core';
import { SharedModule } from '../../shared/shared.module';
import { EditorComponent } from './editor.component';
import { EditorRoutingModule } from './editor-routing.module';

@NgModule({
  declarations: [EditorComponent],
  imports: [SharedModule, EditorRoutingModule]
})
export class EditorModule {}
