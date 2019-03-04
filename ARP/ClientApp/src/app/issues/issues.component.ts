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

import { Component, Inject, Input, OnInit } from '@angular/core';
import { GarbService, Issue, IssueSeverity, IssueStatus, IssueType } from '../garb.service';
import { MessageService } from '../message.service';

@Component({
  selector: 'app-issues',
  templateUrl: './issues.component.html',
  styleUrls: ['./issues.component.css']
})
export class IssuesComponent implements OnInit {

  public issues: Issue[];
  public noIssues: boolean = true;

  private _projectName: string;
  private _screenName: string;
  private _locale: string;

  public IssueType = IssueType;
  public IssueSeverity = IssueSeverity;
  public IssueStatus = IssueStatus;

  @Input()
  set projectName(projectName: string) {
    if (this._projectName !== projectName) {
      this._projectName = projectName;
      this.getIssues();
    }
  }

  @Input()
  set screenName(screenName: string) {
    if (this._screenName !== screenName) {
      this._screenName = screenName;
      this.getIssues();
    }
  }

  @Input()
  set locale(locale: string) {
    if (this._locale !== locale) {
      this._locale = locale;
      this.getIssues();
    }
  }


  constructor(private messageService: MessageService,
    @Inject('API_URL') private originUrl: string,
    private garbService: GarbService) {
  }

  public getIssues() {
    if (this._projectName == null || this._screenName == null || this._locale == null) {
      this.issues = null;
      this.noIssues = true;
      this.log("no issues");
    }
    else {
      this.garbService.getIssues(this._projectName, this._screenName, this._locale).subscribe(result => {
        this.issues = result as Issue[];
        this.noIssues = false;
        this.log("issues loaded");
      });
    }
  }

  ngOnInit() {
  }

  /** Log a IssuesComponent message with the MessageService */
  private log(message: string) {
    this.messageService.add('IssuesComponent: ' + message);
  }

  mouseEnter(div: string) {
    this.log("mouse enter : " + div);
  }

  mouseLeave(div: string) {
    this.log('mouse leave :' + div);
  }
}
