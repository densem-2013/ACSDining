(function() {

    ko.observable.fn.store = function() {
        var self = this;
        var oldValue = self();

        var observable = ko.computed({
            read: function() {
                return self();
            },
            write: function(value) {
                oldValue = self();
                self(value);
            }
        });

        this.revert = function() {
            self(oldValue);
        }
        this.commit = function() {
            oldValue = self();
        }
        return this;
    }

    var UserInfo = function(user) {

        var self = this;

        user = user || {};

        self.UserId = ko.observable(user.id).store();
        self.FirstName = ko.observable(user.firstName).store();
        self.LastName = ko.observable(user.lastName).store();
        self.Email = ko.observable(user.email).store();
        self.UserName = ko.observable(user.userName).store();
        self.IsDinningRoomClient = ko.observable(user.isDiningRoomClient).store();
        self.LastLoginTime = ko.observable(user.lastLoginTime);
        self.RegistrationDate = ko.observable(user.registrationDate);
        self.Roles = ko.observableArray(user.roles).store();

        self.FullName = ko.observable(self.LastName() + ' ' + self.FirstName());

        self.CanApp = ko.computed(function() { return self.IsDinningRoomClient() ? 'Да' : 'Нет'; }.bind(self));

        self.isEditMode = ko.observable(false);
    }

    var AccountsViewModel = function() {

        var self = this;

        self.Accounts = ko.observableArray();

        self.Message = ko.observable("");

        self.BeenChanged = ko.observable(false);

        self.ChangeSaved = ko.observable(false);



        // Callback for error responses from the server.
        function onError(error) {
            self.Message('Error: ' + error.status + ' ' + error.statusText);
        }

        self.LoadAccounts = function() {
            self.Message(''); // Clear the error

            app.service.allUsers().then(addUsers, onError);
        }

        self.pageSize = ko.observable(10);

        self.pageIndex = ko.observable(0);

        self.pagedList = ko.dependentObservable(function() {
            var size = self.pageSize();
            var start = self.pageIndex() * size;
            return self.Accounts.slice(start, start + size);
        }.bind(self));

        self.maxPageIndex = ko.dependentObservable(function() {
            return Math.ceil(self.Accounts().length / self.pageSize()) - 1;
        });

        self.previousPage = function() {
            if (self.pageIndex() > 0) {
                self.pageIndex(self.pageIndex() - 1);
            }
        };

        self.nextPage = function() {
            if (self.pageIndex() < self.maxPageIndex()) {
                self.pageIndex(self.pageIndex() + 1);
            }
        };

        self.allPages = ko.dependentObservable(function() {
            var pages = [];
            for (i = 0; i <= self.maxPageIndex(); i++) {
                pages.push({ pageNumber: (i + 1) });
            }
            return pages;
        });

        self.moveToPage = function(index) {
            self.pageIndex(index);
        };


        self.edit = function(item) {
            item.editing(true);
        };


        function applyFn(item, fn) {
            for (var prop in item) {
                if (item.hasOwnProperty(prop) && item[prop][fn]) {
                    item[prop][fn].apply();
                }
            }
        }

        function commitChanges(item) { applyFn(item, 'commit'); }

        function revertChanges(item) { applyFn(item, 'revert'); }

        self.cancel = function(item) {
            revertChanges(item);
            item.editing(false);
        };

        self.save = function(item) {
            app.service.update(item).then(
                function() {
                    commitChanges(item);
                },
                function(error) {
                    onError(error);
                    revertChanges(item);
                }).always(function() {
                item.editing(false);
            });
        }

        // Adds a JSON array of users to the view model.
        function addUsers(data) {

            var mapped = ko.utils.arrayMap(data, function (item) {
                return new UserInfo(item);
            });
            self.Accounts(mapped);

        };

        self.LoadAccounts();

    }
    ko.applyBindings(new AccountsViewModel());

}())