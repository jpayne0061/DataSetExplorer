import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { FormsModule } from '@angular/forms';
import { UploadComponent } from './upload/upload.component';
import { HttpClientModule } from '@angular/common/http';
import { ExploreComponent } from './explore/explore.component';

const appRoutes: Routes = [
  { path: 'new-data-set', component: UploadComponent },
  { path: 'explore', component: ExploreComponent }
];

@NgModule({
  declarations: [
    AppComponent,
    UploadComponent,
    ExploreComponent
  ],
  imports: [
    HttpClientModule,
    BrowserModule,
    FormsModule,
    RouterModule,
    RouterModule.forRoot(appRoutes)
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
