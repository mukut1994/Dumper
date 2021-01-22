import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { RootRenderComponent } from './root-render/root-render.component';
import { ArrayRenderComponent } from './array-render/array-render.component';
import { AnyRenderComponent } from './any-render/any-render.component';
import { ObjectRenderComponent } from './object-render/object-render.component';
import { FormRenderComponent } from './form-render/form-render.component';
import { ProgressRenderComponent } from './progress-render/progress-render.component';
import { EnumRenderComponent } from './enum-render/enum-render.component';
import { NavigationRenderComponent } from './navigation-render/navigation-render.component';
import { HorizontalRenderComponent } from './horizontal-render/horizontal-render.component';
import { VerticalRenderComponent } from './vertical-render/vertical-render.component';
import { LogRenderComponent } from './log-render/log-render.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NavBarComponent } from './navigation-render/nav-bar/nav-bar.component';
import { CommonModule } from '@angular/common';
import { RenderDirective } from './render.service';

@NgModule({
  declarations: [
    AppComponent,
    RootRenderComponent,
    ArrayRenderComponent,
    AnyRenderComponent,
    ObjectRenderComponent,
    FormRenderComponent,
    ProgressRenderComponent,
    EnumRenderComponent,
    NavigationRenderComponent,
    HorizontalRenderComponent,
    VerticalRenderComponent,
    LogRenderComponent,
    NavBarComponent,
    RenderDirective
  ],
  imports: [
  BrowserModule,
    AppRoutingModule,
    FormsModule,
    NgbModule,
    CommonModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
