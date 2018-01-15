export class PartyDTO {
    public PartyId: string;
    public PartyName: string;
    public PartyDate: Date;
    public EnterChili: boolean;
    public ChiliName: string;
    public HostName: string;
    public ChiliNumber: number;
    public PartyCode: string;
    public MaxChilis: number;
    public HasVotingStarted: boolean;
    public HaveVotesBeenTallied: boolean;
    public HasSubmittedBallot: boolean;
    public IsHostUpdate: boolean;

    constructor(
        PartyId: string,
        PartyName: string,
        PartyDate: Date,
        EnterChili: boolean,
        ChiliName: string,
        HostName: string,
        ChiliNumber: number,
        PartyCode: string,
        MaxChilis: number,
        HasVotingStarted: boolean,
        HaveVotesBeenTallied: boolean,
        HasSubmittedBallot: boolean,
        IsHostUpdate: boolean
    ) {
        this.PartyId = PartyId;
        this.PartyName = PartyName;
        this.PartyDate = PartyDate;
        this.EnterChili = EnterChili;
        this.ChiliName = ChiliName;
        this.HostName = HostName;
        this.ChiliNumber = ChiliNumber;
        this.PartyCode = PartyCode;
        this.MaxChilis = MaxChilis;
        this.HasVotingStarted = HasVotingStarted;
        this.HaveVotesBeenTallied = HaveVotesBeenTallied;
        this.HasSubmittedBallot = HasSubmittedBallot;
        this.IsHostUpdate = IsHostUpdate;
    }
}