import { Injectable } from '@angular/core';

export type EditorMode = 'manual' | 'angular';

@Injectable({ providedIn: 'root' })
export class EditorModeService {
  private readonly storageKey = 'oo-editor-mode';
  private readonly defaultMode: EditorMode = 'manual';

  getMode(): EditorMode {
    const mode = localStorage.getItem(this.storageKey);
    return mode === 'angular' || mode === 'manual' ? mode : this.defaultMode;
  }

  setMode(mode: EditorMode): void {
    localStorage.setItem(this.storageKey, mode);
  }

  getModeLabel(mode: EditorMode = this.getMode()): string {
    return mode === 'angular'
      ? 'Wrapper oficial @onlyoffice/document-editor-angular'
      : 'Integración manual con api.js';
  }

  buildEditorRoute(documentId: string, mode: EditorMode = this.getMode()): string[] {
    return mode === 'angular' ? ['/editor', 'angular', documentId] : ['/editor', 'manual', documentId];
  }
}