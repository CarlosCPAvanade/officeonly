export interface DocumentItem {
  id: string;
  title: string;
  originalFileName: string;
  fileType: string;
  currentVersionNumber: number;
  sizeInBytes: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdBy: string;
  canRead: boolean;
  canEdit: boolean;
  canDelete: boolean;
}

export interface DocumentVersion {
  id: string;
  versionNumber: number;
  sizeInBytes: number;
  createdAtUtc: string;
  createdBy: string;
  changeSummary: string;
}

export interface DocumentDetail extends DocumentItem {
  mimeType: string;
  versions: DocumentVersion[];
}

export interface UploadResult {
  documentId: string;
  versionNumber: number;
  title: string;
}

export interface OnlyOfficeConfig {
  documentType: string;
  type: string;
  document: Record<string, unknown>;
  editorConfig: Record<string, unknown>;
  token: string;
}
