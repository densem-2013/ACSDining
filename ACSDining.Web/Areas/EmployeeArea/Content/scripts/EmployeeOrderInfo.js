(function() {
    
    /// <reference path="../jquery-2.1.3.min.js" />
    /// <reference path="../knockout-3.2.0.js" />

        ko.observableArray.fn.pushAll = function (valuesToPush) {
            var underlyingArray = this();
            this.valueWillMutate();
            ko.utils.arrayPushAll(underlyingArray, valuesToPush);
            this.valueHasMutated();
            return this;
        };

        var objForMap = function () {
            this.categories = ["Первое блюдо", "Второе блюдо", "Салат", "Напиток"];
            this.target = [];
            this.sortFunc = function (value) {
                for (var i = 0; i < 4; i++) {

                    if (value.category == this.categories[i]) {

                        this.target.push(new DishInfo(value));
                    }
                }
            }
        }


        var viewModel = {
            MenuId: ko.observable(),
            CurrentWeekNumber: ko.observable(),
            WeekNumber: ko.observable(),

            MFD_models: ko.observableArray([]),

            Message: ko.observable(""),


            NumbersWeeks: ko.observableArray(),

            BeenChanged: ko.observable(false),

            ChangeSaved: ko.observable(false),

            FirstCourseValues: [0, 0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0, 4.5, 5.0],
            QuantValues: [0, 1, 2, 3, 4, 5], 

            OrderId : ko.observable(),
            UserId : ko.observable(),
            UserName : ko.observable(),
            SummaryPrice : ko.observable(0),
            WeekIsPaid : ko.observable()

        
        };

        var DishInfo = function (dinfo) {

            self = this;

            self.DishId = ko.observable(dinfo.dishID);
            self.Title = ko.observable(dinfo.title);
            self.ProductImage = ko.observable(dinfo.productImage);
            self.Price = ko.observable(dinfo.price.toFixed(2));
            self.Category = ko.observable(dinfo.category);

            self.isEditMode = ko.observable(false);
            self.Quantity = ko.observable(0);

            self.clicked = function (item) {
                $(item).focus();
            };
            self.doubleClick = function () {
                this.isEditMode(true);
            };
            self.onFocusOut = function () {
                this.isEditMode(false);
                viewModel.CalcSummary();
            };

        }

        viewModel.SaveToServer = function () {
            var source = {
                ID: viewModel.MenuId(),
                WeekNumber: viewModel.WeekNumber(),
                MFD_models: this.MFD_models(),
                SummaryPrice: this.SummaryPrice()

            }

            var objToServer = ko.toJSON(source);
            $.ajax({
                url: '/api/WeekMenu/' + this.WeekNumber(),
                type: 'put',
                data: objToServer,
                contentType: 'application/json'
            }).done(function () {

                viewModel.BeenChanged(false);
                viewModel.ChangeSaved(true);
                ko.utils.arrayForEach(viewModel.MFD_models(), function (item) {
                    item.UnEditable();
                });
            }).error(function (err) {
                viewModel.Message("Error! " + err.status);
            });
        };

        viewModel.loadWeekNumbers = function () {
            $.ajax({
                url: "/api/Employee/WeekNumbers",
                type: "GET"
            }).done(function (resp) {

                for (var i = 0; i < resp.length; i++) {

                    viewModel.NumbersWeeks.push(resp[i]);

                };

            }).error(function (err) {
                viewModel.Message("Error! " + err.status);
            });
        };



        viewModel.LoadOrder = function (id,numweek, year) {

            numweek = numweek == undefined ? '' : "/" + numweek;
            year = year == undefined ? '' : "/" + year;
            $.ajax({
                url: "/api/Employee/" + id + numweek + year,
                type: "GET"
            }).done(function (resp) {

                OrdersViewModel.UserOrders([]);
                OrdersViewModel.WeekNumber(resp.weekNumber);

                ko.utils.arrayForEach(resp.userOrders, function (object) {

                    OrdersViewModel.UserOrders.push(new UserWeekOrder(object));

                });

            }).error(function (err) {
                OrdersViewModel.Message("Error! " + err.status);
            });
        }

        viewModel.LoadWeekMenu = function (numweek, year) {

            numweek = numweek == undefined ? '' : numweek;
            year = year == undefined ? '' : "/" + year;
            $.ajax({
                url: "/api/Employee/" + numweek + year,
                type: "GET"
            }).done(function (resp) {

                viewModel.MFD_models();

                viewModel.MenuId(resp.id);
                viewModel.WeekNumber(resp.weekNumber);

                ko.utils.arrayMap(resp.mfD_models, function (object) {
                    var obj = new objForMap();
                    object.dishes.map(obj.sortFunc, obj);

                    var MenuForDayInfo = {

                        ID: ko.observable(object.id),
                        DayOfWeek: ko.observable(object.dayOfWeek),
                        Dishes: ko.observableArray(obj.target),
                        Editing: ko.observable(false),
                        TotalPrice: ko.observable(),

                        Editable: function () {
                            this.Editing(true);
                        },

                        UnEditable: function () {
                            this.Editing(false);
                        }


                    }

                    MenuForDayInfo.CalcTotal = function () {
                        var sum = 0;
                        var valsum;
                        for (var i = 0; i < MenuForDayInfo.Dishes().length; i++) {

                            valsum = parseFloat(MenuForDayInfo.Dishes()[i].Price());
                            sum += valsum * MenuForDayInfo.Dishes()[i].Quantity();
                        };
                        MenuForDayInfo.TotalPrice(sum.toFixed(2));

                    }.bind(MenuForDayInfo);
                    viewModel.MFD_models.push(MenuForDayInfo);
                });


            }).error(function (err) {
                viewModel.Message("Error! " + err.status);
            });
        }
        viewModel.GetCurrentWeekNumber = function () {

            $.ajax({
                url: "/api/Employee/CurrentWeek",
                type: "GET"
            }).done(function (resp) {
                viewModel.CurrentWeekNumber(resp);
            });
        }

        viewModel.IsCurrentWeek = ko.computed(function () {
            return viewModel.CurrentWeekNumber() == viewModel.WeekNumber();
        }.bind(viewModel));

        viewModel.applyChanges = function () {

            var obj = new objForMap();
            var catIndex = $.map(obj.categories, function (n, i) {
                if (viewModel.Category() == n)
                    return i;
            });


            var dish = ko.utils.arrayFirst(viewModel.DishesByCategory(), function (value) {
                if (value.DishId() == viewModel.SelectedDish()) {
                    return value;
                }
            });
            if (dish != undefined) {

                viewModel.MFD_models()[viewModel.UpdatableMFD()].Dishes()[catIndex].update(dish);
                this.CalcSummary();
            };



            $("#modalbox").modal("hide");
        }


        viewModel.CalcSummary = function () {

            var sum = 0, buf; 
            for (var ind = 0; ind < viewModel.MFD_models().length; ind++) {
                viewModel.MFD_models()[ind].CalcTotal();
                buf = viewModel.MFD_models()[ind].TotalPrice();
                sum += parseFloat(buf);

            }

            this.SummaryPrice(sum.toFixed(2));
        }.bind(viewModel);


        viewModel.LoadWeekMenu();
        viewModel.loadWeekNumbers();
        viewModel.GetCurrentWeekNumber();

        ko.applyBindings(viewModel);

        ko.bindingHandlers.singleClick = {
            init: function (element, valueAccessor) {
                var handler = valueAccessor(),
                    delay = 400,
                    clickTimeout = false;

                $(element).click(function () {
                    if (clickTimeout !== false) {
                        clearTimeout(clickTimeout);
                        clickTimeout = false;
                    } else {
                        clickTimeout = setTimeout(function () {
                            clickTimeout = false;
                            handler();
                        }, delay);
                    }
                });
            }
        };
}())