import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';

class Record {
  public SalaryDataID: number;
  public CalendarYear: number;
  public EmployeeName: string;
  public Department: string;
  public FirstName: string;
  public LastName: string;
  public JobTitle: string;
  public AnnualRate: number;
  public RegularRate: number;
  public OvertimeRate: number;
  public IncentiveAllowance: number;
  public Other: number;
  public YearToDate: number;
}


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: [
    './app.component.css'
  ]
})
export class AppComponent {

  public record: Record;
  public records: Record[];

  constructor(private http: HttpClient) {
    this.record = new Record();
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
