/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Areas/SU_Area/Content/scripts/app.su_Service.js" />
/// <reference path="~/Scripts/knockout-3.2.0.js" />
/// <reference path="~/Scripts/bootstrap.js" />
/// <reference path="~/Scripts/knockout-3.3.0.debug.js" />
/// <reference path="~/Scripts/knockout.mapping-latest.debug.js" />
/// <reference path="~/Content/app/jquery-1.10.2.js" />
(function () {

    $("#infoTitle span").text("Управление пользователями")
        .css({ 'background': "rgba(119, 222, 228, 0.61)", 'color': "rgb(232, 34, 208)", 'border': "3px solid rgb(50, 235, 213)" });
    $("ul.nav.navbar-nav li:last-child").addClass("active");
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
        self.Balance = ko.observable(account.balance);
        self.CanMakeBooking = ko.observable(account.canMakeBooking);
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

        function onError(error) {
            self.Message("Error: " + error.status + " " + error.statusText);
        }
        //self.checkAsRemoved = function (item) {
        //    app.su_Service.DeleteAccount(item.UserId()).then(function (resp) {
        //        self.loadAccounts();
        //    }, onError);
        //};

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
        self.update= function(account) {
            var forupdate = {
                UserId: account.UserId(),
                Email: account.Email().Email(),
                CanMakeBooking: account.CanMakeBooking(),
                IsExisting: account.IsExisting()
            }
            app.su_Service.UpdateAccount(forupdate).then(function(res) {

            });
        }

        self.init = function () {

            self.loadAccounts();

        };

        self.init();
    }

    ko.applyBindings(new accountsViewModel());

}());