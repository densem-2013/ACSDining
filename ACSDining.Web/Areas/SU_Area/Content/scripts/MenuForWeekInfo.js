/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Areas/SU_Area/Content/scripts/app.su_Service.js" />
/// <reference path="~/Scripts/knockout-3.2.0.js" />
/// <reference path="~/Scripts/bootstrap.js" />
/// <reference path="~/Scripts/knockout-3.3.0.debug.js" />
/// <reference path="~/Scripts/knockout.mapping-latest.debug.js" />
/// <reference path="~/Content/app/jquery-1.10.2.js" />
(function() {

    $("#menucontainer span").attr({ 'data-bind': "text: WeekTitle" });

    $("ul.nav.navbar-nav li:first-child").addClass("active"); 
    $("#autorizeMessage span").css({ 'paddingLeft': "160px" });
    var sendButtonDiv = $('<div></div>').css({ 'whith': '100%','padding':'10px' });
    var sendButtonInput = $('<input type="button" id="btSend" class="btn btn-info" value="Отправить сообщение заказавщим" data-bind="click: SendBooking, visible: ForEmailExistsObject"/>');
    sendButtonDiv.append(sendButtonInput);
    $('#datepick').append(sendButtonDiv);
   // $('.wrapper').css({ 'margin': 'auto' });
    //sendButtonDiv.insertAfter($('#datepick'));
    $("#submenu td:first-child").attr({ 'data-bind': "text: CurNextTitle" });
    var curweekbut=$("<input/>")
    .attr({ 'data-bind': "click: GoToCurrentWeekMenu,visible: !IsCurrentWeekYear()" , "id": "curweek" ,  "type": "button" ,  "value": "Перейти на текущую неделю" })
    .addClass("btn btnaddmenu");
    $("#submenu td:nth-child(2)").append(curweekbut);

    var btWorkDays=$("<input/>")
    .attr({ 'data-bind': "click: WorkWeekApply,visible: !WorkingDaysAreSelected()", "id": "btWorkDays", "type": "button", "value": "Подтвердить рабочие дни" })
    .addClass("btn btnaddmenu");
    $("#submenu td:nth-child(3)").append(btWorkDays);

    var btWorkDays = $("<input/>")
    .attr({ 'data-bind': "click: SetAsOrderable,visible: WorkingDaysAreSelected()&&!OrderCanBeCreated()", "type": "button", "value": "Подтвердить возможность заказа по меню" })
    .addClass("btn btnaddmenu");
    $("#submenu td:nth-child(3)").append(btWorkDays);

    var nextweekbut = $("<input/>")
    .attr({ 'data-bind': "click: GoToNextWeekMenu,visible: !IsNextWeekYear() && IsNextWeekMenuExists()", "type": "button", "value": "Редактировать меню на следующую неделю" })
    .addClass("btn btnaddmenu");
    $("#submenu td:last-child").append(nextweekbut);

    var nextweekbut = $("<input/>")
    .attr({ 'data-bind': "click: CreateNextWeekMenu,visible: !IsNextWeekYear() && !IsNextWeekMenuExists()", "type": "button", "value": "Создать меню на следующую неделю" })
    .addClass("btn btnaddmenu");
    $("#submenu td:last-child").append(nextweekbut);

    var dishInfo = function(dinfo) {

        var self = this;

        self.DishId = ko.observable(dinfo.dishId);
        var titleval = dinfo.title || "Блюдо не выбрано";
        self.Title = ko.observable(titleval);
        self.Description = ko.observable(dinfo.description);
        self.Price = ko.observable(dinfo.price.toFixed(2));
        self.Category = ko.observable(dinfo.category);

        self.UpdateWeekMenu = function(dishupdate) {
            self.DishId(dishupdate.DishId());
            self.Title(dishupdate.Title());
            self.Description(dishupdate.Description());
            self.Price(dishupdate.Price());

        };
        self.isHovering = ko.observable(false);
    }
    
    var menuForDayInfo = function(object, isworking) {

        var self = this;

        object = object || {};

        self.Id = ko.observable(object.id);
        self.IsWorking = ko.observable(isworking);
        var ind = 0;
        self.Dishes = ko.observableArray(ko.utils.arrayMap(object.dishes, function (item) {
            return new dishInfo(item);
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



    var weekMenuModel = function() {
        var self = this;

        self.MenuId = ko.observable();

        self.CurrentWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.WeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.NextWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.MFD_models = ko.observableArray([]);

        self.UpdatedDayMenus = ko.observableArray([]);

        self.Title = ko.observable("");

        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.DishesByCategory = ko.observableArray([]);

        self.Category = ko.observable();

        self.Categories = ko.observableArray();

        self.WorkingDaysAreSelected = ko.observable();

        self.SelectedDish = ko.observable();

        self.UpdatableMFD = ko.observable();

        self.SummaryPrice = ko.observable();

        self.IsNextWeekMenuExists = ko.observable();

        self.WorkWeekDays = ko.observableArray([]);

        self.DayNames = ko.observableArray([]);
        
        self.IsNextWeekYear = ko.pureComputed(function () {

            var res=self.NextWeekYear().week === self.WeekYear().week && self.NextWeekYear().year === self.WeekYear().year;
            //console.log("IsNextWeekYear = " + res + " week =" + self.NextWeekYear().week);
            return res;

        }.bind(self));

        self.WorkWeek = ko.observable();

        self.OrderCanBeCreated = ko.observable();

        function performEffect(seltor) {
            setInterval(function() {
                $(seltor).animate({ opacity: "-=0.8" }, 1000).animate({ opacity: "+=0.8" }, 1000);
            }, 1000);
        };
        

    //Сигнализирует, что есть пользователи, которым нужно отправить сообщение об изменении меню
        self.ForEmailExistsObject = ko.pureComputed(function() {
            var res = self.UpdatedDayMenus().length > 0;
            if (res) {
                performEffect('#btSend');
            };
            return self.UpdatedDayMenus().length > 0;
        }.bind(self));

        self.IsCurrentWeekYear = ko.pureComputed(function () {

            return self.CurrentWeekYear().week === self.WeekYear().week && self.CurrentWeekYear().year === self.WeekYear().year;

        }.bind(self));

        self.BeenChanged = ko.observable(false);

        self.ChangeSaved = ko.observable(false);

        self.CurNextTitle = ko.pureComputed(function () {
            if (self.IsCurrentWeekYear()) {
                return "Текущая неделя";
            } else if (self.IsNextWeekYear()) {
                return "Следующая неделя";
            } else {
                return "";
            };
        });

        function modalShow(title, message) {

            self.Title(title);
            self.Message(message);
            $("#modalMessage").modal("show");

        };
        // Callback for error responses from the server.
        function onError(error) {

            modalShow("Внимание, ошибка! ", "Error: " + error.status + " " + error.statusText);
        };

        self.WeekTitle = ko.computed(function () {
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
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week);
            var n1 = new Date(w);
            var n2 = new Date(w + 345600000);
            return "Неделя " + week + ": " + n1.toLocaleDateString("ru-RU", options) + " - " + n2.toLocaleDateString("ru-RU", options);
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

        self.showDishes = function (searchdish, index) {
            if (self.WorkingDaysAreSelected()) {
                self.DishesByCategory([]);
                self.UpdatableMFD(index);
                self.Category(searchdish.Category());
                self.loadDishes(searchdish);
                self.pageIndex(0);

                $("#modalbox").modal("show");
            } else {
                modalShow("Создание меню", "Сначала подтвердите выбор рабочих дней");
            }
        }

        self.changeSelected = function(clikedItem) {
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

        self.loadWeekMenu = function (wyDto) {

            app.su_Service.LoadWeekMenu(wyDto).then(function(resp) {
                    if (resp != null) {
                        self.MFD_models(ko.utils.arrayMap(resp.mfdModels, function(item, ind) {
                            return new menuForDayInfo(item, resp.workWeekDays[ind]);
                        }));

                        self.MenuId(resp.id);
                        self.WeekYear(resp.weekYear);
                        self.SummaryPrice(resp.summaryPrice.toFixed(2));
                        self.OrderCanBeCreated(resp.orderCanBeCreated);
                        self.WorkingDaysAreSelected(resp.workingDaysAreSelected);
                        self.DayNames(resp.dayNames);
                        app.su_Service.IsNextWeekMenuExists().then(function(respnext) {
                            self.IsNextWeekMenuExists(respnext);
                        }, onError);
                    } else {
                        if (!self.IsCurrentWeekYear()) {
                            modalShow("Сообщение", "На выбранную Вами дату не было создано меню для заказа. Будьте внимательны!");
                        }
                    }
                },
                onError);
            
        }


        self.GoToNextWeekMenu = function() {

                self.SetMyDateByWeek(self.NextWeekYear());
        };


        self.GoToCurrentWeekMenu = function () {

            self.SetMyDateByWeek(self.CurrentWeekYear());
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

                        self.loadWeekMenu(weekyear);
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

        //self.DeleteNextWeekMenu = function() {
        //    var menuid = self.MenuId();
        //    app.su_Service.DeleteNextWeekMenu(menuid).then(function () {

        //        app.su_Service.IsNextWeekMenuExists().then(function (respnext) {
        //            self.IsNextWeekMenuExists(respnext);
        //        }, onError);

        //        self.SetMyDateByWeek(self.CurrentWeekYear());
        //    }, onError);
        //}

        self.CreateNextWeekMenu = function() {
            app.su_Service.CreateNextWeekMenu().then(function(res) {

                    self.NextWeekYear(res);

                    app.su_Service.IsNextWeekMenuExists().then(function (respnext) {
                        self.IsNextWeekMenuExists(respnext);
                    }, onError);

                    self.SetMyDateByWeek(self.NextWeekYear());
            });
        };

        self.CalcTotal = function() {

            var sum = 0;
            ko.utils.arrayForEach(self.MFD_models(), function(item, index) {
                if (item.IsWorking()) {
                    sum += parseFloat(item.TotalPrice());
                }

            });

            self.SummaryPrice(sum.toFixed(2));

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
                WeekMenuId: self.MenuId(),
                DateTime: self.WeekTitle()
            };
            app.su_Service.SetAsOrderable(orderablemessage).then(function(res) {
                self.OrderCanBeCreated(res);
            }, onError);
        }

        self.WorkWeekApply = function() {
            var wwdaysarray = ko.utils.arrayMap(self.MFD_models(), function(item) {
                return item.IsWorking();
            });
            var sendwdaysobj= {
                MenuId: self.MenuId(),
                WorkDays: wwdaysarray
            }
            app.su_Service.ApplyWorkWeek(sendwdaysobj).then(function (res) {

                self.WorkingDaysAreSelected(res);

            });

        };

        self.init = function () {
            app.su_Service.GetCategories().then(function (resp) {
                self.Categories(resp);
            }, onError);


            app.su_Service.GetCurrentWeekYear().then(function (resp) {

                self.CurrentWeekYear(resp);
            }, onError);

            app.su_Service.IsNextWeekMenuExists().then(function (respnext) {
                self.IsNextWeekMenuExists(respnext);
            }, onError);

            app.su_Service.GetNextWeekYear(self.CurrentWeekYear()).then(function (nextDto) {
                self.NextWeekYear(nextDto);
            }, onError);
            if (!self.WorkingDaysAreSelected()) {
                performEffect('#btWorkDays');
            }
        };
        self.init();

    }


    ko.applyBindings(new weekMenuModel());


})();
