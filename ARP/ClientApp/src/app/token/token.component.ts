/*
Copyright (c) 2019 Veritas Technologies LLC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

import { Component, OnInit } from '@angular/core';
import { VdlDialogRef } from 'vdl-angular';

@Component({
  selector: 'app-token',
  template: `
  <vdl-dialog-header>
      <vdl-dialog-title>
         Your token
      </vdl-dialog-title>
  </vdl-dialog-header>
  <vdl-dialog-content>
      {{ token }}
  </vdl-dialog-content>
  <vdl-dialog-actions>
      <vdl-button-bar>
        <button vdl-tertiary-button vdl-dialog-close (click)="dialogRef.close('Dismiss')">Dismiss</button>
      </vdl-button-bar>
  </vdl-dialog-actions>
`
})
export class TokenComponent implements OnInit {

  public token: string;

  constructor(public dialogRef: VdlDialogRef<TokenComponent>) { }

  ngOnInit() {
    let tokenData = JSON.parse(localStorage.getItem("Authorization"));
    this.token = tokenData.token;
  }

}
