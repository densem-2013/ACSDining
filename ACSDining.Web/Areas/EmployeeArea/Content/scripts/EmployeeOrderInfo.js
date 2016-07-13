/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Scripts/knockout-3.2.0.js" />
/// <reference path="~/Scripts/knockout-2.2.1.debug.js" />
/// <reference path="~/Areas/EmployeeArea/Content/scripts/app.EmployeeService.js" />
/// <reference path="~/Content/app/jquery-1.10.2.js" />
/// <reference path="~/Scripts/knockout.mapping-latest.debug.js" />
/// <reference path="~/Scripts/bootstrap.js" />
/// <reference path="~/Areas/EmployeeArea/Content/scripts/app.EmployeeService.js" />
/// <reference path="~/Scripts/knockout-3.3.0.debug.js" />
(function() {

    $("#menucontainer span").attr({ 'data-bind': "text: WeekTitle" }).css({ "white-space": "nowrap" });

   var quantValueModel = function(value) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Quantity = ko.observable(value);
        self.Store = ko.observable();
        self.beenChanged = ko.observable(false);
        self.clicked = function(item) {
            $(item).focusin();
        };

        self.doubleClick = function() {
            self.beenChanged(false);
            self.Store(self.Quantity());
            self.isEditMode(true);
        };

        self.onFocusOut = function() {
            self.beenChanged(self.Store() !== self.Quantity());
            self.isEditMode(false);
        };
    }

    var dishInfo = function(dinfo, quantity) {

        var self = this;

        self.Title = ko.observable(dinfo.title || "-------------");
        //self.ProductImage = ko.observable(dinfo.productImage);
        self.Price = ko.observable(dinfo.price.toFixed(2));
        self.Category = ko.observable(dinfo.category);
        self.Description = ko.observable(dinfo.description);
        self.OrderQuantity = ko.observable(new quantValueModel(quantity));
        self.isHovering = ko.observable(false);
    }

    var userDayOrderInfo = function(dayOrdObject, dishQuantities) {

        var self = this;

        dayOrdObject = dayOrdObject || {};

        self.DayOrderId = ko.observable(dayOrdObject.dayOrdId);
        self.OrderCanByChanged = ko.observable(dayOrdObject.orderCanByChanged);
        self.Dishes = ko.observableArray(ko.utils.arrayMap(dayOrdObject.dishes, function(obj, index) {
            return new dishInfo(obj, dishQuantities[index]);
        }));

        self.DayOrderSummary = ko.observable(dayOrdObject.dayOrderSummary.toFixed(2));

        self.CalcDayOrderTotal = function() {

            var sum = 0;

            ko.utils.arrayForEach(self.Dishes(), function(dish) {
                sum += parseFloat(dish.Price() * dish.OrderQuantity().Quantity());
            });
            return self.DayOrderSummary(sum.toFixed(2));
        };
        self.DayName = ko.observable(dayOrdObject.dayName);
    }

    var weekUserOrderModel = function() {

        var self = this;

        self.OrderId = ko.observable();

        self.CurrentWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.WeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.NextWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.UserDayOrders = ko.observableArray([]);

        self.Store = ko.observableArray();

        self.Title = ko.observable("");

        self.Message = ko.observable("");

        self.myDate = ko.observable();

        self.WeekSummaryPrice = ko.observable(0);

        self.WeekPaid = ko.observable();

        self.WeekIsPaid = ko.observable();

        self.WeekPaiment = ko.observable(0);

        self.CanCreateOrderOnNextWeek = ko.observable();

        self.Email = ko.observable();

        self.HasEmail = ko.observable();

        self.IsEditEnable = ko.observable(false);

        self.DayNames = ko.observableArray([]);

        self.IsNextWeekYear = ko.pureComputed(function() {
            var res = self.NextWeekYear().week === self.WeekYear().week && self.NextWeekYear().year === self.WeekYear().year;
            return res;

        }.bind(self));

        self.FirstCourseValues = [0, 0.5, 1, 1.5, 2, 3, 4, 5];

        self.QuantValues = [0, 1, 2, 3, 4, 5];

        self.HasBalance = ko.observable();
        self.HasSummary = ko.observable();
        self.CheckDebt = ko.observable();
        self.PreviosweekBalance = ko.observable(0);
        self.BeenChanged = ko.observable(false);
        self.AllowDebt = ko.observable();
        self.Balance = ko.dependentObservable(function() {
            var bal1 = parseFloat(self.HasBalance());
            var bal2 = self.WeekSummaryPrice();
            var bal3 = parseFloat(self.HasSummary());
            return bal1 + bal3 - bal2;
        });


        self.IsCurrentWeek = ko.pureComputed(function() {

            var res = self.CurrentWeekYear().week === self.WeekYear().week && self.CurrentWeekYear().year === self.WeekYear().year;
            return res;

        }.bind(self));

        self.IsEditAllowed = ko.pureComputed(function() {

            //console.log("item.IsCurrentWeek()= ", self.IsCurrentWeek());
            //console.log("item.IsNextWeekYear()= ", self.IsNextWeekYear());
            if (self.IsCurrentWeek() === true || self.IsNextWeekYear() === true) {
                var res = ko.utils.arrayFirst(self.UserDayOrders(), function (item) {

                    //console.log("item.OrderCanByChanged()= ", item.OrderCanByChanged());
                    return item.OrderCanByChanged() === true;
                });
                //console.log("res= ",res != null);
                return res != null;
            }
            return false;
        }.bind(self));

        self.HoverDesciption = ko.dependentObservable(function() {
            var hovday = ko.utils.arrayFirst(self.UserDayOrders(), function(item) {
                return ko.utils.arrayFirst(item.Dishes(), function(dish) {
                    return dish.isHovering();
                });
            });
            if (hovday == null) {
                return "";
            }
            var hovitem = ko.utils.arrayFirst(hovday.Dishes(), function(dish) {
                return dish.isHovering();
            });
            return hovitem == null ? "" : (hovitem.Description() === "") ? "Описание блюда отсутствует" : hovitem.Description();
        });

        self.CurNextTitle = ko.pureComputed(function() {
            if (self.IsCurrentWeek()) {
                return "Текущая неделя";
            } else if (self.IsNextWeekYear()) {
                return "Следующая неделя";
            } else {
                return "";
            };
        });

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

        self.SetMyDateByWeek = function(wyDto) {
            var firstDay = new Date(wyDto.year, 0, 1).getDay();
            var d = new Date("Jan 01, " + wyDto.year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (wyDto.week);
            self.myDate(new Date(w));
        }.bind(self);

       


        var loadUserWeekOrder = function(wyDto1) {

            if (wyDto1.Week === 0) return;

            app.EmployeeService.LoadUserWeekOrder(wyDto1).then(function(resp) {
                if (resp) {
                    self.OrderId(resp.weekOrderId);
                    self.WeekIsPaid(resp.weekIsPaid);
                    self.WeekYear(resp.weekYear);
                    self.DayNames(resp.dayNames);
                    var summary = resp.weekOrderDishes.pop();
                    self.WeekSummaryPrice(summary.toFixed(2));
                    self.HasSummary(summary.toFixed(2));
                    self.HasBalance(resp.balance.toFixed(2));
                    self.UserDayOrders(ko.utils.arrayMap(resp.dayOrders, function(object, index) {
                        var dishcount = object.dishes.length;
                        var start = resp.weekOrderDishes.slice(index * dishcount, (index + 1) * dishcount);
                        return new userDayOrderInfo(object, start);
                    }));
                    self.PreviosweekBalance(resp.prevWeekBalance);
                    self.WeekPaiment(resp.weekPaiment);
                    self.AllowDebt(resp.allowDebt);
                    self.CheckDebt(resp.checkDebt);
                    localStorage.setItem("LastEmployeeView", ko.mapping.toJSON({ Week: wyDto1.Week, Year: wyDto1.Year }));
                } else {
                    modalShow("Сообщение", "На выбранную Вами дату Ваших заказов не было!");
                }


            }, onError);

        }
        self.FirstDay = ko.pureComputed(function () {
            var day = self.UserDayOrders()[0].DayName();
            return ko.utils.arrayIndexOf(self.DayNames(), day);

        });

        self.LastDay = ko.pureComputed(function () {
            var lastindex = self.UserDayOrders().length;
            var day = self.UserDayOrders()[lastindex-1].DayName();
            ko.utils.arrayForEach(self.DayNames(), function (item, index) {
                if (item === day) {
                    lastindex = index;
                };
            });

            return lastindex;
        });

        self.WeekTitle = ko.computed(function () {

            if (self.WeekYear().week === undefined || self.UserDayOrders().length===0) return "";
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

        self.checkallowedit = function (orderCanBeChanged) {
            if (orderCanBeChanged === false) {

                modalShow("Сообщение", "На выбранный Вами день редактирование заказа недоступно!");
                setTimeout(function() {
                    $("#modalMessage").modal("hide");
                }, 2000);
            }

        };

        self.GoToNextWeekOrder = function() {
            self.SetMyDateByWeek(self.NextWeekYear());
        };


        self.GoToCurrentWeekOrder = function() {
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

        //self.update = function (dayord, catnumber, quantity) {

        //    var userweekorder = {
        //        DayOrderId: dayord.DayOrderId(),
        //        CategoryId: catnumber,
        //        Quantity: quantity
        //    };

        //    self.CalcSummary();

        //    if (self.CheckDebt() && (self.Balance() < -1 * self.AllowDebt())) {

        //        modalShow("Внимание!", "Ваш текущий баланс превышает допустимый лимит задолженности. Заказ не может быть принят.");

        //        loadUserWeekOrder(self.WeekYear());

        //        return;
        //    };
        //    app.EmployeeService.UserWeekUpdateOrder(userweekorder).then(function(res) {
        //        self.BeenChanged(false);
        //        if (res === "noordchenged") {
        //            dayord.OrderCanByChanged(false);
        //            modalShow("Внимание!", "Редактирование заявки на этот день уже закрыто. Если Вам необходио внести изменения в заказ на этот день, обратитесь к администрации столовой.")
        //        }
        //    });
        //};


        self.myDate.subscribe = ko.computed(function() {

            if (self.myDate() == undefined) return;

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

        self.applyEmail = function() {

            app.EmployeeService.SetEmail(self.Email());
            $("#emailComfirm").modal("hide");

        };

        self.setasprev = function () {

            var asPrevObj;
            app.EmployeeService.GetPrevWeekOrderQuantity(self.OrderId()).then(function(resp) {
                asPrevObj = {
                    OrderId: resp.prevWeekOrdId,
                    DayNames: resp.dayNames,
                    Prevquants: resp.prevquants
                };
                ko.utils.arrayForEach(self.UserDayOrders(), function (dayobj, dayindex) {
                    var dnind;
                    var dnfirst = ko.utils.arrayFirst(asPrevObj.DayNames, function (item, ind) {
                        dnind = ind;
                        return item === dayobj.DayName();
                    });
                    if (dayobj.OrderCanByChanged() === true && dnfirst !== null) {
                        var dishcount = dayobj.Dishes().length;
                        ko.utils.arrayForEach(dayobj.Dishes(), function (dish, dishindex) {
                            dish.OrderQuantity(new quantValueModel(asPrevObj.Prevquants[dishcount * dnind + dishindex]));
                        });
                    }
                });
            }).then(function() {

                self.CalcSummary();

                if (self.CheckDebt() && (self.Balance() < -1 * self.AllowDebt())) {

                    modalShow("Внимание!", "Ваш текущий баланс превышает допустимый лимит задолженности. Заказ не может быть принят.");

                    loadUserWeekOrder(self.WeekYear());
                    return;
                }else {
                    app.EmployeeService.SetOrderAsPrevWeek(self.OrderId()).then(function (resp) {
                        if (resp === true) {
                            loadUserWeekOrder(self.WeekYear());
                        }
                    });
                };
            });

        };

        self.allbyone = function () {
                ko.utils.arrayForEach(self.UserDayOrders(), function (dayobj, dayindex) {
                if (dayobj.OrderCanByChanged() === true ) {
                    var dishcount = dayobj.Dishes().length;
                    ko.utils.arrayForEach(dayobj.Dishes(), function (dish, dishindex) {
                        dish.OrderQuantity(new quantValueModel(1));
                    });
                }
            });
            self.CalcSummary();

            if (self.CheckDebt() && (self.Balance() < -1 * self.AllowDebt())) {

                modalShow("Внимание!", "Ваш текущий баланс превышает допустимый лимит задолженности. Заказ не может быть принят.");

                loadUserWeekOrder(self.WeekYear());

                return;
            } else {
                app.EmployeeService.SetAllByOne(self.OrderId()).then(function (resp) {
                    if (resp === true) {
                        loadUserWeekOrder(self.WeekYear());
                    }
                });
            };
        };

        self.SetEditMode = function () {
            self.Store([]);
            var arr = ko.utils.arrayMap(ko.utils.arrayFilter(self.UserDayOrders(), function(dayorder) {
                return dayorder.OrderCanByChanged() === true;
            }), function(dayord) {
                return ko.utils.arrayMap(dayord.Dishes(), function(dish) {
                    return dish.OrderQuantity().Quantity();
                });
            });

            ko.utils.arrayForEach(arr, function(item) {
                self.Store.pushAll(item);
            });

            ko.utils.arrayForEach(self.UserDayOrders(), function(dayor) {
                if (dayor.OrderCanByChanged() === true) {
                    ko.utils.arrayForEach(dayor.Dishes(), function(dish) {
                        dish.OrderQuantity().isEditMode(true);
                    });
                }
            });
            self.IsEditEnable(true);
        };

        self.OrderSave = function () {
            var quantarr = [];
            ko.utils.arrayForEach(ko.utils.arrayMap(ko.utils.arrayFilter(self.UserDayOrders(), function(dayorder) {
                return dayorder.OrderCanByChanged() === true;
            }), function(dayord) {
                return ko.utils.arrayMap(dayord.Dishes(), function(dish) {
                    return dish.OrderQuantity().Quantity();
                });
            }), function(quar) {
                ko.utils.arrayForEach(quar, function(quaelem) {
                    quantarr.push(quaelem);
                });
            });
            var forsaveobj = {
                DayOrdIds: ko.utils.arrayMap(ko.utils.arrayFilter(self.UserDayOrders(), function (dayorder) {
                    return dayorder.OrderCanByChanged() === true;
                }), function (item) {
                    return item.DayOrderId();
                }),
                WeekOrdId: self.OrderId(),
                QuantArray: quantarr
            }
            self.IsEditEnable(false);
            ko.utils.arrayForEach(ko.utils.arrayFilter(self.UserDayOrders(), function (dayorder) {
                return dayorder.OrderCanByChanged() === true;
            }), function (dayor) {
                    ko.utils.arrayForEach(dayor.Dishes(), function(dish) {
                        dish.OrderQuantity().isEditMode(false);
                    });
            });
            self.CalcSummary();

            if (self.CheckDebt() && (self.Balance() < -1 * self.AllowDebt())) {

                modalShow("Внимание!", "Ваш баланс после данного заказа будет превышать допустимый лимит задолженности. Заказ не может быть принят.");
                ko.utils.arrayForEach(ko.utils.arrayFilter(self.UserDayOrders(), function (dayorder) {
                    return dayorder.OrderCanByChanged() === true;
                }), function (dayor, dayindex) {
                    var dishcount = dayor.Dishes().length;
                    ko.utils.arrayForEach(dayor.Dishes(), function (dish, dishindex) {
                        dish.OrderQuantity(new quantValueModel(self.Store()[dishcount * dayindex + dishindex]));
                    });
                });

                self.CalcSummary();

                return;
            } else {
                app.EmployeeService.UpdateAll(forsaveobj).then(function (resp) {
                    if (resp === true) {
                        loadUserWeekOrder(self.WeekYear());
                    }
                });
            };
        };

        self.init = function() {

            app.EmployeeService.GetCurrentWeekYearForEmployee().then(function (resp2) {

                self.CurrentWeekYear(resp2);

            }, onError)
                .then(function () {

                var lastweekyear = localStorage.getItem("LastEmployeeView");
                var obj = ko.mapping.fromJSON(lastweekyear);

                if (lastweekyear == null || obj.Week == undefined) {
                    localStorage.setItem("LastEmployeeView", ko.mapping.toJSON({ Week: self.CurrentWeekYear().week, Year: self.CurrentWeekYear().year }));
                    self.SetMyDateByWeek(self.CurrentWeekYear());
                }
                else {
                 if (obj.Week() === 0) {
                     self.SetMyDateByWeek(self.CurrentWeekYear());
                 }
                 else {
                    self.SetMyDateByWeek({ week: obj.Week(), year: obj.Year() });
                 }
                }

            });

            app.EmployeeService.GetUserNextWeekYear().then(function(nextWeekYear) {
                self.NextWeekYear(nextWeekYear);
            });

            app.EmployeeService.CanCreateOrderOnNextWeek().then(function(cancreatenext) {
                self.NextWeekOrderExist(cancreatenext);
            }, onError);

            app.EmployeeService.IsEmailExists().then(function(exists) {
                self.HasEmail(exists);
                if (!exists) {
                    $("#emailComfirm").modal("show");
                }
            });

        }
        self.init();
    }
    ko.applyBindings(new weekUserOrderModel());

}());

