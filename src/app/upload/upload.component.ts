import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { HttpEventType, HttpClient } from '@angular/common/http';
import { TableFile } from '../../models/tableFile';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.css']
})
export class UploadComponent implements OnInit {

  public tableFile: TableFile;

  public progress: number;
  public message: string;
  @Output() public onUploadFinished = new EventEmitter();

  constructor(private http: HttpClient) { }

  ngOnInit() {
  }

  public uploadFile = (files) => {
    if (files.length === 0) {
      return;
    }

    let fileToUpload = <File>files[0];
    const formData = new FormData();
    formData.append('file', fileToUpload, fileToUpload.name);

    this.http.post<TableFile>('api/Values/upload', formData, { reportProgress: true, observe: 'events' })
      .subscribe(event => {
        if (event.type === HttpEventType.UploadProgress)
          this.progress = Math.round(100 * event.loaded / event.total);
        else if (event.type === HttpEventType.Response) {
          console.log("table file: ", event.body);
          this.tableFile = event.body;
          this.message = 'Upload success.';
          this.onUploadFinished.emit(event.body);
        }
      });
  }

  public CreateTable() {
    console.log("this table: ", this.tableFile);

    this.RequestCeateTable(this.tableFile).subscribe(x => {
      console.log("response from create table: ", x);
    });
  }


  RequestCeateTable(tableFile: TableFile) {
    return this.http.post('api/Values/CreateTable', tableFile)
      .pipe(map(res => res));
  }

}
