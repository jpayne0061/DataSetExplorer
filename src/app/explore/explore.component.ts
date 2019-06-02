import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { HttpEventType, HttpClient } from '@angular/common/http';
import { TableFile } from '../../models/tableFile';
import { Observable, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { Column } from '../../models/column';

@Component({
  selector: 'app-explore',
  templateUrl: './explore.component.html',
  styleUrls: ['./explore.component.css']
})
export class ExploreComponent implements OnInit {
  @Input() parentSubject: Subject<any>;

  public tableGuid: string;

  public columns: Column[];

  public record: any;

  public records: any[];

  public dataSetTitle: string;

  //need timeout on ctor - - wait to fetch colunns
  constructor(private http: HttpClient) {

  }

  ngOnInit() {
    this.parentSubject.subscribe(event => {
      this.tableGuid = event;

      if (event == '0') {
        this.columns = null;
        this.dataSetTitle = "";
        this.records = null;
        return;
      }

      this.getColumns(this.tableGuid).subscribe(x => {
        console.log("columns: ", x);
        this.columns = x;
        this.record = {};
        console.log("this.columns: ", this.columns);
        if (this.columns && this.columns.length > 1) {
          //let objectProps: string[] = Object.keys(this.columns[0]);
          this.dataSetTitle = this.capitalizeFirstLetter(this.columns[0].dataSetTitle);
          this.record.tablePseudonym = this.tableGuid;
          for (var i = 0; i < this.columns.length; i++) {
            this.columns[i].columnName = this.capitalizeFirstLetter(this.columns[i].columnName);
            this.columns[i].displayName = this.capitalizeFirstLetter(this.columns[i].displayName);
            this.record[this.columns[i].columnName] = " ";
          }

        }

        console.log("record: ", this.record);
      });
    });
}

  public Send() {
    this.getRecords(this.record).subscribe(result => {
      console.log(result);
      this.records = result;
    });
  }

  getRecords(record: any): Observable<any[]> {
    return this.http.post<models.Record[]>('api/Values/', record)
      .pipe(map(res => res));
  }

  getColumns(tableName: string): Observable<Column[]> {
    return this.http.get<Column[]>('api/Values/' + tableName)
      .pipe(map(res => res));
  }

  capitalizeFirstLetter(string: string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
  }

  lowerFirstLetter(string: string) {
    return string.charAt(0).toLowerCase() + string.slice(1);
  }

}
