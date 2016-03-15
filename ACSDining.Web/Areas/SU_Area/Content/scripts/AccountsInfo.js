(function () {

    var accountInfo = function(account) {
        var self = this;
        self.UserId = ko.observable(account.userId);
        self.FullName = ko.observable(account.lastName + ' ' + account.firstName);
        self.Login = ko.observable(account.userName);
        self.LastLoginTime = ko.observable(account.lastLoginTime);
        self.RegistrationDate = ko.observable(account.registrationDate);
    };

    var accountsViewModel = function() {
        var self = this;
        self.Accounts = ko.observableArray([]);

        function onError(error) {
            self.Message('Error: ' + error.status + ' ' + error.statusText);
        };

        self.loadAccounts = function () {
            app.su_Service.GetAccounts().then(function (resp) {
                self.Accounts([]);
                self.Accounts().pushAll(ko.utils.arrayMap(resp, function (item) {
                    return new accountInfo(item);
                }));
            }, onError);
        }

        self.remove = function (item) {
            app.su_Service.DeleteAccount(item.UserId()).then(function (resp) {
                self.loadAccounts();
            }, onError);
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
            }
        };

        self.nextPage = function () {
            if (self.pageIndex() < self.maxPageIndex()) {
                self.pageIndex(self.pageIndex() + 1);
            }
        };

        self.allPages = ko.dependentObservable(function () {
            var pages = [];
            for (i = 0; i <= self.maxPageIndex() ; i++) {
                pages.push({ pageNumber: (i + 1) });
            }
            return pages;
        });

        self.moveToPage = function (index) {
            self.pageIndex(index);
        };

        self.loadAccounts();
    }

    ko.applyBindings(new accountsViewModel());

}());