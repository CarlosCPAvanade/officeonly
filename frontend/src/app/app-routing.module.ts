import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/auth/auth.guard';

const routes: Routes = [
  {
    path: 'login',
    loadChildren: () => import('./features/login/login.module').then((m) => m.LoginModule)
  },
  {
    path: 'documents',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/documents/documents.module').then((m) => m.DocumentsModule)
  },
  {
    path: 'editor',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/editor/editor.module').then((m) => m.EditorModule)
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'documents'
  },
  {
    path: '**',
    redirectTo: 'documents'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
