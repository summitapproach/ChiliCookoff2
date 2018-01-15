"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
Object.defineProperty(exports, "__esModule", { value: true });
var core_1 = require("@angular/core");
var chili_cookoff_service_1 = require("./chili-cookoff.service");
var MyPartiesComponent = /** @class */ (function () {
    function MyPartiesComponent(chiliCookoffService) {
        this.chiliCookoffService = chiliCookoffService;
    }
    MyPartiesComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.chiliCookoffService.getParties().subscribe(function (personDTO) { return _this.personDTO = personDTO; });
    };
    MyPartiesComponent = __decorate([
        core_1.Component({
            selector: 'my-parties',
            template: "\n        <h3>My Parties</h3>\n\n        <div class=\"col-sm-4\">\n            <button class=\"joinedPartyLink\" *ngFor=\"let joinedParty of personDTO?.JoinedParties\">{{joinedParty.PartyName}}</button>\n        </div>\n        "
        }),
        __metadata("design:paramtypes", [chili_cookoff_service_1.ChiliCookoffService])
    ], MyPartiesComponent);
    return MyPartiesComponent;
}());
exports.MyPartiesComponent = MyPartiesComponent;
//# sourceMappingURL=my-parties.component.js.map