import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroCloudArrowUp, heroXMark } from '@ng-icons/heroicons/outline';

@Component({
  selector: 'hrm-file-upload',
  standalone: true,
  imports: [CommonModule, NgIcon],
  providers: [provideIcons({ heroCloudArrowUp, heroXMark })],
  templateUrl: './file-upload.component.html',
  styleUrl: './file-upload.component.scss',
})
export class FileUploadComponent {
  @Input() accept = 'image/*,application/pdf';
  @Input() maxBytes = 5 * 1024 * 1024;
  @Input() label = 'Click to upload or drag and drop';
  @Input() hint = 'PDF, PNG, JPG up to 5 MB';
  @Input() disabled = false;

  @Output() fileSelected = new EventEmitter<File>();
  @Output() error = new EventEmitter<string>();

  readonly fileName = signal<string | null>(null);
  readonly fileSize = signal<number>(0);
  readonly isDragging = signal(false);

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    this.handleFile(input.files[0]);
    input.value = '';
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
    if (this.disabled) return;
    const file = event.dataTransfer?.files?.[0];
    if (file) this.handleFile(file);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (!this.disabled) this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
  }

  clear(): void {
    this.fileName.set(null);
    this.fileSize.set(0);
  }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
  }

  private handleFile(file: File): void {
    if (file.size > this.maxBytes) {
      const limit = this.formatSize(this.maxBytes);
      this.error.emit(`File exceeds the maximum size of ${limit}.`);
      return;
    }

    this.fileName.set(file.name);
    this.fileSize.set(file.size);
    this.fileSelected.emit(file);
  }
}
