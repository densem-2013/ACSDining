/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />


(function () {

   // $("#infoTitle span").attr({ 'data-bind': 'text: WeekTitle' });


    var DishInfo = function (dinfo) {

        var self = this;

        self.DishId = ko.observable(dinfo.dishID);
        self.Title = ko.observable(dinfo.title);
        self.ProductImage = ko.observable(dinfo.productImage);
        self.Price = ko.observable(dinfo.price.toFixed(2));
        self.Category = ko.observable(dinfo.category);

        self.UpdateWeekMenu = function (dishupdate) {
            this.DishId(dishupdate.DishId());
            this.Title(dishupdate.Title());
            this.ProductImage(dishupdate.ProductImage());
            this.Price(dishupdate.Price());

        };
    }

    var MenuForDayInfo = function (object,categs) {

        var self = this;

        object = object || {};
        categs = categs || [];

        self.ID = ko.observable(object.id);
        self.DayOfWeek = ko.observable(object.dayOfWeek);
        self.Dishes = ko.observableArray(ko.utils.arrayMap(categs, function (item) {
            var first = ko.utils.arrayFirst(object.dishes, function (element) {

                return element.category === item;
            });
            return new DishInfo(first);
        }));
        self.Editing = ko.observable(false);
        self.TotalPrice = ko.observable();

        self._Undo ;


        self.Dishes.subscribe = ko.computed(function () {
            var sum = 0;
            var valsum;
            for (var i = 0; i < self.Dishes().length; i++) {

                valsum = parseFloat(self.Dishes()[i].Price());
                sum += valsum;
            };


            self.TotalPrice(sum.toFixed(2));

        }.bind(self));

    }

    var weekMenuModel = function() {
        var self = this;

        self.MenuId = ko.observable();
        self.CurrentWeekNumber = ko.observable();
        self.WeekNumber = ko.observable();

        self.MFD_models = ko.observableArray([]);


        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.DishesByCategory = ko.observableArray([]);
        self.Category = ko.observable();
        self.Categories = ko.observableArray();

        self.SelectedDish = ko.observable();

        self.UpdatableMFD = ko.observable();

        self.SummaryPrice = ko.observable();

        self.NumbersWeeks = ko.observableArray();

        self.BeenChanged = ko.observable(false);

        self.ChangeSaved = ko.observable(false);
        self.Year = ko.observable();

        // Callback for error responses from the server.
        function onError(error) {
            self.Message('Error: ' + error.status + ' ' + error.statusText);
        }

        self.WeekTitle = ko.computed(function() {
            var options = {
                weekday: "short",
                year: "numeric",
                month: "short",
                day: "numeric"
            };
            var year = self.Year();
            var firstDay = new Date(year, 0, 1).getDay();

            var week = self.WeekNumber();
            var d = new Date("Jan 01, " + year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week - 1);
            var n1 = new Date(w);
            var n2 = new Date(w + 345600000);
            return "Неделя " + week + ", " + n1.toLocaleDateString("ru-RU", options) + " - " + n2.toLocaleDateString("ru-RU", options);
        }.bind(self));


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

        self.save = function(item) {

            app.su_Service.UpdateWeekMenu(item).then(
                function() {
                    commitChanges(item);
                },
                function(error) {
                    onError(error);
                    revertChanges(item);
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


        self.loadWeekNumbers = function () {
            app.su_Service.LoadWeekNumbers().then(function(resp) {

                for (var i = 0; i < resp.length; i++) {

                    self.NumbersWeeks.push(resp[i]);

                };
            }, onError);
        };

        self.loadDishes = function (dish) {
            app.su_Service.DishesByCategory(dish.Category()).then(function (resp) {
                self.DishesByCategory([]);

                for (var i = 0; i < resp.length; i++) {

                    self.DishesByCategory.push(new DishInfo(resp[i]));

                    if (resp[i].dishID == dish.DishId()) {
                        self.SelectedDish(resp[i].dishID);
                    };
                };

            }, onError);
        }
        self.showDishes = function (searchdish, index) {
            self.UpdatableMFD(index);
            self.Category(searchdish.Category());
            self.loadDishes(searchdish);


            $("#modalbox").modal("show");
        }

        self.changeSelected = function (clikedItem) {
            if (self.SelectedDish() === clikedItem.DishId()) {
                self.BeenChanged(true);
                self.SelectedDish(clikedItem.DishId());
            }
            return true;
        }

        self.SetMyDateByWeek = function (weeknumber) {
            var year = self.Year();
            var firstDay = new Date(year, 0, 1).getDay();
            var d = new Date("Jan 01, " + year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (weeknumber - 1);
            self.myDate(new Date(w));
        }.bind(self);

        self.LoadWeekMenu = function (numweek, year) {


            app.su_Service.LoadWeekMenu(numweek, year).then(function (resp) {
                self.MFD_models([]);
                //self.SetMyDateByWeek(resp.weekNumber);

                self.MenuId(resp.id);
                self.WeekNumber(resp.weekNumber);
                self.Year(resp.yearNumber);
                ko.utils.arrayForEach(resp.mfD_models, function (object) {

                    self.MFD_models.push(new MenuForDayInfo(object,self.Categories()));

                });

            }, onError);
        }


        self.myDate.subscribe = ko.computed(function () {
            var takedWeek = self.myDate().getWeek() + 1;
            var curweek = self.WeekNumber();
            if (takedWeek !== curweek) {
                self.LoadWeekMenu(takedWeek, self.Year());
            }
        }, self);

        self.GetCurrentWeekNumber = function () {

            app.su_Service.GetCurrentWeekNumber().then(function (resp) {
                self.CurrentWeekNumber(resp);

            }, onError);
        }

        self.IsCurrentWeek = ko.computed(function () {
            return self.CurrentWeekNumber() == self.WeekNumber();
        }.bind(self));



        self.applyChanges = function () {

            var catIndex = $.map(self.Categories(), function (n, i) {
                if (self.Category() === n)
                    return i;
            });


            var dish = ko.utils.arrayFirst(self.DishesByCategory(), function (value) {
                return value.DishId() === self.SelectedDish();
            });

            if (dish != undefined) {

                self.MFD_models()[self.UpdatableMFD()].Dishes()[catIndex].UpdateWeekMenu(dish);
                this.CalcTotal();
            };



            $("#modalbox").modal("hide");
        }


        self.CalcTotal = function () {

            var sum = 0;

            for (ind = 0; ind < self.MFD_models().length; ind++) {
                sum += parseFloat(self.MFD_models()[ind].TotalPrice());

            }

            this.SummaryPrice(sum.toFixed(2));
        }.bind(self);

        self.MFD_models.subscribe = ko.computed(self.CalcTotal, self);

        app.su_Service.GetCategories().then(function (resp) {
            self.Categories([]);
            self.Categories.pushAll(resp);
        }, onError);

        self.LoadWeekMenu();
        self.loadWeekNumbers();
        self.GetCurrentWeekNumber();
    };

    ko.applyBindings( new weekMenuModel());


})();
