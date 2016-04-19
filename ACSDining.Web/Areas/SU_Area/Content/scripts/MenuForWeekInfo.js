/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Areas/SU_Area/Content/scripts/app.su_Service.js" />
/// <reference path="~/Scripts/knockout-3.2.0.js" />
/// <reference path="~/Scripts/bootstrap.js" />
/// <reference path="~/Scripts/knockout-3.3.0.debug.js" />
/// <reference path="~/Scripts/knockout.mapping-latest.debug.js" />
(function () {

    $("#infoTitle span").attr({ 'data-bind': "text: WeekTitle" })
        .css({ 'background': "rgba(119, 222, 228, 0.61)", 'color': "rgb(232, 34, 208)", 'border': "3px solid rgb(50, 235, 213)" });


    var dishInfo = function (dinfo) {

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

    var menuForDayInfo = function (object,categs) {

        var self = this;

        object = object || {};
        categs = categs || [];

        self.ID = ko.observable(object.id);
        self.DayOfWeek = ko.observable(object.dayOfWeek);
        var ind = 0;
        self.Dishes = ko.observableArray(ko.utils.arrayMap(categs, function (item) {
            var first = ko.utils.arrayFirst(object.dishes, function (element) {

                return element.category === item;
            });
            if (first != null) {
                return new dishInfo(first);
            }
            var dish= {
                dishID: "0",
                title: ":",
                productImage: "",
                price: 0.0,
                category: categs[ind++]
            }
            return new dishInfo(dish);
        }));
        self.Editing = ko.observable(false);
        self.TotalPrice = ko.observable();
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

        self.CurrentWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.WeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));


        self.MFD_models = ko.observableArray([]);


        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.DishesByCategory = ko.observableArray([]);

        self.Category = ko.observable();

        self.Categories = ko.observableArray();

        self.SelectedDish = ko.observable();

        self.UpdatableMFD = ko.observable();

        self.SummaryPrice = ko.observable();

        //self.NumbersWeeks = ko.observableArray();

        self.IsNextWeekMenuExist = ko.observable();

        //self.IsNextWeekMenuExist.subscribe = ko.computed(function() {

        //    var cur = self.CurrentWeekNumber();
        //    var result = false;
        //    ko.utils.arrayForEach(self.NumbersWeeks(), function (value) {
        //        if (value === cur + 1) result = true;
        //    });
        //    self.IsNextWeekMenuExist(result);
        //});

        self.IsNextWeekYear = ko.observable(false);

       

        self.IsCurrentWeek = ko.pureComputed(function () {

            return self.CurrentWeekYear().week === self.WeekYear().week && self.CurrentWeekYear().year === self.WeekYear().year;

        }, self);

        self.BeenChanged = ko.observable(false);

        self.ChangeSaved = ko.observable(false);

        // Callback for error responses from the server.
        function onError(error) {
            self.Message("Error: " + error.status + " " + error.statusText);
            $("#modalMessage").modal("show");
        }

        self.WeekTitle = ko.computed(function () {
            var options = {
                weekday: "short",
                year: "numeric",
                month: "short",
                day: "numeric"
            };
            var year = self.WeekYear().year;
            var firstDay = new Date(year, 0, 1).getDay();

            var week = self.WeekYear().week;
            var d = new Date("Jan 01, " + year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week);
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
            for (var i = 0; i <= self.maxPageIndex(); i++) {
                pages.push({ pageNumber: (i + 1) });
            }
            return pages;
        });

        self.moveToPage = function(index) {
            self.pageIndex(index);
        };

        function commitChanges(item) {
            item.Editing(false);
            self.BeenChanged(false);
        }

        function revertChanges(item) {
            item.Editing(false);
            self.BeenChanged(false);
        }

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



        self.cancel = function(item) {
            item = ko.mapping.fromJS(item._Undo, {}, item);
            revertChanges(item);
            item.Editing(false);
        };


        self.loadWeekNumbers = function () {
            app.su_Service.LoadWeekNumbers().then(function(resp) {
                self.NumbersWeeks([]);
                for (var i = 0; i < resp.length; i++) {

                    self.NumbersWeeks.push(resp[i]);

                };
            }, onError);
        };

        self.loadDishes = function (dish) {
            app.su_Service.DishesByCategory(dish.Category()).then(function (resp) {
                self.DishesByCategory([]);

                for (var i = 0; i < resp.length; i++) {

                    self.DishesByCategory.push(new dishInfo(resp[i]));

                    if (resp[i].dishID === dish.DishId()) {
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

        self.SetMyDateByWeek = function (wyDto) {
            var firstDay = new Date(wyDto.year, 0, 1).getDay();
            var d = new Date("Jan 01, " + wyDto.year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (wyDto.week);
            self.myDate(new Date(w));
        }.bind(self);

        var loadWeekMenu = function (wyDto) {

            app.su_Service.LoadWeekMenu(wyDto).then(function (resp) {

                self.MFD_models(ko.utils.arrayMap(resp.mfdModels,function(item) {
                    return new menuForDayInfo(item, self.Categories());
                }));

                self.MenuId(resp.id);
                self.WeekYear(resp.weekYear);
                self.SummaryPrice(resp.summaryPrice);

                app.su_Service.IsNextWeekYear(wyDto).then(function (resp2) {
                    self.IsNextWeekYear(resp2);
                });

            }, onError);

        }

        //self.LoadWeekMenu = function (numweek, year) {

        //    loadmenu(numweek, year);

        //}

        self.NextWeekMenu = function () {

            app.su_Service.GetNextWeekYear(self.WeekYear()).then(function(resp) {

                self.SetMyDateByWeek(resp);

            }, onError);

        }
        self.PrevWeekMenu = function () {

            app.su_Service.GetPrevWeekYear(self.WeekYear()).then(function (resp) {

                self.SetMyDateByWeek(resp);
            }, onError);

        }

        self.GoToNextWeekMenu = function () {
            
            app.su_Service.GetNextWeekYear(self.CurrentWeekYear()).then(function (nextWeekYear) {

                self.SetMyDateByWeek(nextWeekYear);

            });
        };

        self.myDate.subscribe = ko.computed(function () {
            var takedWeek = self.myDate().getWeek() - 1;
            var needObj = self.WeekYear();
            if (needObj != undefined) {
                var curweek = needObj.week;
                if (!isNaN(takedWeek) && takedWeek !== curweek) {
                    var weekyear = {
                        Week: takedWeek,
                        Year: self.myDate().getFullYear()
                    };
                    if (!isNaN(weekyear.Week) && !isNaN(weekyear.Year)) {

                        loadWeekMenu(weekyear);
                    }
                };
            };
        }, self);

        self.GetCurrentWeekYear = function () {

            app.su_Service.GetCurrentWeekYear().then(function (resp) {

                self.CurrentWeekYear(resp);

            }, onError);
        }



        self.applyChanges = function () {

            var catIndex = $.map(self.Categories(), function (n, i) {
                if (self.Category() === n)
                    return i;
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

        self.DeleteNextWeekMenu = function () {
            var menuid = self.MenuId();
            app.su_Service.DeleteNextWeekMenu(menuid).then(function () {
                self.LoadWeekMenu();
                self.loadWeekNumbers();
                self.SetMyDateByWeek(self.CurrentWeekNumber(), self.Year());
            }, onError);
        }

        self.CalcTotal = function () {

            var sum = 0;
            ko.utils.arrayForEach(self.MFD_models(), function(item) {

                sum += parseFloat(item.TotalPrice());

            });

            this.SummaryPrice(sum.toFixed(2));

        }.bind(self);

        self.MFD_models.subscribe = ko.computed(self.CalcTotal, self);

        self.init = function () {
            app.su_Service.GetCategories().then(function (resp) {
                self.Categories([]);
                self.Categories.pushAll(resp);
            }, onError);

            //self.loadWeekNumbers();
           
            app.su_Service.GetCurrentWeekYear().then(function(resp) {

                self.CurrentWeekYear(resp);

            }, onError);

             };
           // self.LoadWeekMenu();
        //app.su_Service.LoadWeekMenu()

        app.su_Service.IsNextWeekMenuExist().then(function (respnext) {
            self.IsNextWeekMenuExist(result);
        }, onError);
    }

        self.init();

    ko.applyBindings( new weekMenuModel());


})();
