(function () {

    $("ul.nav.navbar-nav li:nth-child(3)").addClass("active");
    $(".wrapper").css({ 'margin': "auto" });

    var dishInfo = function(dish) {
        var self = this;
        self.DishId = ko.observable(dish.dishId);
        self.Description = ko.observable(dish.description);
        self.Title = ko.observable(dish.title);
        self.Price = ko.observable(dish.price.toFixed(2));
        self.formattedPrice = ko.pureComputed({
            read: function () {
                return  self.Price();
            },
            write: function (value) {
                // Strip out unwanted characters, parse as float, then write the 
                // raw data back to the underlying "price" observable
                value = value.replace(new RegExp(/(.*)[,](.*)/, "g"), "$1\.$2");
                value = value.replace(new RegExp(/(.*)(\.)+(.*)(\.)(.*)/, "g"), "$1$2$3");
                value = value.replace(/[^\.\d]/g, "");
                value = parseFloat(value);
                self.Price(isNaN(value) ? 0 : value); // Write to underlying storage
            },
            owner: self
        });
        self.Category = ko.observable(dish.category);

        self.isHovering = ko.observable(false);
    };

    var dishesViewModel = function() {
        var self = this;

        self.myDate = ko.observable(new Date());
        self.Categories = ko.observableArray([]);
        self.SelectedCategory = ko.observable();
        self.BeenChanged = ko.observable(false);
        self.Message = ko.observable("");
        self.DishesByCategory = ko.observableArray([]);
        self.ChangingDish = ko.observable();
        self.ModalTitle = ko.observable("");

        // Callback for error responses from the server.
        function onError(error) {
            self.Message("Error: " + error.status + " " + error.statusText);
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
        //self.save = function(item) {
        //    app.su_Service.UpdateDish(item).then(
        //        function() {
        //            self.BeenChanged(false);
        //        }, onError);
        //}

        //self.edit = function(item) {
        //    self.BeenChanged(true);
        //};


        self.remove = function(item) {
            app.su_Service.DeleteDish(item.DishId()).then(function (resp) {

                self.loadDishes(self.SelectedCategory());
            }, onError);
        };


        self.loadDishes = function (category) {
            app.su_Service.DishesByCategory(category).then(function (resp) {
                self.DishesByCategory(ko.utils.arrayMap(resp, function (item) {
                    return new dishInfo(item);
                }));

            }, onError);
        }
        self.updatePrepare = function (dish) {
            self.ChangingDish(dish);
            self.ModalTitle("Изменить");

            $("#modalUpdate").modal("show");
            
        }
        self.update = function() {
                app.su_Service.UpdateDish(self.ChangingDish()).then(function(resp) {
                    self.loadDishes(self.SelectedCategory());
                    $("#modalUpdate").modal("hide");
                }, onError);
        };

        self.createPrepare=function() {
            
            var item = {
                title: "",
                price: 0.00,
                description: "",
                category: self.SelectedCategory()
            };

            self.ChangingDish(new dishInfo(item));

            self.ModalTitle("Добавить");

            $("#modalbox").modal("show");
        }

        self.create = function() {
                    app.su_Service.CreateDish(self.ChangingDish()).then(function (resp) {
                        self.loadDishes(self.SelectedCategory());
                        $("#modalbox").modal("hide");
                    }, onError);

        };

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

    ko.applyBindings(new dishesViewModel());
}());
