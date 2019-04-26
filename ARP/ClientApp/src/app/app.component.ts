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

import { Component, ElementRef, NgZone, OnInit, Renderer, ViewChild, ViewEncapsulation } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { NavigationEnd, Router } from '@angular/router';
import { Category, VdlDialog, VdlDialogRef, VdlIconRegistry, VdlOption } from 'vdl-angular';
import { AuthService } from './auth.service';
import { TokenComponent } from './token/token.component';

/*
 * App Component
 * Top Level Component
 */
@Component({
  selector: 'app-root',
  encapsulation: ViewEncapsulation.None,
  styleUrls: ['./app.component.scss'],
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  public selectedCategory: Category | null;
  public currentState = '';
  public tdStates: any[];
  public inputTypeahead: string;
  public inputValue: any;
  public searchHidden: boolean = true;

  @ViewChild('input') public inputElement: ElementRef;

  public categories: Category[] = [
    { displayName: 'Home', route: 'home', icon: 'fa-home' },
    { displayName: 'Screens', route: 'screenshot', icon: 'fa-file-image-o' },
    {
      displayName: 'About', icon: 'fa-info-circle', expanded: true,
      subCategories: [
        { displayName: 'Landing Page', route: 'about/landing', icon: 'fa-info-circle' },
        { displayName: 'Debug messages', route: 'messages', icon: 'fa-info-circle' },
      ]
    },
  ];

  public dialogRef: VdlDialogRef<TokenComponent>;
  public lastCloseResult: string;

  constructor(vdlIconRegistry: VdlIconRegistry,
    public authService: AuthService,
    private _sanitizer: DomSanitizer,
    private _renderer: Renderer,
    private _router: Router,
    private _ngZone: NgZone,
    public dialog: VdlDialog) {
    vdlIconRegistry.registerFontClassAlias('fontawesome', 'fa');
    vdlIconRegistry.addSvgIcon('simple-globe', _sanitizer.bypassSecurityTrustResourceUrl('/assets/img/simple-globe.svg'));
  }

  get menuStatus(): boolean {
    return this.authService.showMenu;
  }

  public ngOnInit() {
    this.selectedCategory = this.categories[0];
    console.log('App has initialized!');
    this.authService.checkAuthentication();

    this._router.events.subscribe(
      (event) => {
        if (event instanceof NavigationEnd) {
          this.selectedCategory = this.findCategoryByRoute(this.categories, event.urlAfterRedirects);
          this.authService.checkAuthentication();
        }
      }
    );
  }

  public OnLogout() {
    this.authService.Logout();
  }

  public onCategorySelect(category: Category) {
    this._router.navigate([category.route]);
  }

  public alert(value: string) {
    alert(value);
  }

  public searchClick() {
    // Give Angular a chance to create the input container and put focus on the input when shown
    this._ngZone.runOutsideAngular(() => {
      setTimeout(() => this._renderer.invokeElementMethod(this.inputElement.nativeElement, 'focus', []));
    });

    this.searchHidden = !this.searchHidden;
  }

  public handleOptionEvent(option: VdlOption) {
    if (this.inputElement) {
      this.inputValue = this.inputElement.nativeElement.value;
    }

    if (option && option.value) {
      let matchFront = new RegExp('^(?:' + this.inputElement.nativeElement.value + ')(.+$)', 'i');
      let match = matchFront.exec(option.value);

      if (match) {
        this.inputTypeahead = match[1];
      } else {
        this.inputTypeahead = '';
      }
    } else {
      this.inputTypeahead = '';
    }
  }

  private findCategoryByRoute(categories: Category[], route: string): Category | null {
    const trimmedRoute: string = route.startsWith('/') ? route.substr(1) : route; //  trimStart(route, '/');

    function reducer(pv: Category, cv: Category): Category {
      if (cv.subCategories && cv.subCategories.length > 0) {
        return cv.subCategories.reduce(reducer, pv);
      }
      return cv.route === trimmedRoute ? cv : pv;
    }

    return categories.reduce(reducer);

  }


  public openSupportPage() {
    window.open('https://github.com/VeritasOS/assisted-review-portal');
  }

  public GetToken() {
    this.dialogRef = this.dialog.open(TokenComponent);

    this.dialogRef.afterClosed().subscribe((result: string) => {
      this.lastCloseResult = result;
      this.dialogRef = null;
    });
  }
}
