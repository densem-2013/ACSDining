/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Scripts/knockout-3.2.0.js" />
/// <reference path="~/Areas/AdminArea/Content/scripts/app.service.js" />
/// <reference path="~/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/SU_Area/Content/scripts/app.su_Service.js" />
/// <reference path="~/Content/app/jquery-1.10.2.js" />
(function () {

    $("#infoTitle span").attr({ 'data-bind': "text: WeekTitle" })
        .css({ 'background': 'rgba(119, 222, 228, 0.61)', 'color': 'rgb(232, 34, 208)', 'border': '3px solid rgb(50, 235, 213)' });



    var quantValueModel = function(value,canchange) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Quantity = ko.observable(value);
        self.CanBeChanged = ko.observable(/*canchange*/true);
        self.Store = ko.observable();
        self.beenChanged = ko.observable(false);
        self.clicked = function (item) {
            if (self.CanBeChanged()) {
                $(item).focusin();
            }
        };
        self.doubleClick = function () {
            if (self.CanBeChanged()) {
                self.beenChanged(false);
                self.Store(self.Quantity());
                self.isEditMode(true);
            }
        };
        self.onFocusOut = function () {
            if (self.CanBeChanged()) {
                self.beenChanged(self.Store() !== self.Quantity());
                self.isEditMode(false);
            }
        };
    }

    var dishInfo = function(dinfo, canbechanged, quantity) {

        var self = this;

        self.DishId = ko.observable(dinfo.dishID);
        self.Title = ko.observable(dinfo.title);
        self.ProductImage = ko.observable(dinfo.productImage);
        self.Price = ko.observable(dinfo.price.toFixed(2));
        self.Category = ko.observable(dinfo.category);

        self.OrderQuantity = ko.observable(new quantValueModel(quantity, canbechanged));
    }

    var menuForDay = function(dayobj, categs) {

        var self = this;

        dayobj = dayobj || {};
        categs = categs || [];

        self.ID = ko.observable(dayobj.id);
        self.DayOfWeek = ko.observable(dayobj.menuForDay.dayOfWeek);
        self.Dishes = ko.observableArray(ko.utils.arrayMap(categs, function(item) {
            var ind = 0;
            var catind;
            var first = ko.utils.arrayFirst(dayobj.menuForDay.dishes, function(element) {
                var positivRes = element.category === item;
                if (positivRes) {
                    catind = ind;
                }
                ind++;
                return positivRes;
            });
            if (first != null && catind != null) {
                return new dishInfo(first, dayobj.quantCanBeChanged, dayobj.dishQuantities[catind]);
            }
            return null;

        }));

    }
    var userDayOrderInfo = function(dayOrdObject, categories) {

        var self = this;

        dayOrdObject = dayOrdObject || {};
        categories = categories || [];

        self.ID = ko.observable(dayOrdObject.id);
        self.DayOfWeek = ko.observable(dayOrdObject.dayOfWeek);

        self.MenuForDay = ko.observable(new menuForDay(dayOrdObject,categories));

        self.OrderCanBeChanged = ko.observable(dayOrdObject.orderCanBeChanged);
        self.DayOrderSummary = ko.observable(dayOrdObject.dayOrderSummary.toFixed(2));
        self.CalcDayOrderTotal = function() {
            var sum = 0;
            var valsum;
            for (var i = 0; i < self.MenuForDay().Dishes().length; i++) {
                var dish = self.MenuForDay().Dishes()[i];
                valsum = parseFloat(dish.Price() * dish.OrderQuantity().Quantity());
                sum += valsum;
            };
            self.DayOrderSummary(sum.toFixed(2));
        };
    }

    var weekUserOrderModel = function() {

        var self = this;

        self.UserId = ko.observable();

        self.OrderId = ko.observable();

        self.CurrentWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.WeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));


        self.UserDayOrders = ko.observableArray([]);


        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.Categories = ko.observableArray([]);

        self.WeekSummaryPrice = ko.observable();
        
        self.WeekIsPaid = ko.observable();

        self.CanCreateOrderOnNextWeek = ko.observable();
        
        self.IsNextWeekYear = ko.observable(false);

        self.FirstCourseValues = [0, 0.5, 1, 2, 3, 4, 5];

        self.QuantValues = [0, 1, 2, 3, 4, 5];

        self.BeenChanged = ko.observable(false);

        self.IsCurrentWeek = ko.observable();

        self.NextWeekOrderExist = ko.observable();

        // Callback for error responses from the server.
        function onError(error) {
            self.Message("Error: " + error.status + " " + error.statusText);
        }

        self.WeekTitle = ko.computed(function() {
            var options = {
                weekday: "short",
                year: "numeric",
                month: "short",
                day: "numeric"
            };

            var needed = self.WeekYear();
            if (needed !== null && needed !== undefined) {

                var year = needed.year;
                var firstDay = new Date(year, 0, 1).getDay();

                var week = self.WeekYear().week;
                var d = new Date("Jan 01, " + year + " 01:00:00");
                var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week - 1);
                var n1 = new Date(w);
                var n2 = new Date(w + 345600000);
                return "Неделя " + week + ", " + n1.toLocaleDateString("ru-RU", options) + " - " + n2.toLocaleDateString("ru-RU", options);
            } else {
                return null;
            }
        }.bind(self));

        
        self.SetMyDateByWeek = function (wyDto) {
            var firstDay = new Date(wyDto.year, 0, 1).getDay();
            var d = new Date("Jan 01, " + wyDto.year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (wyDto.week - 1);
            self.myDate(new Date(w));
        }.bind(self);


        var loadUserWeekOrder = function(wyDto) {

            app.su_Service.LoadUserWeekOrder(wyDto).then(function(resp) {
                self.UserDayOrders([]);

                self.OrderId(resp.orderId);
                self.UserId(resp.userId);
                self.WeekIsPaid(resp.weekIsPaid);
                self.WeekYear(resp.weekYear);
                self.WeekSummaryPrice(resp.weekSummaryPrice.toFixed(2));
                ko.utils.arrayForEach(resp.dayOrderDtos, function(object) {

                    self.UserDayOrders.push(new userDayOrderInfo(object, self.Categories(), object.orderCanBeChanged));

                });

                app.su_Service.IsNextWeekYear(wyDto).then(function (resp) {
                    self.IsNextWeekYear(resp);
                });

                var res = self.CurrentWeekYear().week === self.WeekYear().week && self.CurrentWeekYear().year === self.WeekYear().year;

                self.IsCurrentWeek(res);

            }, onError);

        }
        
        self.NextWeekOrder = function() {
            var weekYear = new WeekYear(self.WeekYear());
            app.su_Service.GetNextWeekYear(weekYear).then(function(resp) {

                self.SetMyDateByWeek(resp);

            }, onError);

        }
        self.PrevWeekOrder = function() {
            var weekYear = new WeekYear(self.WeekYear());
            app.su_Service.GetPrevWeekYear(weekYear).then(function(resp) {

                self.SetMyDateByWeek(resp);

            }, onError);

        }
        self.GoToNextWeekOrder = function() {
            
            app.su_Service.GetNextWeekYear(self.CurrentWeekYear()).then(function (nextWeekYear) {

                self.SetMyDateByWeek(nextWeekYear);
            });
        };

        self.myDate.subscribe = ko.computed(function() {
            var takedWeek = self.myDate().getWeek() + 1;
            var needObj = self.WeekYear();
            if (needObj != undefined) {
                var curweek = needObj.Week;
                if (takedWeek !== curweek) {
                    var weekyear = {
                        Week: takedWeek,
                        Year: self.myDate().getFullYear()
                    };
                    if (!isNaN(weekyear.Week)  && !isNaN(weekyear.Year) ) {

                        loadUserWeekOrder(weekyear);
                    }
                };
            }
        }, self);

        self.GetCurrentWeekYear = function() {

            app.su_Service.GetCurrentWeekYear().then(function(resp) {

                self.CurrentWeekYear(resp);

            }, onError);
        }

        self.DeleteNextWeekOrder = function() {
            var menuid = self.OrderId();
            app.su_Service.DeleteNextWeekOrder(menuid).then(function () {

                self.SetMyDateByWeek(self.CurrentWeekYear());
            }, onError);
        }


        self.CalcSummary = function (beenchanged) {
            if (beenchanged) {

                self.BeenChanged(true);
            }
            var sum = 0;

            for (var ind = 0; ind < self.UserDayOrders().length; ind++) {

                self.UserDayOrders()[ind].CalcDayOrderTotal();

                sum += parseFloat(self.UserDayOrders()[ind].DayOrderSummary());
            }

            self.WeekSummaryPrice(sum.toFixed(2));
        };

        self.update = function () {
            var userweekorder = {
                UserId: self.UserId(),
                OrderId: self.OrderId(),
                DayOrderDtos: self.UserDayOrders(),
                WeekSummaryPrice: self.WeekSummaryPrice(),
                WeekIsPaid: self.WeekIsPaid(),
                WeekYearDto:self.WeekYear()
            };

            app.su_Service.UserWeekUpdateOrder(userweekorder).then(function () {
                self.BeenChanged(false);
            }, onError());
        };

        self.init = function() {
            app.su_Service.GetCategories().then(function(resp) {
                self.Categories([]);
                self.Categories.pushAll(resp);
            }, onError);

            self.GetCurrentWeekYear();

            app.su_Service.NextWeekOrderExists().then(function (respnext) {
                self.NextWeekOrderExist(respnext);
            }, onError);

            self.SetMyDateByWeek(self.CurrentWeekYear());

        }

        self.init();

    };

    ko.applyBindings(new weekUserOrderModel());
}());

