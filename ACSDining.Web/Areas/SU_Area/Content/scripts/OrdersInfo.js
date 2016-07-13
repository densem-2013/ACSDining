/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Scripts/jquery-ui-i18n.min.js" />
/// <reference path="~/Scripts/knockout-3.3.0.debug.js" />
/// <reference path="~/Areas/SU_Area/Content/scripts/app.su_Service.js" />
/// <reference path="~/Scripts/knockout.mapping-latest.debug.js" />
(function () {

    $("#menucontainer span").attr({ 'data-bind': "text: WeekTitle" });
    $("#submenu >table td:first-child").attr({ 'data-bind': "text: CurNextTitle" });

    $("#submenu td:nth-child(2)").removeClass("t-label").addClass("navlink").css({"padding":"5px 15px 25px 10px"}).text("Заявки");

    var radiobutdiv = $("<div>").attr({ "class": "radio-btn" }).css({ "padding-top": "7px", "float": "left", "display": "inline-flex" });
    radiobutdiv.append($("<!--ko foreach: PlanFactValues-->"));
    var factordersinput = $("<input/>")
    .attr({ 'data-bind': "checked: $parent.ItsFact, checkedValue: $data, click: $parent.changeSelected, attr:{id: 'dbc' + $index(),name: 'dbc' + $index()}","type": "radio" });
    radiobutdiv.append(factordersinput);
    var factorderslabel = $("<label></label>").addClass("navlink")
        .attr({ "data-bind": " attr:{'for': 'dbc' + $index()},text:$index()%2==0?'Фактические  ':'Плановые'" }).css({"padding-right": "35px"});
    radiobutdiv.append(factorderslabel);

    radiobutdiv.append($("<!--/ko-->"));

    $("#submenu td:nth-child(3)").append(radiobutdiv);

    $("ul.nav.navbar-nav li:nth-child(2)").addClass("active");
    $("#autorizeMessage span").css({ 'paddingLeft': "160px" });
    var excelButtonDiv = $('<div></div>').css({ 'whith': "100%", 'padding': "10px" });
    var sendButtonInput = $('<input type="button" id="btExcel" class="btn btn-info" value="Выгрузить в Excel" data-bind="click: GetExcel"/>');
    excelButtonDiv.append(sendButtonInput);
    $("#forpaibutton").append(excelButtonDiv);


    $("#submenu td:first-child").attr({ 'data-bind': "text: CurNextTitle" });
    var butdiv = $("<div>").css({ "display": "inline-flex" });
    var curweekbut = $("<input/>")
        .attr({ 'data-bind': "click: GoToCurrentWeekOrders, visible: !IsCurrentWeek()", "id": "curweek", "type": "button", "value": "Перейти на текущую неделю" })
        .addClass("btn btnaddmenu");
    butdiv.append(curweekbut);

    var nextweekbut = $("<input/>")
        .attr({ 'data-bind': "click: GoToNextWeekOrders,visible: !IsNextWeekYear() && IsNextWeekMenuExists()", "type": "button", "value": "Перейти на следующую неделю" })
        .addClass("btn btnaddmenu");
    butdiv.append(nextweekbut);
    $("#submenu td:last-child").append(butdiv);

    var quantValueModel = function (value) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Quantity = ko.observable(value);
        self.Store = ko.observable();

        self.beenChanged = ko.observable(false);
        self.clicked = function (item) {
            $(item).focusin();
            //event.preventDefault();
        };

        self.doubleClick = function () {
            self.beenChanged(false);
            self.Store(self.Quantity());
            self.isEditMode(true);
        };

        self.onFocusOut = function () {
                self.beenChanged(self.Store() !== self.Quantity());
                self.isEditMode(false);
        };
    }


    var weekUserOrderModel = function(userWeekOrder, sumprice) {

        var self = this;

        self.UserId = ko.observable(userWeekOrder.userId);

        self.OrderId = ko.observable(userWeekOrder.orderId);

        self.UserName = ko.observable(userWeekOrder.userName);

        self.DayOrdIdArray = ko.observableArray(userWeekOrder.dayOrdIdArray);

        self.WeekSummaryPrice = ko.observable(sumprice.toFixed(2));

        self.BeenChanged = ko.observable(false);

        self.WeekPaid = ko.observable(userWeekOrder.weekPaid);

        self.WeekIsPaid = ko.observable(userWeekOrder.weekIsPaid);

        self.UserWeekOrderDishes = ko.observableArray(ko.utils.arrayMap(userWeekOrder.userWeekOrderDishes, function(item) {
            return new quantValueModel(item);
        }));

        self.isHovering = ko.observable(false);

        self.IsSelectedRow = ko.observable(false);

        self.IsEditEnable = ko.observable(false);

        self.CalcSummary = function(weekDishPrices) {
            var userweeksum = 0;

            ko.utils.arrayForEach(self.UserWeekOrderDishes(), function (item, index) {

                userweeksum += item.Quantity() * weekDishPrices[index];
            });

            self.WeekSummaryPrice(userweeksum.toFixed(2));
        }

    };

    var weekOrdersModel = function () {
        var self = this;

        self.Title = ko.observable("");

        self.Message = ko.observable("");

        self.myDate = ko.observable();

        self.CurrentWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.WeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.NextWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.Categories = ko.observableArray([]);

        self.WeekUserOrderModels = ko.observableArray([]);

        self.SUCanChangeOrder = ko.observable();

        self.FirstCourseValues = [0, 0.5, 1, 1.5, 2, 3, 4, 5];

        self.QuantValues = [0, 1, 2, 3, 4, 5];
        self.PlanFactValues = ["fact", "plan"];
        self.ItsFact = ko.observable("fact");
        self.PageSizes = ko.pureComputed(function () {
            var res = [2, 5, 7, 10, 15, 20, 25];
            var all = self.WeekUserOrderModels().length;
            if (all > 25) {
                res.push(all);
            }
            return res;

        });

        self.Store = ko.observableArray([]);

        self.BeenChanged = ko.observable(false);

        self.SummaryDishQuantities = ko.observableArray([]);

        self.IsNextWeekYear = ko.observable();

        self.DaysOfWeek = ko.observableArray([]);

        self.AllDayNames = ko.observableArray([]);

        self.WeekDishPrices = ko.observableArray([]);

        self.PlanWeekDishPrices = ko.observableArray([]);

        self.IsNextWeekMenuExists = ko.observable();

        self.IsCurrentWeek = ko.pureComputed(function () {

            var res = self.CurrentWeekYear().week === self.WeekYear().week && self.CurrentWeekYear().year === self.WeekYear().year;
            return res;

        }.bind(self));
        
        self.IsNextWeekYear = ko.pureComputed(function() {

            var res = self.NextWeekYear().week === self.WeekYear().week && self.NextWeekYear().year === self.WeekYear().year;
            return res;

        }.bind(self));

        self.IsEditOn = ko.pureComputed(function() {
            return ko.utils.arrayFirst(self.WeekUserOrderModels(), function(model) {
                return model.IsEditEnable() === true;
            }) != null;
        }.bind(self));
        
        self.CurNextTitle = ko.pureComputed(function () {
            if (self.IsCurrentWeek()) {
                return "Текущая неделя";
            } else if (self.IsNextWeekYear()) {
                return "Следующая неделя";
            } else {
                return "";
            };
        });
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

        self.CalcSummaryDishQuantyties = function(wuord) {
            var catlengh = self.Categories().length;
            ko.utils.arrayForEach(wuord.DayOrdIdArray, function(dayid, daynum) {
                ko.utils.arrayForEach(self.Categories(), function (category, catnum) {
                    var sumbydish = 0;
                    ko.utils.arrayForEach(self.WeekUserOrderModels(), function(object) {
                        sumbydish += object.UserWeekOrderDishes()[catlengh * daynum + catnum].Quantity();
                    });
                    self.SummaryDishQuantities.replace(self.SummaryDishQuantities()[catlengh * daynum + catnum], sumbydish);
                });
            });
        };

        self.WeekTotal = ko.pureComputed(function() {
            var sum = 0;
            ko.utils.arrayForEach(self.WeekUserOrderModels(), function(object) {
                sum += parseFloat(object.WeekSummaryPrice());
            });
            return sum.toFixed(2);
        });
        
        self.SetMyDateByWeek = function (wyDto) {
            var firstDay = new Date(wyDto.year, 0, 1).getDay();
            var d = new Date("Jan 01, " + wyDto.year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (wyDto.week);
            self.myDate(new Date(w));
        }.bind(self);

        self.FirstDay = ko.pureComputed(function () {
            var day = self.DaysOfWeek()[0];
            return ko.utils.arrayIndexOf(self.AllDayNames(), day);

        });

        self.LastDay = ko.pureComputed(function () {
            var lastindex = self.DaysOfWeek().length;
            var day = self.DaysOfWeek()[lastindex - 1];
            return ko.utils.arrayIndexOf(self.AllDayNames(), day);
        });

        self.WeekTitle = ko.computed(function () {

            if (self.WeekYear().week === undefined ) return "";
            var options = {
                weekday: "long",
                year: "numeric",
                month: "short",
                day: "numeric"
            };
            var year = self.WeekYear().year;
            var firstDay = new Date(year, 0, 1).getDay();

            var week = self.WeekYear().week;
            var d = new Date("Jan 01, " + year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week) + (3600000 * 24 * self.FirstDay());
            var n1 = new Date(w);
            var n2 = new Date(w + 3600000 * 24 * (self.LastDay() - self.FirstDay()));
            return "Неделя " + week + ": " + n1.toLocaleDateString("ru-RU", options) + " - " + n2.toLocaleDateString("ru-RU", options);
        }.bind(self));


        self.pageSize = ko.observable(10);

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

        var updateViewModel = function(resp1) {
            if (resp1 != null) {
                if (resp1.weekYearDto.week === self.WeekYear().week && resp1.weekYearDto.year === self.WeekYear().year && self.WeekUserOrderModels().length!== 0) return;
                self.WeekYear(resp1.weekYearDto);
                self.DaysOfWeek(resp1.dayNames);
                self.AllDayNames(resp1.allDayNames);

                self.WeekUserOrderModels(ko.utils.arrayMap(resp1.userWeekOrders, function(uwoObject) {
                    var summaryprice = uwoObject.userWeekOrderDishes.pop();
                    return new weekUserOrderModel(uwoObject, summaryprice);
                }));

                self.SummaryDishQuantities(resp1.summaryDishQuantities);
                self.WeekDishPrices(resp1.weekDishPrices);
                self.SUCanChangeOrder(resp1.suCanChangeOrder);
                localStorage.setItem("LastOrderView", ko.mapping.toJSON({ Week: resp1.weekYearDto.week, Year: resp1.weekYearDto.year }));
                var lastfactval = localStorage.getItem("FactValue");
                self.ItsFact(lastfactval || "fact");
            } else {
                if (!self.IsCurrentWeek()) {
                    modalShow("Сообщение", "На выбранную Вами дату не было создано меню для заказа. Будьте внимательны!");
                }
            };
        };

        var loadWeekOrders = function (wyDto1, foplan) {
            if (foplan === "fact") {
                app.su_Service.LoadFactWeekOrders(wyDto1).then(function (resp1) {
                    localStorage.setItem("FactValue","fact");
                    updateViewModel(resp1);
                }, onError);
            } else {
                app.su_Service.LoadPlanWeekOrders(wyDto1).then(function (resp1) {
                    localStorage.setItem("FactValue", "plan");
                    updateViewModel(resp1);
                }, onError);
            }
        };


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

        self.changeSelected = function(checkval) {

            self.ItsFact(checkval);
            loadWeekOrders(self.WeekYear(), checkval);
            //self.SetMyDateByWeek(self.WeekYear());
            return true;
        };

        self.myDate.subscribe = ko.computed(function () {

            if (self.myDate() == undefined) return;

            var takedWeek = self.myDate().getWeek() - 1;
            var needObj = self.WeekYear();
            if (needObj != undefined && !isNaN(takedWeek)) {
                var curweek = needObj.week;
                if (!isNaN(takedWeek) ) {
                    var weekyear = {
                        Week: takedWeek,
                        Year: self.myDate().getFullYear()
                    };
                    if (!isNaN(weekyear.Week) && !isNaN(weekyear.Year)) {

                        loadWeekOrders(weekyear, self.ItsFact());
                    }
                };
            };
        }, self);

        self.GetExcel = function() {
            var forexcel = {
                ItsFact: self.ItsFact(),
                WeekYear: self.WeekYear(),
                DataString: self.WeekTitle()
            }
            app.su_Service.GetExcelOrders(forexcel)
                .then(function(res) {
                    window.location.assign(res.fileName);
                });
        };

        self.GoToNextWeekOrders = function () {
            self.SetMyDateByWeek(self.NextWeekYear());
        };


        self.GoToCurrentWeekOrders = function () {
            self.SetMyDateByWeek(self.CurrentWeekYear());
        };


        self.IsEditAllowed = ko.pureComputed(function () {

            
            return self.IsCurrentWeek() === true || self.IsNextWeekYear() === true;

        }.bind(self));

        self.SetEditMode = function (userWeekOrderModel) {
            self.Store([]);
            ko.utils.arrayForEach(userWeekOrderModel.UserWeekOrderDishes(), function (item) {
                item.isEditMode(true);
            });
            var arr = ko.utils.arrayMap(userWeekOrderModel.UserWeekOrderDishes(), function (quant) {

                    return quant.Quantity();
            });

            self.Store.pushAll(arr);

            userWeekOrderModel.IsEditEnable(true);
        };
        self.CancelEditMode = function (userWeekOrderModel) {
            console.log(self.Store());
            ko.utils.arrayForEach(userWeekOrderModel.UserWeekOrderDishes(), function ( item, index) {
                item.Quantity(self.Store()[index]);
                item.isEditMode(false);
            });
            userWeekOrderModel.CalcSummary(self.WeekDishPrices());
            self.CalcSummaryDishQuantyties(userWeekOrderModel);
            userWeekOrderModel.IsEditEnable(false);
        };

        self.rowclicked = function (item, event) {

                if (self.IsEditOn()&& self.ItsFact()!=="plan") {
                    modalShow("Сообщение", "Подтвердите или отмените редактирование предыдущей заявки!");
                    setTimeout(function () {
                        $("#modalMessage").modal("hide");
                    }, 2000);
                    return;
                };

                if (self.IsEditAllowed() === false) {
                    modalShow("Сообщение", "На выбранную Вами неделю редактирование заявок уже недоступно!");
                    setTimeout(function() {
                        $("#modalMessage").modal("hide");
                    }, 2000);
                    return;
                }
                var iseditable = false;

                ko.utils.arrayForEach(self.WeekUserOrderModels(), function(obj) {
                    iseditable = obj.isHovering() === true && self.SUCanChangeOrder() === true;
                    obj.IsSelectedRow(iseditable);
                });

                var editableObj = ko.utils.arrayFirst(self.WeekUserOrderModels(), function(obj) {
                    return obj.IsSelectedRow() === true;
                });

                self.SetEditMode(editableObj);
        };

        self.OrderSave = function (userWeekOrderModel) {
            
            var quantarr = ko.utils.arrayMap(userWeekOrderModel.UserWeekOrderDishes(), function (quar) {
                return quar.Quantity();
            });

            var forsaveobj = {
                DayOrdIds: userWeekOrderModel.DayOrdIdArray(),
                WeekOrdId: userWeekOrderModel.OrderId(),
                QuantArray: quantarr
            }
            
            ko.utils.arrayForEach(userWeekOrderModel.UserWeekOrderDishes(), function (quant) {
                quant.isEditMode(false);
            });

            app.su_Service.UpdateOrder(forsaveobj).then(function (res) {
                if (res) {
                   
                    self.CalcSummaryDishQuantyties(userWeekOrderModel);

                    userWeekOrderModel.IsEditEnable(false);
                }
            },onError);
        };

        self.init = function () {
            app.su_Service.GetCategories().then(function (resp) {
                self.Categories(resp);
            }, onError);

            app.su_Service.GetCurrentWeekYear().then(function(resp) {
                    self.WeekYear(resp);
                self.CurrentWeekYear(resp);

            }, onError)
                .then(function () {

                    var lastweekyear = localStorage.getItem("LastOrderView");
                    var lastfactval = localStorage.getItem("FactValue");
                    self.ItsFact(lastfactval || "fact");
                    if (lastweekyear == null) {
                        localStorage.setItem("LastOrderView", ko.mapping.toJSON({ Week: self.CurrentWeekYear().week, Year: self.CurrentWeekYear().year }));
                        self.SetMyDateByWeek(self.CurrentWeekYear());
                    } else {
                        var obj = ko.mapping.fromJSON(lastweekyear);
                        self.SetMyDateByWeek({week: obj.Week(), year: obj.Year()});
                    }
                });

            app.su_Service.GetNextWeekYear(self.CurrentWeekYear()).then(function (nextDto) {
                self.NextWeekYear(nextDto);
            }, onError);

            app.su_Service.IsNextWeekMenuExists().then(function (respnext) {
                self.IsNextWeekMenuExists(respnext);
            }, onError);

        }

        self.init();

    };

    ko.applyBindings(new weekOrdersModel());

})();

