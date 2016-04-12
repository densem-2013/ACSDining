/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Scripts/knockout-3.2.0.js" />
/// <reference path="~/Areas/AdminArea/Content/scripts/app.service.js" />
/// <reference path="~/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/SU_Area/Content/scripts/app.su_Service.js" />
/// <reference path="~/Content/app/jquery-1.10.2.js" />
(function () {

    $("#infoTitle span").attr({ 'data-bind': "text: WeekTitle" });

    var quantValueModel = function(value,canchange) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Quantity = ko.observable(value);
        self.CanBeChanged = ko.observable(canchange);
        self.clicked = function (item) {
            if (self.CanBeChanged()) {

                $(item).focusin();
            }
        };
        self.doubleClick = function () {
            if (self.CanBeChanged()) {
                this.isEditMode(true);
            }
        };
        self.onFocusOut = function () {
            if (self.CanBeChanged()) {
                this.isEditMode(false);
            }
        };
    }

    var dishInfo = function(dinfo, canbechanged) {

        var self = this;

        self.DishId = ko.observable(dinfo.dishID);
        self.Title = ko.observable(dinfo.title);
        self.ProductImage = ko.observable(dinfo.productImage);
        self.Price = ko.observable(dinfo.price.toFixed(2));
        self.Category = ko.observable(dinfo.category);
        self.OrderQuantity = ko.observable(new quantValueModel(dinfo.quantity, canbechanged));
    }

    var menuForDayInfo = function(object, categs, canquantchange) {

        var self = this;

        object = object || {};
        categs = categs || [];

        self.ID = ko.observable(object.id);
        self.DayOfWeek = ko.observable(object.dayOfWeek);
        var ind = 0;
        self.Dishes = ko.observableArray(ko.utils.arrayMap(categs, function(item) {
            var first = ko.utils.arrayFirst(object.dishes, function(element) {

                return element.category === item;
            });
            if (first != null) {
                return new dishInfo(first, canquantchange);
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
            var valsum;
            for (var i = 0; i < self.Dishes().length; i++) {

                valsum = parseFloat(self.Dishes()[i].Price());
                sum += valsum;
            };


            self.TotalPrice(sum.toFixed(2));

        }.bind(self));
    }

    var weekUserOrderModel = function() {

        var self = this;

        self.UserId = ko.observable();

        self.OrderId = ko.observable();

        self.CurrentWeekYear = ko.observable();

        self.WeekYear = ko.observable();


        self.MFD_models = ko.observableArray([]);


        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.Categories = ko.observableArray();

        self.SummaryPrice = ko.observable();

        self.NumbersWeeks = ko.observableArray();
        self.WeekIsPaid = ko.observable();

        self.CanCreateOrderOnNextWeek = ko.observable();
        
        self.IsNextWeekYear = ko.observable(false);

        self.IsNextWeekYear.subscribe = ko.computed(function() {

            var cur = self.WeekYear();
            app.su_Service.IsNextWeekYear(cur).then(function(resp) {
                self.IsNextWeekYear(resp);
            });
        });


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

            var needed = ko.mapping.toJS(self.WeekYear);
            if (needed !== undefined) {

                var year = needed.Year;
                var firstDay = new Date(year, 0, 1).getDay();

                var week = self.WeekYear().Week;
                var d = new Date("Jan 01, " + year + " 01:00:00");
                var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week - 1);
                var n1 = new Date(w);
                var n2 = new Date(w + 345600000);
                return "Неделя " + week + ", " + n1.toLocaleDateString("ru-RU", options) + " - " + n2.toLocaleDateString("ru-RU", options);
            } else {
                return null;
            }
        }.bind(self));


        //self.loadWeekNumbers = function() {
        //    app.su_Service.LoadUserOrderWeekNumbers().then(function(resp) {
        //        self.NumbersWeeks([]);
        //        for (var i = 0; i < resp.length; i++) {

        //            self.NumbersWeeks.push(resp[i]);

        //        };
        //    }, onError);
        //};


        self.SetMyDateByWeek = function(wyDto) {
            var firstDay = new Date(wyDto.year, 0, 1).getDay();
            var d = new Date("Jan 01, " + wyDto.year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (wyDto.week - 1);
            self.myDate(new Date(w));
        }.bind(self);

        var loadUserWeekOrder = function(wyDto) {

            app.su_Service.LoadUserWeekOrder(wyDto).then(function(resp) {
                self.MFD_models([]);

                self.OrderId(resp.orderId);
                self.UserId(resp.userId);
                self.WeekIsPaid(resp.weekIsPaid);
                self.WeekYear(resp.weekYear);
                ko.utils.arrayForEach(resp.mfD_models, function(object) {

                    self.MFD_models.push(new menuForDayInfo(object, self.Categories()));

                });

            }, onError);

        }

        self.LoadUserWeekOrder = function(wyDto) {

            loadUserWeekOrder(wyDto);
        }

        self.NextWeekOrder = function() {
            var weekYear = new WeekYearModel(self.WeekYear());
            app.su_Service.GetNextWeekYear(weekYear).then(function(resp) {

                self.SetMyDateByWeek(resp);

            }, onError);

        }
        self.PrevWeekOrder = function() {
            var weekYear = new WeekYearModel(self.WeekYear());
            app.su_Service.GetPrevWeekYear(weekYear).then(function(resp) {

                self.SetMyDateByWeek(resp);
            }, onError);

        }
        self.GoToNextWeekOrder = function() {

            var curWeekYear = new WeekYearModel(self.CurrentWeekYear());

            app.su_Service.GetNextWeekYear(curWeekYear).then(function(nextWeekYear) {
                self.loadWeekNumbers();
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
                    self.LoadUserWeekOrder(new WeekYear(weekyear));
                }
            }
        }, self);

        self.GetCurrentWeekYear = function() {

            app.su_Service.GetCurrentWeekYear().then(function(resp) {

                self.CurrentWeekYear(resp);

            }, onError);
        }

        self.IsCurrentWeek = ko.computed(function () {

            var res=self.CurrentWeekYear._isEqual(self.WeekYear);
            console.log("res= " + res);
            return res;

        }.bind(self));


        self.DeleteNextWeekOrder = function() {
            var menuid = self.OrderId();
            app.su_Service.DeleteNextWeekOrder(menuid).then(function() {
                self.LoadUserWeekOrder();
                //self.loadWeekNumbers();
                self.SetMyDateByWeek(self.CurrentWeekYear());
            }, onError);
        }

        self.CalcTotal = function() {

            var sum = 0;

            for (var ind = 0; ind < self.MFD_models().length; ind++) {

                sum += parseFloat(self.MFD_models()[ind].TotalPrice());
            }

            this.SummaryPrice(sum.toFixed(2));

        }.bind(self);

        self.MFD_models.subscribe = ko.computed(self.CalcTotal, self);

        self.init = function() {
            app.su_Service.GetCategories().then(function(resp) {
                self.Categories([]);
                self.Categories.pushAll(resp);
            }, onError);

            //self.loadWeekNumbers();
            self.GetCurrentWeekYear();
            self.LoadUserWeekOrder();
        }
        self.init();
    };

    ko.applyBindings(new weekUserOrderModel());
}());

