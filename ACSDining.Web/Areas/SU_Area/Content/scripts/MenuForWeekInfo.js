/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Areas/SU_Area/Content/scripts/app.su_Service.js" />
/// <reference path="~/Scripts/knockout-3.2.0.js" />
/// <reference path="~/Scripts/bootstrap.js" />
/// <reference path="~/Scripts/knockout-3.3.0.debug.js" />
/// <reference path="~/Scripts/knockout.mapping-latest.debug.js" />
/// <reference path="~/Content/app/jquery-1.10.2.js" />
(function() {

    $("#infoTitle span").attr({ 'data-bind': "text: WeekTitle" })
        .css({ 'background': "rgba(119, 222, 228, 0.61)", 'color': "rgb(232, 34, 208)", 'border': "3px solid rgb(50, 235, 213)" });

    $("ul.nav.navbar-nav li:first-child").addClass("active");

    var dishInfo = function(dinfo) {

        var self = this;

        self.DishId = ko.observable(dinfo.dishId);
        self.Title = ko.observable(dinfo.title);
        self.ProductImage = ko.observable(dinfo.productImage);
        self.Price = ko.observable(dinfo.price.toFixed(2));
        self.Category = ko.observable(dinfo.category);

        self.UpdateWeekMenu = function(dishupdate) {
            this.DishId(dishupdate.DishId());
            this.Title(dishupdate.Title());
            this.ProductImage(dishupdate.ProductImage());
            this.Price(dishupdate.Price());

        };
        this.isHovering = ko.observable(false);
    }

    var menuForDayInfo = function(object, categs) {

        var self = this;

        object = object || {};
        categs = categs || [];

        self.Id = ko.observable(object.id);
        self.DayOfWeek = ko.observable(object.dayOfWeek);
        var ind = 0;
        self.Dishes = ko.observableArray(ko.utils.arrayMap(categs, function(item) {
            var first = ko.utils.arrayFirst(object.dishes, function(element) {

                return element.category === item;
            });
            if (first != null) {
                return new dishInfo(first);
            }
            var dish = {
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
        self.Dishes.subscribe = ko.computed(function() {
            var sum = 0;

            ko.utils.arrayForEach(self.Dishes(), function(dish) {
                sum += parseFloat(dish.Price());
            });

            self.TotalPrice(sum.toFixed(2));

        }.bind(self));

        //На меню уже был сделан заказ
        self.OrderWasBooking = ko.observable(object.orderWasBooking);
    }

    var workDayInfo = function(workday) {
        var self = this;
        self.WorkDayId = ko.observable(workday.workDayId);
        self.IsWorking = ko.observable(workday.isWorking);
        self.DayNumber = ko.observable(workday.dayNumber);
        self.DayName = ko.observable(workday.dayName);
    }

    var workWeekModel = function(weekmodel) {
        var self = this;

        self.WorkweekId = ko.observable(weekmodel.workWeekId);
        self.WorkingDays = ko.observableArray(ko.utils.arrayMap(weekmodel.workDays, function(workday) {
            return new workDayInfo(workday);
        }));
        self.CanBeChanged = ko.observable();
    }

    var weekMenuModel = function() {
        var self = this;

        self.MenuId = ko.observable();

        self.CurrentWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.WeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));


        self.MFD_models = ko.observableArray([]);

        self.UpdatedDayMenus = ko.observableArray([]);

        self.Title = ko.observable("");
        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.DishesByCategory = ko.observableArray([]);

        self.Category = ko.observable();

        self.Categories = ko.observableArray();

        self.SelectedDish = ko.observable();

        self.UpdatableMFD = ko.observable();

        self.SummaryPrice = ko.observable();

        self.IsNextWeekMenuExists = ko.observable();
        
        self.IsNextWeekYear = ko.observable(false);

        self.WorkingWeek = ko.observable();

        self.OrderCanBeCreated = ko.observable();

        //Сигнализирует, что есть пользователи, которым нужно отправить сообщение об изменении меню
        self.ForEmailExistsObject = ko.pureComputed(function() {
            return self.UpdatedDayMenus().length > 0;
        }, self);

        self.IsCurrentWeekYear = ko.pureComputed(function() {

            return self.CurrentWeekYear().week === self.WeekYear().week && self.CurrentWeekYear().year === self.WeekYear().year;

        }, self);

        self.BeenChanged = ko.observable(false);

        self.ChangeSaved = ko.observable(false);

        // Callback for error responses from the server.
        function onError(error) {
            self.Title("Внимание, ошибка! ");
            self.Message("Error: " + error.status + " " + error.statusText);
            $("#modalMessage").modal("show");
        }

        self.NextWeekMenuButton = {
            Message: function() { return self.IsNextWeekYear() ? "Удалить меню на следующую неделю" : (self.IsNextWeekMenuExists() ? "Перейти к меню на следующую неделю" : "Создать меню на следующую неделю") },
            action: function() { return  self.IsNextWeekYear() ? self.DeleteNextWeekMenu : (self.IsNextWeekMenuExists() ? self.GoToNextWeekMenu : self.CreateNextWeekMenu) },
            cssclass: function() { return self.IsNextWeekYear() ? "btn-danger" : "btnaddmenu enabled" }
        };

        self.WeekTitle = ko.computed(function() {
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


        self.loadDishes = function(dish) {
            app.su_Service.DishesByCategory(dish.Category()).then(function(resp) {

                var disharray = ko.utils.arrayMap(resp, function(catdish) {
                    if (catdish.dishId === dish.DishId()) {
                        self.SelectedDish(catdish.dishId);
                    };
                    return new dishInfo(catdish);
                });
                self.DishesByCategory(disharray);

            }, onError);
        }

        self.showDishes = function(searchdish, index) {
            self.UpdatableMFD(index);
            self.Category(searchdish.Category());
            self.loadDishes(searchdish);


            $("#modalbox").modal("show");
        }

        self.changeSelected = function(clikedItem) {
            if (self.SelectedDish() === clikedItem.DishId()) {
                self.BeenChanged(true);
                self.SelectedDish(clikedItem.DishId());
            }
            return true;
        }

        self.SetMyDateByWeek = function(wyDto) {
            var firstDay = new Date(wyDto.year, 0, 1).getDay();
            var d = new Date("Jan 01, " + wyDto.year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (wyDto.week);
            self.myDate(new Date(w));
        }.bind(self);

        var loadWeekMenu = function(wyDto) {

            app.su_Service.LoadWeekMenu(wyDto).then(function(resp) {

                self.MFD_models(ko.utils.arrayMap(resp.mfdModels, function(item) {
                    return new menuForDayInfo(item, self.Categories());
                }));

                self.MenuId(resp.id);
                self.WeekYear(resp.workWeek.weekYear);
                self.SummaryPrice(resp.summaryPrice);
                self.OrderCanBeCreated(resp.orderCanBeCreated);
                self.WorkingWeek(new workWeekModel(resp.workWeek));

                app.su_Service.IsNextWeekYear(wyDto).then(function(resp2) {
                    self.IsNextWeekYear(resp2);
                });

            }, onError);

        }


        self.GoToNextWeekMenu = function() {

            app.su_Service.GetNextWeekYear(self.CurrentWeekYear()).then(function(nextWeekYear) {

                self.SetMyDateByWeek(nextWeekYear);

            });
        };

        self.myDate.subscribe = ko.computed(function() {
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
        
        self.applyChanges = function() {

            var catIndex = ko.utils.arrayFirst(ko.utils.arrayMap(self.Categories(), function(cat, ind) {
                if (cat === self.Category()) {

                    return ind;
                }
                return null;
            }), function(elem) {
                return elem !== null;
            });

            var dish = ko.utils.arrayFirst(self.DishesByCategory(), function(value) {
                return value.DishId() === self.SelectedDish();
            });
            var upateIndex = self.UpdatableMFD();

            var mfdupdate = self.MFD_models()[upateIndex];
            //Если на меню уже был сделан заказ, сохраняем id
            if (mfdupdate.OrderWasBooking()) {

                var exists = ko.utils.arrayFirst(self.UpdatedDayMenus(), function(item) {
                    return item === mfdupdate.Id();
                });
                if (exists==null) {
                    self.UpdatedDayMenus.push(mfdupdate.Id());
                };

            }

            if (dish != undefined) {
                var dishupdate = mfdupdate.Dishes()[catIndex];
                var dishUpObject = ko.mapping.toJS(dishupdate);
                dishupdate.UpdateWeekMenu(dish);
                self.CalcTotal();
            };

            self.save(mfdupdate);

            $("#modalbox").modal("hide");
        }

        self.DeleteNextWeekMenu = function() {
            var menuid = self.MenuId();
            app.su_Service.DeleteNextWeekMenu(menuid).then(function () {

                app.su_Service.IsNextWeekMenuExists().then(function (respnext) {
                    self.IsNextWeekMenuExists(respnext);
                }, onError);

                self.SetMyDateByWeek(self.CurrentWeekYear());
            }, onError);
        }

        self.CreateNextWeekMenu = function() {
            app.su_Service.CreateNextWeekMenu().then(function(res) {
                if (res) {

                    app.su_Service.IsNextWeekMenuExists().then(function (respnext) {
                        self.IsNextWeekMenuExists(respnext);
                    }, onError);

                    app.su_Service.GetNextWeekYear(self.CurrentWeekYear()).then(function(nextDto) {
                        self.SetMyDateByWeek(nextDto);
                    }, onError);
                }
            });
        };

        self.CalcTotal = function() {

            var sum = 0;
            ko.utils.arrayForEach(self.MFD_models(), function(item) {

                sum += parseFloat(item.TotalPrice());

            });

            this.SummaryPrice(sum.toFixed(2));

        }.bind(self);

        self.MFD_models.subscribe = ko.computed(self.CalcTotal, self);


        self.SendBooking = function() {

            var menuUpdate = {
                DateTime: self.WeekTitle(),
                Message: self.Message(),
                UpdatedDayMenu: self.UpdatedDayMenus()
            };

            app.su_Service.SendMenuUpdateMessage(menuUpdate).then(function() {
                self.UpdatedDayMenus([]);
            }, onError);
        };
        self.SetAsOrderable=function() {

            var orderablemessage = {
                DateTime: self.WeekTitle()
            };
            app.su_Service.SetAsOrderable(orderablemessage).then(function(res) {
                self.OrderCanBeCreated(res);
            }, onError);
        }

        self.init = function() {
            app.su_Service.GetCategories().then(function(resp) {
                self.Categories([]);
                self.Categories.pushAll(resp);
            }, onError);

            //self.loadWeekNumbers();

            app.su_Service.GetCurrentWeekYear().then(function(resp) {

                self.CurrentWeekYear(resp);

            }, onError);

            app.su_Service.IsNextWeekMenuExists().then(function(respnext) {
                self.IsNextWeekMenuExists(respnext);
            }, onError);

        };
        self.init();

    }


    ko.applyBindings(new weekMenuModel());


})();
