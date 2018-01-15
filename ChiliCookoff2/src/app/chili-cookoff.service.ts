import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { Observable } from 'rxjs/Observable';

import { PersonDTO } from './PersonDTO';

@Injectable()
export class ChiliCookoffService {
    private chiliCookOffUrl: string = "http://localhost:1476/api/";

    constructor(private http: HttpClient) { }

    public getParties(): Observable<PersonDTO> {
        return this.http.get<PersonDTO>(this.chiliCookOffUrl + 'Party');
    }

}