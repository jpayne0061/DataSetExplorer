import { Column } from "./column";

export class TableFile {
  public fileName: string;
  public columns: Column[];
  public description: string = "";

  public PrepareTableName() {
    return this.fileName.split('.')[0].replace(/[^a-z]/gi, '');
  }

}
