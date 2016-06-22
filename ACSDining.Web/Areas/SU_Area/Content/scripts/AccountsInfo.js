/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Areas/SU_Area/Content/scripts/app.su_Service.js" />
/// <reference path="~/Scripts/knockout-3.2.0.js" />
/// <reference path="~/Scripts/bootstrap.js" />
/// <reference path="~/Scripts/knockout-3.3.0.debug.js" />
/// <reference path="~/Scripts/knockout.mapping-latest.debug.js" />
/// <reference path="~/Content/app/jquery-1.10.2.js" />
(function () {

    $("#menucontainer span").text("Управление пользователями")
        .addClass("navlink");
    $("ul.nav.navbar-nav li:last-child").addClass("active");

    var divadd = $("<div>").attr({ "id": "searchItems" });
    divadd.append($("<span>").text("Поиск"));
     var searchinput = $("<input/>")
    .attr({ 'data-bind': "value: handledSearch, valueUpdate: 'keyup'", "type": "search","autocomplete":"off","class":"form-control" });
    divadd.append(searchinput);
    divadd.append($("<span>").addClass("glyphicon glyphicon-search"));
    $("#submenu td:nth-child(3)").append(divadd);

    var divallowdebt = $("<div>").attr({ "id": "allowDebt" });
    divallowdebt.append($("<span>").text("Допустимый кредит : ").css({"padding":"5px"}));
    var debtInput = $("<input/>")
   .attr({ 'data-bind': "value: CurrentDebt, visible: DebtChangeMode", "type": "text", "class": "form-control" });
    divallowdebt.append(debtInput);
    var spanDebt = $("<span>").attr({ "data-bind": "text: CurrentDebt() +' грн', visible:!DebtChangeMode()" }).css({"width":"100px","font-size": "20px","text-align":"center"});
    divallowdebt.append(spanDebt);
    var changebut = $("<button>").attr({ "id": "changebut", "type": "button", "class": "btn btn-default btn-circle", "data-bind": "click: setDebtChangeMode, visible:!DebtChangeMode()" });
    changebut.append($("<span>").addClass("glyphicon glyphicon-edit"));
    divallowdebt.append(changebut);
    var savebut = $("<button>").attr({ "id": "savebut", "type": "button", "class": "btn btn-default btn-circle", "data-bind": "click: DebtSave, visible:DebtChangeMode" });
    savebut.append($("<span>").addClass("glyphicon glyphicon-save"));
    divallowdebt.append(savebut);
    $("#submenu td:nth-child(4)").append(divallowdebt);

    var emailValueModel = function (email) {
        var self = this;
        self.isEditMode = ko.observable(false);
        self.Email = ko.observable(email);
        self.clicked = function (item) {
            $(item).siblings("input").first().focusin();
        };
        self.doubleClick = function () {
            self.isEditMode(true);
        };
        self.onFocusOut = function () {
            self.isEditMode(false);
        };
    }
    var accountInfo = function (account) {
        var self = this;
        self.UserId = ko.observable(account.userId);
        self.FullName = ko.observable(account.fullName);
        self.Email = ko.observable(new emailValueModel(account.email));
        self.LastLoginTime = ko.observable(account.lastLoginTime);
        self.RegistrationDate = ko.observable(account.registrationDate);
        self.Balance = ko.observable(account.balance.toFixed(2));
        self.CheckDebt = ko.observable(account.checkDebt);
        self.IsExisting = ko.observable(account.isExisting);
    };

    var accountsViewModel = function() {
        var self = this;
        self.query = ko.observable("");
        self.Accounts = ko.observableArray([]);
        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.PageSizes = ko.pureComputed(function () {
            var res = [2, 5, 7, 10, 15, 20, 25];
            var all = self.Accounts().length;
            if (all > 25) {
                res.push(all);
            }
            return res;

        });
        self.CurrentDebt = ko.observable();
        self.DebtChangeMode = ko.observable(false);
        function onError(error) {
            self.Message("Error: " + error.status + " " + error.statusText);
        }
        //self.checkAsRemoved = function (item) {
        //    app.su_Service.DeleteAccount(item.UserId()).then(function (resp) {
        //        self.loadAccounts();
        //    }, onError);
        //};
        self.setDebtChangeMode = function() {
            self.DebtChangeMode(true);
        };

        self.DebtSave = function () {

            app.su_Service.UpdateAllowDebt(self.CurrentDebt());
            self.DebtChangeMode(false);
        };
        self.pageSize = ko.observable(7);

        self.pageIndex = ko.observable(0);

        self.pagedList = ko.dependentObservable(function () {
            var size = self.pageSize();
            var start = self.pageIndex() * size;
            return self.Accounts().slice(start, start + size);
        });

        self.maxPageIndex = ko.dependentObservable(function () {
            return Math.ceil(self.Accounts().length / self.pageSize()) - 1;
        });

        self.previousPage = function () {
            if (self.pageIndex() > 0) {
                self.pageIndex(self.pageIndex() - 1);
                //item.blur();
            }
        };

        self.nextPage = function () {
            if (self.pageIndex() < self.maxPageIndex()) {
                self.pageIndex(self.pageIndex() + 1);
                //item.blur();
            }
        };

        self.allPages = ko.dependentObservable(function () {
            var pages = [];
            for (var i = 0; i <= self.maxPageIndex() ; i++) {
                pages.push({ pageNumber: (i + 1) });
            }
            return pages;
        });

        self.moveToPage = function (index) {
            self.pageIndex(index);
        };
        self.loadAccounts = function () {
            app.su_Service.GetAccounts().then(function (resp) {

                self.Accounts(ko.utils.arrayMap(resp, function (item) {
                    return new accountInfo(item);
                }));
            }, onError);
        }
        var stringStartsWith = function (string, startsWith) {
            string = string || "";
            if (startsWith.length > string.length)
                return false;
            return string.substring(0, startsWith.length) === startsWith;
        };
        self.handledSearch = ko.pureComputed({
            read: function () {
                return self.query();
            },
            write: function (value) {

                value = value.replace(new RegExp(/[^a-zA-Zа-яА-Я]/g), "");
                self.query(value);
            },
            owner: self
        });

        self.filterList = ko.dependentObservable(function () {
            var filter = self.query().toLowerCase();

            if (!filter) {
                return self.pagedList();
            } else {
                return ko.utils.arrayFilter(self.Accounts(), function (item) {
                    return stringStartsWith(item.FullName().toLowerCase(), filter);
                });
            }
        });
        
       
        self.updateEmail= function(account) {
            var forupdate = {
                UserId: account.UserId(),
                Email: account.Email().Email()
            }
            app.su_Service.UpdateAccountEmail(forupdate);
        }
        self.debtupdate = function (account) {

            var formbupdate = {
                UserId: account.UserId(),
                CheckDebt: !account.CheckDebt()
            }
            app.su_Service.UpdateAccountCheckDebt(formbupdate);
            return true;
        }
        self.existsupdate = function (account) {

            var forupdate = {
                UserId: account.UserId(),
                IsExisting: !account.IsExisting()
            }
            app.su_Service.UpdateAccountExists(forupdate);
            return true;
        }

        self.init = function () {

            self.loadAccounts();
            app.su_Service.GetDebt().then(function(result) {
                self.CurrentDebt(result);
            });
        };

        self.init();
    }

    ko.applyBindings(new accountsViewModel());

}());