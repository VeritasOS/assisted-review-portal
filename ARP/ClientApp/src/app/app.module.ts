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

import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FlexLayoutModule } from "@angular/flex-layout";
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { PreloadAllModules, RouterModule } from '@angular/router';
import 'hammerjs';
import { ANIMATION_TYPES, LoadingModule } from 'ngx-loading';
import { environment } from '../environments/environment';
import './styles/global.scss';
import { AboutComponent } from './about';
// App is our top level component
import { AppComponent } from './app.component';
import { ROUTES } from './app.routes';
/*
 * Platform and Environment providers/directives/pipes
 */
import { AuthInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';
import { GarbService } from './garb.service';
import { HomeComponent } from './home';
import { IconListService } from './icon.service';
import { IssuesComponent } from './issues/issues.component';
import { LocalStorageService } from './local-storage.service';
import { LoginComponent } from './login/login.component';
import { MessageService } from './message.service';
import { MessagesComponent } from './messages/messages.component';
import { ScreenshotComponent } from './screenshot';
import { TokenComponent } from './token/token.component';
import { VdlModule } from './vdl-module';

@NgModule({
  bootstrap: [AppComponent],
  declarations: [
    AppComponent,
    HomeComponent,
    ScreenshotComponent,
    AboutComponent,
    MessagesComponent,
    LoginComponent,
    IssuesComponent,
    TokenComponent
  ],
  entryComponents: [TokenComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    FlexLayoutModule,
    HttpClientModule,
    RouterModule.forRoot(ROUTES, { useHash: true, preloadingStrategy: PreloadAllModules }),
    VdlModule,
    LoadingModule.forRoot({
      animationType: ANIMATION_TYPES.rectangleBounce
    })
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true,
    },
    environment.ENV_PROVIDERS,
    GarbService,
    MessageService,
    AuthService,
    LocalStorageService,
    IconListService]
})
export class AppModule {
}
