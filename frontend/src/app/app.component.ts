import { Component } from '@angular/core';
import { AuthService } from './core/auth/auth.service';
import { EditorModeService } from './core/onlyoffice/editor-mode.service';

@Component({
  selector: 'app-root',
  standalone: false,
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  constructor(
    public authService: AuthService,
    public editorModeService: EditorModeService
  ) {}

  logout(): void {
    this.authService.logout();
  }
}
