import { PartyDTO } from './PartyDTO';
import { Party } from './Party';

export class PersonDTO {
    public Id: string;
    public UserId: string;
    public FirstName: string;
    public LastName: string;
    public EmailAddress: string;
    public EnterChili: boolean;
    public ChiliName: string;
    public PartyCode: string;
    public IsAccountProfileUpdate: boolean;
    public JoinedParties: PartyDTO[];
    public HostedParties: Party[];

    constructor(
        Id: string,
        UserId: string,
        FirstName: string,
        LastName: string,
        EmailAddress: string,
        EnterChili: boolean,
        ChiliName: string,
        PartyCode: string,
        IsAccountProfileUpdate: boolean,
        JoinedParties: PartyDTO[],
        HostedParties: Party[]
    ) {
        this.Id = Id;
        this.UserId = UserId;
        this.FirstName = FirstName;
        this.LastName = LastName;
        this.EmailAddress = EmailAddress;
        this.EnterChili = EnterChili;
        this.ChiliName = ChiliName;
        this.PartyCode = PartyCode;
        this.IsAccountProfileUpdate = IsAccountProfileUpdate;
        this.JoinedParties = JoinedParties;
        this.HostedParties = HostedParties;
    }
}