import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';

import { MyPartiesComponent } from './my-parties.component';

import { ChiliCookoffService } from './chili-cookoff.service';

@NgModule({
  declarations: [
      AppComponent,
      MyPartiesComponent
  ],
    imports: [
        NgbModule.forRoot(),
        BrowserModule,
        HttpClientModule
    ],
    providers: [ChiliCookoffService],
  bootstrap: [AppComponent]
})
export class AppModule { }
