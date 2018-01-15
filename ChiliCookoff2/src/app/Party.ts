export class Party {
    public Id: string;
    public HostId: string;
    public Name: string;
    public Code: string;
    public Date: Date;
    public HasVotingStarted: boolean;
    public HaveVotesBeenTallied: boolean;
    public MaxChilis: number;

    constructor(
        Id: string,
        HostId: string,
        Name: string,
        Code: string,
        Date: Date,
        HasVotingStarted: boolean,
        HaveVotesBeenTallied: boolean,
        MaxChilis: number
    ) {
        this.Id = Id;
        this.HostId = HostId;
        this.Name = Name;
        this.Code = Code;
        this.Date = Date;
        this.HasVotingStarted = HasVotingStarted;
        this.HaveVotesBeenTallied = HaveVotesBeenTallied;
        this.MaxChilis = MaxChilis;
    }
}