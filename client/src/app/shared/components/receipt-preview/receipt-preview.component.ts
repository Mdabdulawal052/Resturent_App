import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-receipt-preview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './receipt-preview.component.html',
  styleUrl: './receipt-preview.component.css'
})
export class ReceiptPreviewComponent {
  @Input() order: any;
  @Output() close = new EventEmitter<void>();

  receiptLanguage = signal<'en' | 'bn'>('bn');

  print() {
    window.print();
  }
}
