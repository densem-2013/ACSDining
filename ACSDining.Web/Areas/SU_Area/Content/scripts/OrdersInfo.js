/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Scripts/jquery-ui-i18n.min.js" />
/// <reference path="~/Areas/AdminArea/Content/scripts/app.service.js" />
/// <reference path="~/Scripts/knockout-3.3.0.debug.js" />
/// <reference path="~/Areas/SU_Area/Content/scripts/app.su_Service.js" />
(function () {

    $("#infoTitle span").attr({ 'data-bind': "text: WeekTitle" })
        .css({ 'background': "rgba(119, 222, 228, 0.61)", 'color': "rgb(232, 34, 208)", 'border': "3px solid rgb(50, 235, 213)" });

    
    $("ul.nav.navbar-nav li:nth-child(2)").addClass("active");
    $("#autorizeMessage span").css({ 'paddingLeft': "160px" });
    //$(".container").css({ 'marginLeft': 0 });

    var quantValueModel = function (value) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Quantity = ko.observable(value);
        self.Store = ko.observable();

        self.beenChanged = ko.observable(false);
        self.clicked = function (item, canchange) {
            if (!canchange) return;
            $(item).focusin();
        };

        self.onmouseenter = function (canchange) {
            if (!canchange) return;
            self.beenChanged(false);
            self.Store(self.Quantity());
            self.isEditMode(true);
        };

        self.onFocusOut = function (canchange) {
            if (!canchange) return;
            self.beenChanged(self.Store() !== self.Quantity());
            self.isEditMode(false);
        };
    }

    var userDayOrderInfo = function (dayOrdObject, dishQuantities) {

        var self = this;

        dayOrdObject = dayOrdObject || {};

        self.DayOrderId = ko.observable(dayOrdObject.dayOrderId);

        self.OrderCanBeChanged = ko.observable(dayOrdObject.orderCanBeChanged);

        self.DishQuantities = ko.observableArray(ko.utils.arrayMap(dishQuantities, function (item) {
            return new quantValueModel(item, dayOrdObject.orderCanBeChanged);
        }));

    }

    var weekUserOrderModel = function (userWeekOrder,catlength) {

        var self = this;

        self.UserId = ko.observable(userWeekOrder.userId);

        self.OrderId = ko.observable(userWeekOrder.orderId);

        self.UserName = ko.observable(userWeekOrder.userName);

        self.UserDayOrders = ko.observableArray(ko.utils.arrayMap(userWeekOrder.dayOrderDtos, function (item, index) {
            var dishQuants = userWeekOrder.userWeekOrderDishes.slice(index * catlength, (index + 1) * catlength );
            return new userDayOrderInfo(item, dishQuants);
        }));
        self.WeekSummaryPrice = ko.observable(userWeekOrder.weekSummaryPrice.toFixed(2));//.extend({ numeric: 2 });

        self.BeenChanged = ko.observable(false);


        self.WeekPaid = ko.observable(userWeekOrder.weekPaid);

        self.WeekIsPaid = ko.observable(userWeekOrder.weekIsPaid);
    };

    var weekOrdersModel = function () {
        var self = this;

        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.CurrentWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.WeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.Categories = ko.observableArray([]);

        self.WeekUserOrderModels = ko.observableArray([]);

        self.FirstCourseValues = [0, 0.5, 1, 2, 3, 4, 5];

        self.QuantValues = [0, 1, 2, 3, 4, 5];

        self.PageSizes = ko.pureComputed(function () {
            var res = [2, 5, 7, 10, 15, 20, 25];
            var all = self.WeekUserOrderModels().length;
            if (all>25) {
                res.push(all);
            }
            return res;

        });

        self.TotalCount = ko.observable();

        self.BeenChanged = ko.observable(false);

        self.SummaryDishQuantities = ko.observableArray([]);

        self.IsNextWeekYear = ko.observable();

        self.DaysOfWeek = ko.observableArray([]);

        self.WeekDishPrices = ko.observableArray([]);

        self.PlanWeekDishPrices = ko.observableArray([]);

        function onError(error) {
            self.Message("Error: " + error.status + " " + error.statusText);
        }

        self.CalcSummaryDishQuantyties = function(daynum,catnum) {

            var catlengh = self.Categories().length;

            var dayFilterArray = ko.utils.arrayMap(self.WeekUserOrderModels(), function(weekuserorder) {
                return ko.utils.arrayFirst(weekuserorder.UserDayOrders(), function(dayord, dnum) {
                    return dnum === daynum;
                });
            });
            var dayCatFilterArray = ko.utils.arrayMap(dayFilterArray, function(dayord, dn) {
                return ko.utils.arrayFirst(dayord.DishQuantities(), function(dqua, catind) {
                    return catind === catnum;
                });
            });
            var total = 0;
            ko.utils.arrayForEach(dayCatFilterArray, function(elem) {
                total += elem.Quantity();
            });
            self.SummaryDishQuantities.replace(self.SummaryDishQuantities()[catlengh * daynum + catnum],total);

            ko.utils.arrayForEach(self.WeekUserOrderModels(), function(weekuserorder) {
                if (self.BeenChanged()) {

                    var ordersum = 0;

                    var prices = self.WeekDishPrices();

                    ko.utils.arrayForEach(weekuserorder.UserDayOrders(), function(dayorder, dayindex) {
                        ko.utils.arrayForEach(dayorder.DishQuantities(), function (dishquant, quantindex) {

                            var quant = dishquant.Quantity();
                            ordersum +=  quant * prices[dayindex * catlengh + quantindex];

                        });

                    });

                    weekuserorder.WeekSummaryPrice(ordersum.toFixed(2));
                };
            });

        };


        var factloadWeekOrders = function (wyDto1) {

            app.su_Service.FactLoadWeekOrders(self.pageSize(),self.pageIndex(), wyDto1).then(function (resp1) {
                //self.WeekUserOrderModels([]);

                self.WeekYear(resp1.weekYearDto);
                self.DaysOfWeek([]);
                self.DaysOfWeek.pushAll(resp1.dayNames);
                self.SummaryDishQuantities([]);
                self.SummaryDishQuantities.pushAll(resp1.summaryDishQuantities);

                self.WeekUserOrderModels(ko.utils.arrayMap(resp1.userWeekOrders, function (uwoObject) {

                    return new weekUserOrderModel(uwoObject, self.Categories().length);

                }));

                self.WeekDishPrices(resp1.weekDishPrices);

                self.TotalCount(resp1.totalCount);

                app.su_Service.IsNextWeekYear(wyDto1).then(function (resp2) {
                    self.IsNextWeekYear(resp2);
                });

            }, onError);

        }

        self.update = function (wuOrder,daynumber,catnumder) {

            var weekorddishes = [];

            ko.utils.arrayForEach(wuOrder.UserDayOrders(), function (item) {

                ko.utils.arrayForEach(item.DishQuantities(), function (quant) {
                    weekorddishes.push(quant.Quantity());
                });


            });

            var userweekorder = {
                UserId: wuOrder.UserId(),
                OrderId: wuOrder.OrderId(),
                DayOrderDtos: wuOrder.UserDayOrders(),
                WeekSummaryPrice: wuOrder.WeekSummaryPrice(),
                WeekIsPaid: wuOrder.WeekIsPaid(),
                WeekPaid: wuOrder.WeekPaid(),
                WeekYear: self.WeekYear(),
                UserWeekOrderDishes: weekorddishes
            };

            self.CalcSummaryDishQuantyties(daynumber, catnumder);

            app.su_Service.UpdateOrder(userweekorder).then(function (res) {
                if (res) {
                    wuOrder.BeenChanged(false);
                }
            });
        };

        self.SetMyDateByWeek = function (wyDto) {
            var firstDay = new Date(wyDto.year, 0, 1).getDay();
            var d = new Date("Jan 01, " + wyDto.year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (wyDto.week);
            self.myDate(new Date(w));
        }.bind(self);

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

        self.myDate.subscribe = ko.computed(function () {
            var takedWeek = self.myDate().getWeek() - 1;
            var needObj = self.WeekYear();
            if (needObj != undefined && !isNaN(takedWeek)) {
                var curweek = needObj.week;
                if (!isNaN(takedWeek) && takedWeek !== curweek) {
                    var weekyear = {
                        Week: takedWeek,
                        Year: self.myDate().getFullYear()
                    };
                    if (!isNaN(weekyear.Week) && !isNaN(weekyear.Year)) {

                        factloadWeekOrders(weekyear);
                    }
                };
            };
        }, self);


        self.pageSize = ko.observable(7);

        self.pageIndex = ko.observable(0);

        //self.pagedList = ko.dependentObservable(function () {
        //    var size = self.pageSize();
        //    var start = self.pageIndex() * size;
        //    return self.WeekUserOrderModels.slice(start, start + size);
        //});

        self.maxPageIndex = ko.dependentObservable(function () {
            return Math.ceil(self.TotalCount() / self.pageSize()) - 1;
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
            for (var i = 0; i <= self.maxPageIndex() ; i++) {
                pages.push({ pageNumber: (i + 1) });
            }
            return pages;
        });

        self.moveToPage = function (index) {
            self.pageIndex(index);
        };

        self.init = function () {
            app.su_Service.GetCategories().then(function (resp) {
                self.Categories(resp);
            }, onError);

             app.su_Service.GetCurrentWeekYear().then(function (resp) {

                self.CurrentWeekYear(resp);

            }, onError);
            
            self.SetMyDateByWeek(self.CurrentWeekYear());
        }

        self.init();
    };

    ko.applyBindings(new weekOrdersModel());

})();

