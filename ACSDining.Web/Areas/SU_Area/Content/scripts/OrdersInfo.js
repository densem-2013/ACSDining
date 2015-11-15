/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />


(function() {

    ko.observableArray.fn.pushAll = function(valuesToPush) {
        var underlyingArray = this();
        this.valueWillMutate();
        ko.utils.arrayPushAll(underlyingArray, valuesToPush);
        this.valueHasMutated();
        return this;
    };

    var QuantValueModel = function(value) {
        self = this;
        //self.parent = parent;
        self.isEditMode = ko.observable(false);
        self.Quantity = ko.observable(value);

        self.clicked = function(item) {
            $(item).focusin();
        };
        self.doubleClick = function() {
            this.isEditMode(true);
        };
        self.onFocusOut = function() {
            this.isEditMode(false);
        };
    }

    OrdersViewModel = {
        UserOrders: ko.observableArray([]),
        WeekNumber: ko.observable(),
        NumbersOfWeek: ko.observableArray([]),
        Message: ko.observable(),
        CurrentWeekNumber: ko.observable(),
        myDate: ko.observable(new Date()),
        FirstCourseValues: [0, 0.5, 1.0, 2.0, 3.0,  4.0, 5.0],
        QuantValues: [0, 1, 2, 3, 4, 5]
    }

    var UserWeekOrder = function(item) {

        self = this;

        self.UserId = ko.observable(item.userId);
        self.UserName = ko.observable(item.userName);
        self.SummaryPrice = ko.observable(item.summaryPrice.toFixed(2));
        self.WeekIsPaid = ko.observable(item.weekIsPaid);

        self.CalcSummary = function() {
            var parent = this;
            var source = {
                UserId: this.UserId(),
                Dishquantities: $.map(this.Dishquantities(), function(value) {
                    return value.Quantity;
                })
            };

            var objToServer = ko.toJSON(source);
            $.ajax({
                url: '/api/Orders/summary/' + OrdersViewModel.WeekNumber(),
                type: 'put',
                data: objToServer,
                contentType: 'application/json'
            }).done(function(data) {

                parent.SummaryPrice(data.toFixed(2));

            }).error(function(err) {
                OrdersViewModel.Message("Error! " + err.status);
            });

        }.bind(self);
        self.Dishquantities = ko.observableArray(ko.utils.arrayMap(item.dishquantities, function(value) {
            return new QuantValueModel(value);
        }));


    }

    OrdersViewModel.DishCategories = ["Первое блюдо", "Второе блюдо", "Салат", "Напиток"];
    OrdersViewModel.DaysOfWeek = ["Понедельник", "Вторник", "Среда", "Четверг", "Пятница"];

    OrdersViewModel.pageSize = ko.observable(10);

    OrdersViewModel.pageIndex = ko.observable(0);

    OrdersViewModel.pagedList = ko.dependentObservable(function() {
        var size = OrdersViewModel.pageSize();
        var start = OrdersViewModel.pageIndex() * size;
        return OrdersViewModel.UserOrders.slice(start, start + size);
    });

    OrdersViewModel.maxPageIndex = ko.dependentObservable(function() {
        return Math.ceil(OrdersViewModel.UserOrders().length / OrdersViewModel.pageSize()) - 1;
    });

    OrdersViewModel.previousPage = function() {
        if (OrdersViewModel.pageIndex() > 0) {
            OrdersViewModel.pageIndex(OrdersViewModel.pageIndex() - 1);
        }
    };

    OrdersViewModel.nextPage = function() {
        if (OrdersViewModel.pageIndex() < OrdersViewModel.maxPageIndex()) {
            OrdersViewModel.pageIndex(OrdersViewModel.pageIndex() + 1);
        }
    };

    OrdersViewModel.allPages = ko.dependentObservable(function() {
        var pages = [];
        for (i = 0; i <= OrdersViewModel.maxPageIndex(); i++) {
            pages.push({ pageNumber: (i + 1) });
        }
        return pages;
    });

    OrdersViewModel.moveToPage = function(index) {
        OrdersViewModel.pageIndex(index);
    };

    OrdersViewModel.loadWeekNumbers = function() {
        $.ajax({
            url: "/api/WeekMenu/WeekNumbers",
            type: "GET"
        }).done(function(resp) {

            OrdersViewModel.NumbersOfWeek.pushAll(resp);

        }).error(function(err) {
            OrdersViewModel.Message("Error! " + err.status);
        });
    };

    OrdersViewModel.LoadOrders = function(numweek, year) {

        numweek = numweek == undefined ? '' : numweek;
        year = year == undefined ? '' : "/" + year;
        $.ajax({
            url: "/api/Orders/" + numweek + year,
            type: "GET"
        }).done(function(resp) {

            OrdersViewModel.UserOrders([]);
            OrdersViewModel.WeekNumber(resp.weekNumber);

            ko.utils.arrayForEach(resp.userOrders, function(object) {

                OrdersViewModel.UserOrders.push(new UserWeekOrder(object));

            });

        }).error(function(err) {
            OrdersViewModel.Message("Error! " + err.status);
        });
    }
    OrdersViewModel.GetCurrentWeekNumber = function() {

        $.ajax({
            url: "/api/WeekMenu/CurrentWeek",
            type: "GET"
        }).done(function(resp) {
            OrdersViewModel.CurrentWeekNumber(resp);
        });
    }

    OrdersViewModel.IsCurrentWeek = ko.computed(function() {
        return OrdersViewModel.CurrentWeekNumber() == OrdersViewModel.WeekNumber();
    }.bind(OrdersViewModel));

    OrdersViewModel.LoadOrders();
    OrdersViewModel.loadWeekNumbers();
    OrdersViewModel.GetCurrentWeekNumber();

    ko.bindingHandlers.datepicker = {
        init: function (element, valueAccessor, allBindingsAccessor) {

            var options = $.extend(
                {},
                $.datepicker.regional["ru"],
                {
                    dateFormat: 'dd/mm/yy',
                    showButtonPanel: true,
                    gotoCurrent: true,
                    showOtherMonths: true,
                    selectOtherMonths: true,
                    showWeek: true,
                    constraintInput: true,
                    showAnim: "slideDown",
                    hideIfNoPrevNext: true,
                    onClose: function (dateText, inst) {
                        $(this).blur();
                    }
                }
            );
            $(element).datepicker(options);

            ko.utils.registerEventHandler($(element), "change", function () {
                var observable = valueAccessor();
                observable($(element).datepicker("getDate"));
            });

            ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
                $(element).datepicker("destroy");
            });

        },
        UpdateWeekMenu: function (element, valueAccessor) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            $(element).datepicker("setDate", value);
        }
    };

    ko.bindingHandlers.singleClick = {
        init: function(element, valueAccessor) {
            var handler = valueAccessor(),
                delay = 400,
                clickTimeout = false;

            $(element).click(function() {
                if (clickTimeout !== false) {
                    clearTimeout(clickTimeout);
                    clickTimeout = false;
                } else {
                    clickTimeout = setTimeout(function() {
                        clickTimeout = false;
                        handler();
                    }, delay);
                }
            });
        }
    };
    ko.applyBindings(OrdersViewModel);
    
    /*Date picker value binder for knockout*/
    ko.bindingHandlers.datepicker = {
        init: function (element, valueAccessor, allBindingsAccessor) {
            var options = allBindingsAccessor().datepickerOptions || {};
            $(element).datepicker(options).on("changeDate", function (ev) {
                var observable = valueAccessor();
                observable(ev.date);
                $(element).hide();
            });
        },
        UpdateWeekMenu: function (element, valueAccessor) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            $(element).datepicker("setValue", value);
        }
    };


})();

