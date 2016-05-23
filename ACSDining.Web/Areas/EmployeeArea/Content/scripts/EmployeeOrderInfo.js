/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Scripts/knockout-3.2.0.js" />
/// <reference path="~/Areas/AdminArea/Content/scripts/app.service.js" />
/// <reference path="~/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/SU_Area/Content/scripts/app.su_Service.js" />
/// <reference path="~/Content/app/jquery-1.10.2.js" />
(function() {

    $("#infoTitle span").attr({ 'data-bind': "text: WeekTitle" })
        .css({ 'background': 'rgba(119, 222, 228, 0.61)', 'color': 'rgb(232, 34, 208)', 'border': '3px solid rgb(50, 235, 213)' });


    var quantValueModel = function(value) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Quantity = ko.observable(value);
        self.Store = ko.observable();
        self.beenChanged = ko.observable(false);
        self.clicked = function(item) {
                $(item).focusin();
        };

        self.onmouseenter = function() {
                self.beenChanged(false);
                self.Store(self.Quantity());
                self.isEditMode(true);
        };

        self.onFocusOut = function() {
                self.beenChanged(self.Store() !== self.Quantity());
                self.isEditMode(false);
        };
    }

    var dishInfo = function (dinfo, quantity) {

        var self = this;

        self.Title = ko.observable(dinfo.title||"-------------");
        //self.ProductImage = ko.observable(dinfo.productImage);
        self.Price = ko.observable(dinfo.price.toFixed(2));
        self.Category = ko.observable(dinfo.category);
        self.Description = ko.observable(dinfo.description);
        self.OrderQuantity = ko.observable(new quantValueModel(quantity));
    }

    var userDayOrderInfo = function (dayOrdObject, dishQuantities) {

        var self = this;

        dayOrdObject = dayOrdObject || {};

        self.DayOrderId = ko.observable(dayOrdObject.dayOrdId);
        self.OrderCanByChanged = ko.observable(dayOrdObject.orderCanByChanged);
        self.Dishes = ko.observableArray(ko.utils.arrayMap(dayOrdObject.dishes, function(obj,index) {
            return new dishInfo(obj, dishQuantities[index]);
        }));

        self.DayOrderSummary = ko.observable(dayOrdObject.dayOrderSummary.toFixed(2));

        self.CalcDayOrderTotal=function () {

            var sum = 0;

            ko.utils.arrayForEach(self.Dishes(), function(dish) {
                sum += parseFloat(dish.Price() * dish.OrderQuantity().Quantity());
            });
            return self.DayOrderSummary(sum.toFixed(2));
        };

     }

    var weekUserOrderModel = function() {

        var self = this;
        
        self.OrderId = ko.observable();

        self.CurrentWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.WeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.NextWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.UserDayOrders = ko.observableArray([]);
        self.Title = ko.observable("");
        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.Categories = ko.observableArray([]);

        self.WeekSummaryPrice = ko.observable();

        self.WeekPaid = ko.observable();

        self.WeekIsPaid = ko.observable();

        self.CanCreateOrderOnNextWeek = ko.observable();

        self.IsNextWeekYear = ko.pureComputed(function () {
            var res = self.NextWeekYear().week === self.WeekYear().week && self.NextWeekYear().year === self.WeekYear().year;
            return res;

        }.bind(self));

        self.FirstCourseValues = [0, 0.5, 1, 2, 3, 4, 5];

        self.QuantValues = [0, 1, 2, 3, 4, 5];
        
        self.DaysOfWeek = ko.observableArray([]);

        self.Balance = ko.observable();

        self.BeenChanged = ko.observable(false);
        
        self.IsCurrentWeek = ko.pureComputed(function () {

            var res = self.CurrentWeekYear().week === self.WeekYear().week && self.CurrentWeekYear().year === self.WeekYear().year;
            return res;

        }.bind(self));

        self.NextWeekOrderExist = ko.observable();
        
        // Callback for error responses from the server.
        function modalShow(title, message) {

            self.Title(title);
            self.Message(message);
            $("#modalMessage").modal("show");

        }
        // Callback for error responses from the server.
        function onError(error) {

            modalShow("Внимание, ошибка! ", "Error: " + error.status + " " + error.statusText);
        }

        self.SetMyDateByWeek = function (wyDto) {
            var firstDay = new Date(wyDto.year, 0, 1).getDay();
            var d = new Date("Jan 01, " + wyDto.year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (wyDto.week);
            self.myDate(new Date(w));
        }.bind(self);

        self.WeekTitle =  ko.computed(function () {
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



        var loadUserWeekOrder = function(wyDto1) {

            app.su_Service.LoadUserWeekOrder(wyDto1).then(function (resp) {
                if (resp) {
                    self.OrderId(resp.weekOrderId);
                    self.WeekIsPaid(resp.weekIsPaid);
                    self.WeekYear(resp.weekYear);
                    self.DaysOfWeek(resp.dayNames);
                    var summary = resp.weekOrderDishes.pop();
                    self.WeekSummaryPrice(summary.toFixed(2));
                    self.Balance(resp.balance);
                    self.UserDayOrders(ko.utils.arrayMap(resp.dayOrders, function (object, index) {
                        var dishcount = object.dishes.length;
                        var start = resp.weekOrderDishes.slice(index * dishcount, (index + 1) * dishcount);
                        return new userDayOrderInfo(object, start);

                    }));
                } else {
                    modalShow("Сообщение","На выбранную Вами дату не было создано меню для заказа. Будьте внимательны!");
                }


            }, onError);

        }

        self.GoToNextWeekOrder = function () {

            self.SetMyDateByWeek(self.NextWeekYear());
            
        };


        self.GoToCurrentWeekOrder = function () {

            self.SetMyDateByWeek(self.CurrentWeekYear());

        };

        self.CalcSummary = function(beenchanged) {
            if (beenchanged) {

                self.BeenChanged(true);
            }
            var sum = 0;

            ko.utils.arrayForEach(self.UserDayOrders(), function(item) {
                item.CalcDayOrderTotal();
                sum += parseFloat(item.DayOrderSummary());
            });

            self.WeekSummaryPrice(sum.toFixed(2));
        };

        self.update = function (dayord, catnumber, quantity) {

            var userweekorder = {
                DayOrderId: dayord.DayOrderId(),
                CategoryId: catnumber,
                Quantity: quantity
            };

            self.CalcSummary();

            app.su_Service.UserWeekUpdateOrder(userweekorder).then(function (res) {
                if (res) {
                    self.BeenChanged(false);
                }
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

                        loadUserWeekOrder(weekyear);
                    }
                };
            };
        }, self);


        self.init = function () {

            app.su_Service.GetCategories().then(function (resp) {
                self.Categories(resp);
                app.su_Service.GetCurrentWeekYearForEmployee().then(function (resp) {
                    self.CurrentWeekYear(resp);

                }, onError)
                .then(function () {
                    self.SetMyDateByWeek(self.CurrentWeekYear());
                }
            );
            }, onError);

            app.su_Service.GetUserNextWeekYear().then(function (nextWeekYear) {
                self.NextWeekYear(nextWeekYear);
            });

            app.su_Service.CanCreateOrderOnNextWeek().then(function (cancreatenext) {
                self.CanCreateOrderOnNextWeek(cancreatenext);
            }, onError);


        };
        self.init();

    }

    ko.applyBindings(new weekUserOrderModel());

}());

