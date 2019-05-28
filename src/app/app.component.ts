import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Column } from '../models/column';

class Record {
  public SalaryDataID: string;
  public CalendarYear: string;
  public EmployeeName: string;
  public Department: string;
  public FirstName: string;
  public LastName: string;
  public JobTitle: string;
  public AnnualRate: string;
  public RegularRate: string;
  public OvertimeRate: string;
  public IncentiveAllowance: string;
  public Other: string;
  public YearToDate: string;
}


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: [
    './app.component.css'
  ]
})
export class AppComponent {

  public tableName: string = "SalaryData";

  public columns: Column[];

  public record: any;

  public records: any[];

  constructor(private http: HttpClient) {
    this.getColumns(this.tableName).subscribe(x => {
      console.log("columns: ", x);
      this.columns = x;
      this.record = {};

      if (this.columns && this.columns.length > 1) {
        //let objectProps: string[] = Object.keys(this.columns[0]);

        for (var i = 0; i < this.columns.length; i++) {
          this.record[this.columns[i].columnName] = " ";
        }

      }

      console.log("record: ", this.record);
    });
  }

  public Send() {
    this.getRecords(this.record).subscribe(result => {
      console.log(result);
      this.records = result;
    });
  }

  getRecords(record: any): Observable<any[]> {
    return this.http.post<Record[]>('api/Values/', record)
      .pipe(map(res => res));
  }

  getColumns(tableName: string): Observable<Column[]> {
    return this.http.get<Column[]>('api/Values/' + tableName)
      .pipe(map(res => res));
  }


}
