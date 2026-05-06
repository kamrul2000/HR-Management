import {
  Directive,
  ElementRef,
  EventEmitter,
  HostListener,
  Output,
  inject,
} from '@angular/core';

/**
 * Emits when a click happens outside the host element.
 * Use it for closing dropdowns, popovers, etc.
 */
@Directive({
  selector: '[hrmClickOutside]',
  standalone: true,
})
export class ClickOutsideDirective {
  private readonly elementRef = inject(ElementRef<HTMLElement>);

  @Output('hrmClickOutside') outsideClick = new EventEmitter<MouseEvent>();

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as Node | null;
    if (target && !this.elementRef.nativeElement.contains(target)) {
      this.outsideClick.emit(event);
    }
  }
}
