(function() {
    var DishInfo = function(dish) {
        var self = this;
        self.DishId = ko.observable(dish.dishID);
        self.Title = ko.observable(dish.title);
        self.Price = ko.observable(dish.price.toFixed(2));
        self.Category = ko.observable(dish.category);
        self.Editing = ko.observable(false);
        self.Foods = ko.observable(dish.foods);

        self._Undo;
    };

    var DishesViewModel = function() {
        var self = this;

        self.Categories = ko.observableArray([]);
        self.SelectedCategory = ko.observable();
        self.BeenChanged = ko.observable(false);
        self.Message = ko.observable("");
        self.DishesByCategory = ko.observableArray([]);

// Callback for error responses from the server.
        function onError(error) {
            self.Message('Error: ' + error.status + ' ' + error.statusText);
        };

        self.pageSize = ko.observable(7);

        self.pageIndex = ko.observable(0);

        self.pagedList = ko.dependentObservable(function() {
            var size = self.pageSize();
            var start = self.pageIndex() * size;
            return self.DishesByCategory.slice(start, start + size);
        });

        self.maxPageIndex = ko.dependentObservable(function() {
            return Math.ceil(self.DishesByCategory().length / self.pageSize()) - 1;
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

        self.changeSelected = function(category) {
            if (self.SelectedCategory() === category) {
                self.loadDishes(category);
                self.SelectedCategory(category);
            }
            return true;
        }
        self.save = function(item) {

            app.su_Service.UpdateDish(item).then(
                function() {
                    commitChanges(item);
                },
                function(error) {
                    onError(error);
                    revertChanges(item);
                }).always(function() {
                item.UnEditable();
            });
        }

        self.edit = function(item) {
            self.BeenChanged(true);
            item._Undo = ko.mapping.toJS(item);
            item.Editing(true);
        };


        function commitChanges(item) {
            item.Editing(false);
            self.BeenChanged(false);
        }

        function revertChanges(item) {
            item.Editing(false);
            self.BeenChanged(false);
        }

        self.cancel = function(item) {
            item = ko.mapping.fromJS(item._Undo, {}, item);
            revertChanges(item);
            item.Editing(false);
        };
        self.remove = function(item) {
            app.su_Service.DeleteDish(item).then(function(resp) {
                self.init();
            }, onError);
        };

        self.create = function () {
            var item = new DishInfo();
            app.su_Service.CreateDish(item).then(function (resp) {
                self.init();
            }, onError);
        }
        self.loadDishes = function(category) {
            app.su_Service.DishesByCategory(category).then(function(resp) {
                self.DishesByCategory([]);
                self.DishesByCategory.pushAll(ko.utils.arrayMap(resp, function(item) {
                    return new DishInfo(item);
                }));

            }, onError);
        }

        self.init = function() {
            app.su_Service.GetCategories().then(function(resp) {
                self.Categories([]);
                self.Categories.pushAll(resp);
                self.SelectedCategory(self.Categories()[0]);
                self.loadDishes(self.Categories()[0]);
            }, onError);
        };
        self.init();
    };

    ko.applyBindings(new DishesViewModel());
}());