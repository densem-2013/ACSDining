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

        self.CurrentWeekNumber = ko.observable();

        self.WeekNumber = ko.observable();


        self.MFD_models = ko.observableArray([]);


        self.Message = ko.observable("");

        self.myDate = ko.observable(new Date());

        self.Categories = ko.observableArray();

        self.SummaryPrice = ko.observable();

        self.NumbersWeeks = ko.observableArray();
        self.WeekIsPaid = ko.observable();

        self.CanCreateOrderOnNextWeek = ko.observable();

        //self.CanCreateOrderOnNextWeek.subscribe = ko.computed(function() {

        //    var cur = self.CurrentWeekNumber();
        //    var result = false;
        //    ko.utils.arrayForEach(self.NumbersWeeks(), function(value) {
        //        if (value === cur + 1) result = true;
        //    });
        //    self.CanCreateOrderOnNextWeek(result);
        //});

        self.IsNextWeekMenu = ko.observable(false);

        self.IsNextWeekMenu.subscribe = ko.computed(function() {

            var cur = self.WeekNumber();

            self.IsNextWeekMenu(cur === self.CurrentWeekNumber() + 1);
        });

        self.Year = ko.observable();

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
            var year = self.Year();
            var firstDay = new Date(year, 0, 1).getDay();

            var week = self.WeekNumber();
            var d = new Date("Jan 01, " + year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week - 1);
            var n1 = new Date(w);
            var n2 = new Date(w + 345600000);
            return "Неделя " + week + ", " + n1.toLocaleDateString("ru-RU", options) + " - " + n2.toLocaleDateString("ru-RU", options);
        }.bind(self));


        //self.loadWeekNumbers = function() {
        //    app.su_Service.LoadUserOrderWeekNumbers().then(function(resp) {
        //        self.NumbersWeeks([]);
        //        for (var i = 0; i < resp.length; i++) {

        //            self.NumbersWeeks.push(resp[i]);

        //        };
        //    }, onError);
        //};


        self.SetMyDateByWeek = function(weeknumber, yearnumber) {
            var firstDay = new Date(yearnumber, 0, 1).getDay();
            var d = new Date("Jan 01, " + yearnumber + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (weeknumber - 1);
            self.myDate(new Date(w));
        }.bind(self);

        var loadUserWeekOrder = function(numweek, year) {

            app.su_Service.LoadUserWeekOrder(numweek, year).then(function(resp) {
                self.MFD_models([]);

                self.OrderId(resp.OrderId);
                self.UserId(resp.userId);
                self.WeekIsPaid(resp.weekIsPaid);
                self.WeekNumber(resp.weekNumber);
                self.Year(resp.yearNumber);
                ko.utils.arrayForEach(resp.mfD_models, function(object) {

                    self.MFD_models.push(new menuForDayInfo(object, self.Categories()));

                });

            }, onError);

        }

        self.LoadUserWeekOrder = function(numweek, year) {

            loadUserWeekOrder(numweek, year);
        }

        self.NextWeekOrder = function() {
            var weekYear = new WeekYearModel(self.WeekNumber(), self.Year());
            app.su_Service.GetNextWeekYear(weekYear).then(function(resp) {

                self.SetMyDateByWeek(resp.week, resp.year);

            }, onError);

        }
        self.PrevWeekOrder = function() {
            var weekYear = new WeekYearModel(self.WeekNumber(), self.Year());
            app.su_Service.GetPrevWeekYear(weekYear).then(function(resp) {

                self.SetMyDateByWeek(resp.week, resp.year);
            }, onError);

        }
        self.GoToNextWeekOrder = function() {

            var curWeekYear = new WeekYearModel(self.CurrentWeekNumber(), self.Year());

            app.su_Service.GetNextWeekYear(curWeekYear).then(function(nextWeekYear) {
                self.loadWeekNumbers();
                self.SetMyDateByWeek(nextWeekYear.week, nextWeekYear.year);
            });
        };

        self.myDate.subscribe = ko.computed(function() {
            var takedWeek = self.myDate().getWeek() + 1;
            var curweek = self.WeekNumber();
            if (takedWeek !== curweek) {
                self.LoadUserWeekOrder(takedWeek, self.myDate().getFullYear());
            }
        }, self);

        self.GetCurrentWeekYear = function() {

            app.su_Service.GetCurrentWeekYear().then(function(resp) {

                self.CurrentWeekNumber(resp.week);
                self.Year(resp.year);

            }, onError);
        }

        self.IsCurrentWeek = ko.computed(function() {
            return self.CurrentWeekNumber() === self.WeekNumber();
        }.bind(self));


        self.DeleteNextWeekOrder = function() {
            var menuid = self.OrderId();
            app.su_Service.DeleteNextWeekOrder(menuid).then(function() {
                self.LoadUserWeekOrder();
                //self.loadWeekNumbers();
                self.SetMyDateByWeek(self.CurrentWeekNumber(), self.Year());
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



//        ko.observableArray.fn.pushAll = function (valuesToPush) {
//            var underlyingArray = this();
//            this.valueWillMutate();
//            ko.utils.arrayPushAll(underlyingArray, valuesToPush);
//            this.valueHasMutated();
//            return this;
//        };

//        var objForMap = function () {
//            this.categories = ["Первое блюдо", "Второе блюдо", "Салат", "Напиток"];
//            this.target = [];
//            this.sortFunc = function (value) {
//                for (var i = 0; i < 4; i++) {

//                    if (value.category == this.categories[i]) {

//                        this.target.push(new DishInfo(value));
//                    }
//                }
//            }
//        }


//        var viewModel = {
//            OrderId: ko.observable(),
//            CurrentWeekNumber: ko.observable(),
//            WeekNumber: ko.observable(),

//            MFD_models: ko.observableArray([]),

//            Message: ko.observable(""),

//            WeekTitle: ko.observable(""),

//            NumbersWeeks: ko.observableArray(),

//            BeenChanged: ko.observable(false),

//            ChangeSaved: ko.observable(false),

//            FirstCourseValues: [0, 0.5, 1.0, 2.0, 3.0, 4.0, 5.0],
//            QuantValues: [0, 1, 2, 3, 4, 5], 

//            OrderId : ko.observable(),
//            UserId : ko.observable(),
//            UserName : ko.observable(),
//            SummaryPrice : ko.observable(0),
//            WeekIsPaid : ko.observable(),
//            Year: ko.observable()
        
//        };
//    // Callback for error responses from the server.
//        function onError(error) {
//            self.Message("Error: " + error.status + " " + error.statusText);
//        }

//        viewModel.WeekTitle = ko.computed(function () {
//            var options = {
//                weekday: "short", year: "numeric", month: "short",
//                day: "numeric"
//            };
//            var year = viewModel.Year();
//            var firstDay = new Date(year, 0, 1).getDay();
//            //console.log(firstDay);
//            //var year = 2015;
//            var week = viewModel.WeekNumber();
//            var d = new Date("Jan 01, " + year + " 01:00:00");
//            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week - 1);
//            var n1 = new Date(w);
//            var n2 = new Date(w + 345600000);
//            return "Неделя " + week + ", " + n1.toLocaleDateString("ru-RU", options) + " - " + n2.toLocaleDateString("ru-RU", options);
//        }.bind(viewModel));

//        var DishInfo = function (dinfo) {

//            self = this;

//            self.DishId = ko.observable(dinfo.dishID);
//            self.Title = ko.observable(dinfo.title);
//            self.ProductImage = ko.observable(dinfo.productImage);
//            self.Price = ko.observable(dinfo.price.toFixed(2));
//            self.Category = ko.observable(dinfo.category);

//            self.isEditMode = ko.observable(false);
//            self.Quantity = ko.observable(dinfo.quantity);

//            self.clicked = function (item) {
//                $(item).focusin();
//            };
//            self.doubleClick = function () {
//                this.isEditMode(true);
//            };
//            self.onFocusOut = function () {
//                this.isEditMode(false);
//                viewModel.CalcSummary();
//            };

//        }

//    viewModel.SaveToServer = function() {
//        var source = {
//            ID: viewModel.OrderId(),
//            WeekNumber: viewModel.WeekNumber(),
//            MFD_models: this.MFD_models(),
//            SummaryPrice: this.SummaryPrice()
//        }

//        var objToServer = ko.toJSON(source);
//        $.ajax({
//            url: "/api/WeekMenu/" + this.WeekNumber(),
//            type: "put",
//            data: objToServer,
//            contentType: "application/json"
//        }).done(function() {

//            viewModel.BeenChanged(false);
//            viewModel.ChangeSaved(true);
//            ko.utils.arrayForEach(viewModel.MFD_models(), function(item) {
//                item.UnEditable();
//            });
//        }, onError);
//    };

//        viewModel.loadUserOrdersWeekNumbers = function () {
//            $.ajax({
//                url: "/api/Employee/WeekNumbers",
//                type: "GET"
//            }).done(function (resp) {

//                for (var i = 0; i < resp.length; i++) {

//                    viewModel.NumbersWeeks.push(resp[i]);

//                };

//            }, onError);
//        };



//        viewModel.LoadOrder = function (id,numweek, year) {

//            numweek = numweek == undefined ? "" : "/" + numweek;
//            year = year == undefined ? "" : "/" + year;
//            $.ajax({
//                url: "/api/Employee/" + id + numweek + year,
//                type: "GET"
//            }).done(function (resp) {

//                OrdersViewModel.UserOrders([]);
//                OrdersViewModel.WeekNumber(resp.weekNumber);

//                ko.utils.arrayForEach(resp.userOrders, function (object) {

//                    OrdersViewModel.UserOrders.push(new UserWeekOrder(object));

//                });

//            }, onError);
//        }

//        viewModel.LoadUserWeekOrder = function (numweek, year) {

//            numweek = numweek == undefined ? "" : numweek;
//            year = year == undefined ? "" : "/" + year;
//            $.ajax({
//                url: "/api/Employee/" + numweek + year,
//                type: "GET"
//            }).done(function (resp) {

//                viewModel.MFD_models([]);
//                viewModel.Year(resp.year);
//                viewModel.OrderId(resp.id);
//                viewModel.WeekNumber(resp.weekNumber);

//                ko.utils.arrayMap(resp.mfD_models, function (object) {
//                    var obj = new objForMap();
//                    object.dishes.map(obj.sortFunc, obj);
                   
//                    var MenuForDayInfo = {

//                        ID: ko.observable(object.id),
//                        DayOfWeek: ko.observable(object.dayOfWeek),
//                        Dishes: ko.observableArray(obj.target),
//                        Editing: ko.observable(false),
//                        TotalPrice: ko.observable(),
//                    }

//                    MenuForDayInfo.CalcTotal = function () {
//                        var sum = 0;
//                        var valsum;
//                        for (var i = 0; i < MenuForDayInfo.Dishes().length; i++) {

//                            valsum = parseFloat(MenuForDayInfo.Dishes()[i].Price());
//                            sum += valsum * MenuForDayInfo.Dishes()[i].Quantity();
//                        };
//                        MenuForDayInfo.TotalPrice(sum.toFixed(2));

//                    }.bind(MenuForDayInfo);
//                    viewModel.MFD_models.push(MenuForDayInfo);
//                });
//                var summary = 0;
//                ko.utils.arrayForEach(viewModel.MFD_models(), function (item, index) {
//                    ko.utils.arrayForEach(item.Dishes(), function(value, key) {

//                        value.Quantity(resp.dishquantities[index * 4 + key]);
//                    });
//                    item.CalcTotal();
//                    summary += parseFloat(item.TotalPrice());
//                });
//                viewModel.SummaryPrice(summary.toFixed(2));
//            }).error(function (err) {
//                viewModel.Message("Error! " + err.status);
//            });
//        }
//        viewModel.GetCurrentWeekYear = function () {

//            app.su_Service.GetCurrentWeekYear().then(function(resp) {
//                viewModel.CurrentWeekNumber(resp.week);
//                viewModel.Year(resp.year);
//            },onError);
//        }

//        viewModel.IsCurrentWeek = ko.computed(function () {
//            return viewModel.CurrentWeekNumber() == viewModel.WeekNumber();
//        }.bind(viewModel));

//        viewModel.applyChanges = function () {

//            var obj = new objForMap();
//            var catIndex = $.map(obj.categories, function (n, i) {
//                if (viewModel.Category() == n)
//                    return i;
//            });


//            var dish = ko.utils.arrayFirst(viewModel.DishesByCategory(), function (value) {
//                if (value.DishId() == viewModel.SelectedDish()) {
//                    return value;
//                }
//            });
//            if (dish != undefined) {

//                viewModel.MFD_models()[viewModel.UpdatableMFD()].Dishes()[catIndex].UpdateWeekMenu(dish);
//                this.CalcSummary();
//            };



//            $("#modalbox").modal("hide");
//        }


//        viewModel.CalcSummary = function () {

//            var sum = 0, buf; 
//            for (var ind = 0; ind < viewModel.MFD_models().length; ind++) {
//                viewModel.MFD_models()[ind].CalcTotal();
//                buf = viewModel.MFD_models()[ind].TotalPrice();
//                sum += parseFloat(buf);

//            }

//            this.SummaryPrice(sum.toFixed(2));
//        }.bind(viewModel);


//        viewModel.LoadUserWeekOrder();
//        viewModel.loadWeekNumbers();
//        viewModel.GetCurrentWeekYear();

//        ko.applyBindings(viewModel);

//        ko.bindingHandlers.singleClick = {
//            init: function (element, valueAccessor) {
//                var handler = valueAccessor(),
//                    delay = 400,
//                    clickTimeout = false;

//                $(element).click(function () {
//                    if (clickTimeout !== false) {
//                        clearTimeout(clickTimeout);
//                        clickTimeout = false;
//                    } else {
//                        clickTimeout = setTimeout(function () {
//                            clickTimeout = false;
//                            handler();
//                        }, delay);
//                    }
//                });
//            }
//        };
//}())