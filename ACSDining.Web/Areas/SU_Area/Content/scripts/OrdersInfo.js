/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Scripts/jquery-ui-i18n.min.js" />
/// <reference path="~/Areas/AdminArea/Content/scripts/app.service.js" />
/// <reference path="~/Scripts/knockout-3.3.0.debug.js" />
/// <reference path="~/Areas/SU_Area/Content/scripts/app.su_Service.js" />
(function() {

    $("#infoTitle span").attr({ 'data-bind': "text: WeekTitle" })
        .css({ 'background': "rgba(119, 222, 228, 0.61)", 'color': "rgb(232, 34, 208)", 'border': "3px solid rgb(50, 235, 213)" });
    
    var quantValueModel = function (value, canchange) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Quantity = ko.observable(value);
        self.CanBeChanged = ko.observable(canchange/*true*/);
        self.Store = ko.observable();
        self.beenChanged = ko.observable(false);
        self.clicked = function (item) {
            if (self.CanBeChanged()) {
                $(item).focusin();
            }
        };

        self.onmouseenter = function () {
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

    //var dishInfo = function (dinfo, canbechanged, quantity) {

    //    var self = this;

    //    self.DishId = ko.observable(dinfo.dishID);

    //    self.OrderQuantity = ko.observable(new quantValueModel(quantity, canbechanged));
    //}

    //var menuForDay = function (dayobj, categs) {

    //    var self = this;

    //    dayobj = dayobj || {};
    //    categs = categs || [];

    //    self.ID = ko.observable(dayobj.menuForDay.id);
    //    //self.DayOfWeek = ko.observable(dayobj.menuForDay.dayOfWeek);
    //    self.OrderCanBeChanged = ko.observable(dayobj.menuForDay.orderCanBeChanged);
    //    self.Dishes = ko.observableArray(ko.utils.arrayMap(categs, function (item, ind) {

    //        var first = ko.utils.arrayFirst(dayobj.menuForDay.dishes, function (element) {
    //            var positivRes = element.category === item;

    //            return positivRes;
    //        });
    //        if (first != null) {
    //            return new dishInfo(first, dayobj.menuForDay.orderCanBeChanged, dayobj.dishQuantities[ind]);
    //        };
    //        return null;
    //    }));

    //}
    var userDayOrderInfo = function (dayOrdObject, categories) {

        var self = this;

        dayOrdObject = dayOrdObject || {};
        categories = categories || [];

        self.DayOrderId = ko.observable(dayOrdObject.dayOrderId);

        //self.MenuForDay = ko.observable(new menuForDay(dayOrdObject, categories));
        self.DishQuantities = ko.observableArray(ko.utils.arrayMap(dayOrdObject.dishQuantities,function(item) {
            return new quantValueModel(item, dayOrdObject.orderCanBeChanged);
        }));

    }

    var weekUserOrderModel = function (userWeekOrder,caterories) {

        var self = this;

        self.UserId = ko.observable(userWeekOrder.userId);

        self.OrderId = ko.observable(userWeekOrder.orderId);

        self.UserName = ko.observable(userWeekOrder.userName);

        self.UserDayOrders = ko.observableArray(ko.utils.arrayMap(userWeekOrder.dayOrderDtos, function (item) {
            return new userDayOrderInfo(item, caterories);
        }));
        
        self.BeenChanged = ko.observable(false);


    };

    var weekOrdersModel = function() {
        var self = this;

        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.CurrentWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.WeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.Categories = ko.observableArray([]);

        self.WeekUserOrderModels = ko.observableArray([]);

        self.FirstCourseValues = [0, 0.5, 1, 2, 3, 4, 5];

        self.QuantValues = [0, 1, 2, 3, 4, 5];

        self.BeenChanged = ko.observable(false);

        self.SummaryDishQuantities = ko.observableArray([]);

        self.IsNextWeekYear = ko.observable();

        function onError(error) {
            self.Message("Error: " + error.status + " " + error.statusText);
        }

        self.CalcSummaryDishQuantyties = function(daynum, catnum) {
            var dishquantyties = 0;
            ko.utils.arrayForEach(self.WeekUserOrderModels(), function(weekuserorder) {
                var dish = ko.utils.arrayFirst(weekuserorder.UserDayOrders()[daynum].DishQuantyties(), function(item, index) {
                    return index === catnum;
                });
                dishquantyties += item.Quantity();
            });
            var catLength = self.Categories().length;
            self.SummaryDishQuantities()[catLength * daynum + catnum] = dishquantyties;
        };
        self.GetCurrentWeekYear = function () {

            app.su_Service.GetCurrentWeekYear().then(function (resp) {

                self.CurrentWeekYear(resp);

            }, onError);
        }

        var factloadWeekOrders = function (wyDto1) {

            app.su_Service.FactLoadWeekOrders(wyDto1).then(function (resp1) {
                self.WeekUserOrderModels([]);

                self.WeekYear(resp1.weekYearDto);

                self.SummaryDishQuantities([]);
                self.SummaryDishQuantities.pushAll(resp1.summaryDishQuantities);

                self.WeekUserOrderModels(ko.utils.arrayMap(resp1.userWeekOrders, function (uwoObject) {

                    return  new weekUserOrderModel(uwoObject, self.Categories());

                }));

                app.su_Service.IsNextWeekYear(wyDto1).then(function (resp2) {
                    self.IsNextWeekYear(resp2);
                });

            }, onError);

        }

        self.update = function () {

            var forUpdateUwo = ko.utils.arrayFilter(self.WeekUserOrderModels(), function (item) {
                return item.BeenChanged();
            });

            var userweekorder = {
                UserWeekOrders: forUpdateUwo,
                WeekYear: self.WeekYear()
            };

            app.su_Service.UpdateOrders(userweekorder).then(function () {
                self.BeenChanged(false);
            }, onError);
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
            //if (self.WeekYear() == undefined || self.WeekYear().year==undefined) {
            //    return "";
            //}
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

        self.DaysOfWeek = ["Понедельник", "Вторник", "Среда", "Четверг", "Пятница"];

        self.pageSize = ko.observable(10);

        self.pageIndex = ko.observable(0);

        self.pagedList = ko.dependentObservable(function () {
            var size = self.pageSize();
            var start = self.pageIndex() * size;
            return self.WeekUserOrderModels.slice(start, start + size);
        });

        self.maxPageIndex = ko.dependentObservable(function () {
            return Math.ceil(self.WeekUserOrderModels().length / self.pageSize()) - 1;
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
                self.Categories([]);
                self.Categories.pushAll(resp);
            }, onError);

            self.GetCurrentWeekYear();


            self.SetMyDateByWeek(self.CurrentWeekYear());

        }

        self.init();
    };

    ko.applyBindings(new weekOrdersModel());
    //var ordersViewModel = {
    //    UserOrders: ko.observableArray([]),
    //    WeekNumber: ko.observable(),
    //    NumbersOfWeek: ko.observableArray([]),
    //    Message: ko.observable(),
    //    CurrentWeekNumber: ko.observable(),
    //    myDate: ko.observable(new Date()),
    //    FirstCourseValues: [0, 0.5, 1, 2, 3,  4, 5],
    //    QuantValues: [0, 1, 2, 3, 4, 5],
    //    BeenChanged: ko.observable(false),
    //    Year:ko.observable(),
    //    Categories :ko.observableArray([])
    //};
    //// Callback for error responses from the server.
    //function onError(error) {
    //    ordersViewModel.Message('Error: ' + error.status + ' ' + error.statusText);
    //}

    //ordersViewModel.CalcSummary = function(item) {
    //    var source = {
    //        UserId: item.UserId(),
    //        UserName: item.UserName(),
    //        SummaryPrice: item.SummaryPrice(),
    //        WeekIsPaid: item.WeekIsPaid(),
    //        Dishquantities: $.map(item.Dishquantities(), function(value) {
    //            return value.Quantity;
    //        })
    //    };
    //    app.su_Service.GetOrderSummary(ordersViewModel.WeekNumber(), ordersViewModel.Year(), source).then(
    //        function(data) {
    //            item.SummaryPrice(data.toFixed(2));
    //        },
    //        function(error) {
    //            onError(error);
    //        });

    //}.bind(ordersViewModel);

        
    
    //var userWeekOrder = function(item) {

    //    var self = this;

    //    self.UserId = ko.observable(item.userId);
    //    self.UserName = ko.observable(item.userName);
    //    self.SummaryPrice = ko.observable(item.orderSummaryPrice.toFixed(2));
    //    self.WeekIsPaid = ko.observable(item.weekIsPaid);

    //    self.Dishquantities = ko.observableArray(ko.utils.arrayMap(item.dishquantities, function(value) {
    //        return new quantValueModel(value);
    //    }));


    //}

    
    //ordersViewModel.loadWeekNumbers = function () {
    //    app.su_Service.LoadWeekNumbers().then(function (resp) {

    //        ordersViewModel.NumbersOfWeek.pushAll(resp);

    //    }, onError);
    //};


    //ordersViewModel.LoadOrders = function () {

    //    app.su_Service.LoadWeekOrders(ordersViewModel.WeekNumber(), ordersViewModel.Year()).then(
    //       function (resp) {
    //           ordersViewModel.UserOrders([]);
    //           ordersViewModel.WeekNumber(resp.weekNumber);
    //           ordersViewModel.Year(resp.yearNumber);

    //           ko.utils.arrayForEach(resp.userOrders, function (object) {

    //               ordersViewModel.UserOrders.push(new userWeekOrder(object));

    //           });
    //       },
    //       function (error) {
    //           onError(error);
    //       });
    //}

    //ordersViewModel.GetCurrentWeekYear = function () {

    //    app.su_Service.GetCurrentWeekYear().then(function (resp) {
    //        ordersViewModel.CurrentWeekNumber(resp);

    //    }, onError);
    //}

    //ordersViewModel.IsCurrentWeek = ko.computed(function() {
    //    return ordersViewModel.CurrentWeekNumber() === ordersViewModel.WeekNumber();
    //}.bind(ordersViewModel));


    //app.su_Service.GetCategories().then(function (resp) {
    //    ordersViewModel.Categories.pushAll(resp);
    //}, onError);

    //ordersViewModel.LoadOrders();
    //ordersViewModel.loadWeekNumbers();
    //ordersViewModel.GetCurrentWeekYear();

 
    //ko.applyBindings(ordersViewModel);
    

})();

