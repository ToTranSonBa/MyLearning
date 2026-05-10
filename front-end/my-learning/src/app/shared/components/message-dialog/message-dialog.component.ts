import { Component, Inject } from '@angular/core';

import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';

import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-message-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule],
  templateUrl: './message-dialog.component.html',

  styleUrl: './message-dialog.component.scss',
})
export class MessageDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA)
    public data: {
      title: string;
      message: string;
    },
  ) {}
}
