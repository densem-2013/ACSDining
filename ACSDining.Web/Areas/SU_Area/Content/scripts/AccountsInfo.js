(function () {

    $("ul.nav.navbar-nav li:last-child").addClass("active");
    var accountInfo = function(account) {
        var self = this;
        self.UserId = ko.observable(account.userId);
        self.FullName = ko.observable(account.fullName);
        self.Email = ko.observable(account.email);
        self.LastLoginTime = ko.observable(account.lastLoginTime);
        self.RegistrationDate = ko.observable(account.registrationDate);
        self.Balance = ko.observable(account.balance);
        self.CanMakeBooking = ko.observable(account.canMakeBooking);
        self.IsExisting = ko.observable(account.isExisting);
    };

    var accountsViewModel = function() {
        var self = this;
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

        self.TotalCount = ko.observable();
        self.loadAccounts = function () {
            app.su_Service.GetAccounts().then(function (resp) {
                //self.Accounts([]);
                self.Accounts(ko.utils.arrayMap(resp, function (item) {
                    return new accountInfo(item);
                }));
            }, onError);
        }

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

        self.init = function () {

            self.loadAccounts();

        };

        self.init();
    }

    ko.applyBindings(new accountsViewModel());

}());