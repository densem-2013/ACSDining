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
    var excelButtonDiv = $('<div></div>').css({ 'whith': '100%', 'padding': '10px' });
    var sendButtonInput = $('<input type="button" id="btExcel" class="btn btn-info" value="Выгрузить в Excel" data-bind="click: GetExcel"/>');
    excelButtonDiv.append(sendButtonInput);
    $('#datepick').append(excelButtonDiv);
    //$(".container").css({ 'marginLeft': 0 });

    var quantValueModel = function (value) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Quantity = ko.observable(value);
        self.Store = ko.observable();

        self.beenChanged = ko.observable(false);
        self.clicked = function (item) {
                $(item).focusin();
        };

        self.onmouseenter = function () {
                self.beenChanged(false);
                self.Store(self.Quantity());
                self.isEditMode(true);
        };

        self.onFocusOut = function () {
                self.beenChanged(self.Store() !== self.Quantity());
                self.isEditMode(false);
        };
    }


    var weekUserOrderModel = function (userWeekOrder,sumprice) {

        var self = this;

        self.UserId = ko.observable(userWeekOrder.userId);

        self.OrderId = ko.observable(userWeekOrder.orderId);

        self.UserName = ko.observable(userWeekOrder.userName);
        self.DayOrdIdArray = ko.observableArray(userWeekOrder.dayOrdIdArray);
        self.WeekSummaryPrice = ko.observable(sumprice.toFixed(2));

        self.BeenChanged = ko.observable(false);


        self.WeekPaid = ko.observable(userWeekOrder.weekPaid);

        self.WeekIsPaid = ko.observable(userWeekOrder.weekIsPaid);

        self.UserWeekOrderDishes = ko.observableArray(ko.utils.arrayMap(userWeekOrder.userWeekOrderDishes,function(item) {
            return new quantValueModel(item);
        }));
    };

    var weekOrdersModel = function () {
        var self = this;

        self.Title = ko.observable("");

        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.CurrentWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.WeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.Categories = ko.observableArray([]);

        self.WeekUserOrderModels = ko.observableArray([]);

        self.SUCanChangeOrder = ko.observable();

        self.FirstCourseValues = [0, 0.5, 1, 2, 3, 4, 5];

        self.QuantValues = [0, 1, 2, 3, 4, 5];

        self.PageSizes = ko.pureComputed(function () {
            var res = [2, 5, 7, 10, 15, 20, 25];
            var all = self.WeekUserOrderModels().length;
            if (all > 25) {
                res.push(all);
            }
            return res;

        });

        self.BeenChanged = ko.observable(false);

        self.SummaryDishQuantities = ko.observableArray([]);

        self.IsNextWeekYear = ko.observable();

        self.DaysOfWeek = ko.observableArray([]);

        self.WeekDishPrices = ko.observableArray([]);

        self.PlanWeekDishPrices = ko.observableArray([]);

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

        self.CalcSummaryDishQuantyties = function(wuord, daynum, catnum) {
            var catlengh = self.Categories().length;
            var userweeksum = 0;
            var sumbydish = 0;
            ko.utils.arrayForEach(wuord.UserWeekOrderDishes(), function(item, index) {
                userweeksum += item.Quantity() * self.WeekDishPrices()[index];
            });
            wuord.WeekSummaryPrice(userweeksum.toFixed(2));

            ko.utils.arrayForEach(self.WeekUserOrderModels(), function(object, ind) {
                sumbydish += object.UserWeekOrderDishes()[catlengh * (daynum - 1) + catnum - 1].Quantity();
            });
            self.SummaryDishQuantities.replace(self.SummaryDishQuantities()[catlengh * (daynum-1) + catnum -1], sumbydish);
        };

        self.update = function (wuOrder, index, quantity) {

            var catlengh = self.Categories().length;
            var daynumber = Math.ceil(index / catlengh);
            var catnumber = index - (daynumber - 1) * catlengh;

            var userweekorder = {
                DayOrderId: wuOrder.DayOrdIdArray()[daynumber - 1],
                CategoryId: catnumber,
                Quantity: quantity
            };

            self.CalcSummaryDishQuantyties(wuOrder,daynumber, catnumber);

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


        self.pageSize = ko.observable(7);

        self.pageIndex = ko.observable(0);

        self.pagedList = ko.dependentObservable(function () {
            var size = self.pageSize();
            var start = self.pageIndex() * size;
            return self.WeekUserOrderModels.slice(start, start + size);
        });

        self.maxPageIndex = ko.dependentObservable(function () {
            return Math.ceil(self.WeekUserOrderModels().length/ self.pageSize()) - 1;
        });


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


        var factloadWeekOrders = function (wyDto1) {

            app.su_Service.FactLoadWeekOrders(wyDto1).then(function (resp1) {
                if (resp1 != null) {
                    self.WeekYear(resp1.weekYearDto);
                    self.DaysOfWeek(resp1.dayNames);

                    self.WeekUserOrderModels(ko.utils.arrayMap(resp1.userWeekOrders, function (uwoObject) {
                        var summaryprice = uwoObject.userWeekOrderDishes.pop();
                        return new weekUserOrderModel(uwoObject, summaryprice);
                    }));

                    self.SummaryDishQuantities(resp1.summaryDishQuantities);
                    self.WeekDishPrices(resp1.weekDishPrices);
                    self.SUCanChangeOrder(resp1.suCanChangeOrder);
                } else {
                    modalShow("Сообщение", "На выбранную Вами дату не было создано меню для заказа. Будьте внимательны!");
                };

            }, onError);

        }
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

        self.GetExcel = function () {
            var forexcel = {
                WeekYear: self.WeekYear(),
                DataString: self.WeekTitle()
            }
            app.su_Service.GetExcelFactOrders(forexcel)
                .then(function (res) {
                    window.location.assign(res.fileName);
                });
        };

        self.init = function () {
            app.su_Service.GetCategories().then(function (resp) {
                self.Categories(resp);
            }, onError);

            app.su_Service.GetCurrentWeekYear().then(function(resp) {
                    self.WeekYear(resp);
                self.CurrentWeekYear(resp);

                //self.GetUnitCounts();

            }, onError)
                .then(function () {
                    self.SetMyDateByWeek(self.CurrentWeekYear());
                }
            );


        }

        self.init();

    };

    ko.applyBindings(new weekOrdersModel());

})();

