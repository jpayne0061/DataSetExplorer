import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { TableFile } from '../models/tableFile';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: [
    './app.component.css'
  ]
})
export class AppComponent {
  public tableFiles: TableFile[];

  public selectedDataSet: string;

  public hide: boolean = false;
  public hideUpload: boolean = false;

  parentSubject: Subject<any> = new Subject();

  constructor(private http: HttpClient) {
    this.getTables().subscribe(x => {
      console.log("tables: ", x);
      this.tableFiles = x;
    });
  }

  public setDataSet(dataSet: string) {
    this.hideUpload = true;
    this.notifyChildren(dataSet);
    this.selectedDataSet = dataSet;
  }

  notifyChildren(dataSet: string) {
    this.hide = true;
    this.parentSubject.next(dataSet);
  }

  getTables(): Observable<TableFile[]> {
    return this.http.get<TableFile[]>('api/Values/GetTables')
      .pipe(map(res => res));
  }

  public hideDataSets() {
    this.hide = true;
  }
}
