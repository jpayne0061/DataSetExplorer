import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { HttpEventType, HttpClient } from '@angular/common/http';
import { TableFile } from '../../models/tableFile';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';

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
  @Output() public uploadStarted = new EventEmitter<boolean>();

  public dataStatus: string[];
  public rows: number;
  public rowUpdate: number;

  private hubConnection: HubConnection;
  msgs: string[] = [];

  constructor(private http: HttpClient) {
    this.startSignalR();
  }

  ngOnInit() {

  }

  public startSignalR() {
    let builder = new HubConnectionBuilder();

    // as per setup in the startup.cs
    this.hubConnection = builder.withUrl('/models/echo').build();

    this.hubConnection.on("data update", (message) => {
      console.log(message);
      if (!this.dataStatus) {
        this.dataStatus = [];
      }

      this.dataStatus.push(message);
    });

    this.hubConnection.on("row update", (message) => {
      console.log(message);
      this.rowUpdate = message;
    });

    this.hubConnection.on("rows found", (message) => {
      console.log(message);
      this.rows = message;
    });

    this.hubConnection.on("complete", (message) => {
      console.log(message);
      location.reload();
    });

    // starting the connection
    this.hubConnection.start();
  }

  public uploadFile = (files) => {

    this.uploadStarted.emit(true);

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
