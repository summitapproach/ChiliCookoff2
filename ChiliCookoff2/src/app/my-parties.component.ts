import { Component, OnInit } from '@angular/core';

import { ChiliCookoffService } from './chili-cookoff.service';
import { PersonDTO } from './PersonDTO';

@Component({
    selector: 'my-parties',
    template: 
        `
        <h3>My Parties</h3>

        <div class="col-sm-4">
            <button class="joinedPartyLink" *ngFor="let joinedParty of personDTO?.JoinedParties">{{joinedParty.PartyName}}</button>
        </div>
        `
})
export class MyPartiesComponent implements OnInit {
    personDTO: PersonDTO; 

    constructor(private chiliCookoffService: ChiliCookoffService) { }

    ngOnInit() {
        this.chiliCookoffService.getParties().subscribe(personDTO => this.personDTO = personDTO);
    }
}