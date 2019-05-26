import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';

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

  public record: Record = {
    SalaryDataID: " ",
    CalendarYear: " ",
    EmployeeName: " ",
    Department: " ",
    FirstName: " ",
    LastName: " ",
    JobTitle: " ",
    AnnualRate: " ",
    RegularRate: " ",
    OvertimeRate: " ",
    IncentiveAllowance: " ",
    Other: " ",
    YearToDate: " "
  };
  public records: Record[];

  constructor(private http: HttpClient) {

  }

  public Send() {
    this.getRecords(this.record).subscribe(result => {
      console.log(result);
      this.records = result;
    });
  }

  getRecords(record: Record): Observable<Record[]> {
    return this.http.post<Record[]>('api/Values/', record)
      .pipe(map(res => res));
  }


}
