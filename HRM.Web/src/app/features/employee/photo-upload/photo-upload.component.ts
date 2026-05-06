import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  Input,
  Output,
  inject,
  signal,
} from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroCloudArrowUp, heroXMark } from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../core/services/toast.service';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { EmployeeResponse } from '../models/employee.model';
import { EmployeeService } from '../services/employee.service';

const MAX_BYTES = 1 * 1024 * 1024;
const ALLOWED = ['image/jpeg', 'image/png', 'image/webp'];

@Component({
  selector: 'hrm-photo-upload',
  standalone: true,
  imports: [CommonModule, NgIcon, AvatarComponent],
  providers: [provideIcons({ heroCloudArrowUp, heroXMark })],
  templateUrl: './photo-upload.component.html',
  styleUrl: './photo-upload.component.scss',
})
export class PhotoUploadComponent {
  private readonly service = inject(EmployeeService);
  private readonly toast = inject(ToastService);

  @Input({ required: true }) employee!: EmployeeResponse;
  @Output() uploaded = new EventEmitter<EmployeeResponse>();

  readonly preview = signal<string | null>(null);
  readonly uploading = signal(false);
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
    const file = event.dataTransfer?.files?.[0];
    if (file) this.handleFile(file);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(true);
  }

  onDragLeave(): void {
    this.isDragging.set(false);
  }

  clearPreview(): void {
    this.preview.set(null);
  }

  private handleFile(file: File): void {
    if (!ALLOWED.includes(file.type)) {
      this.toast.error('Only JPEG, PNG, or WebP images are allowed.');
      return;
    }
    if (file.size > MAX_BYTES) {
      this.toast.error('Image must be 1 MB or smaller.');
      return;
    }

    const reader = new FileReader();
    reader.onload = () => this.preview.set(reader.result as string);
    reader.readAsDataURL(file);

    this.upload(file);
  }

  private upload(file: File): void {
    this.uploading.set(true);
    this.service.uploadPhoto(this.employee.id, file).subscribe({
      next: (res) => {
        this.uploading.set(false);
        if (res.success && res.data) {
          this.toast.success('Photo uploaded.');
          this.uploaded.emit(res.data);
          this.preview.set(null);
        } else {
          this.toast.error(res.message || 'Failed to upload photo.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.uploading.set(false);
        this.toast.error(err.error?.message || 'Failed to upload photo.');
      },
    });
  }
}
