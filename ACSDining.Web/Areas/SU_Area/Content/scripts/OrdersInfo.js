/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />


(function() {


    var QuantValueModel = function(value) {

        var self = this;
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
        FirstCourseValues: [0, 0.5, 1, 2, 3,  4, 5],
        QuantValues: [0, 1, 2, 3, 4, 5],
        BeenChanged: ko.observable(false),
        Year:ko.observable(),
        Categories :ko.observableArray([])
    }
    // Callback for error responses from the server.
    function onError(error) {
        OrdersViewModel.Message('Error: ' + error.status + ' ' + error.statusText);
    }

    OrdersViewModel.CalcSummary = function(item) {
        var source = {
            UserId: item.UserId(),
            UserName: item.UserName(),
            SummaryPrice: item.SummaryPrice(),
            WeekIsPaid: item.WeekIsPaid(),
            Dishquantities: $.map(item.Dishquantities(), function(value) {
                return value.Quantity;
            })
        };
        app.su_Service.GetOrderSummary(OrdersViewModel.WeekNumber(), OrdersViewModel.Year(), source).then(
            function(data) {
                item.SummaryPrice(data.toFixed(2));
            },
            function(error) {
                onError(error);
            });

    }.bind(OrdersViewModel);

        
    
    var UserWeekOrder = function(item) {

        var self = this;

        self.UserId = ko.observable(item.userId);
        self.UserName = ko.observable(item.userName);
        self.SummaryPrice = ko.observable(item.orderSummaryPrice.toFixed(2));
        self.WeekIsPaid = ko.observable(item.weekIsPaid);

        self.Dishquantities = ko.observableArray(ko.utils.arrayMap(item.dishquantities, function(value) {
            return new QuantValueModel(value);
        }));


    }

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

    OrdersViewModel.loadWeekNumbers = function () {
        app.su_Service.LoadWeekNumbers().then(function (resp) {

            OrdersViewModel.NumbersOfWeek.pushAll(resp);

        }, onError);
    };


    OrdersViewModel.LoadOrders = function () {

        app.su_Service.LoadWeekOrders(OrdersViewModel.WeekNumber(), OrdersViewModel.Year()).then(
           function (resp) {
               OrdersViewModel.UserOrders([]);
               OrdersViewModel.WeekNumber(resp.weekNumber);
               OrdersViewModel.Year(resp.yearNumber);

               ko.utils.arrayForEach(resp.userOrders, function (object) {

                   OrdersViewModel.UserOrders.push(new UserWeekOrder(object));

               });
           },
           function (error) {
               onError(error);
           });
    }

    OrdersViewModel.GetCurrentWeekNumber = function () {

        app.su_Service.GetCurrentWeekNumber().then(function (resp) {
            OrdersViewModel.CurrentWeekNumber(resp);

        }, onError);
    }

    OrdersViewModel.IsCurrentWeek = ko.computed(function() {
        return OrdersViewModel.CurrentWeekNumber() == OrdersViewModel.WeekNumber();
    }.bind(OrdersViewModel));


    app.su_Service.GetCategories().then(function (resp) {
        OrdersViewModel.Categories.pushAll(resp);
    }, onError);

    OrdersViewModel.LoadOrders();
    OrdersViewModel.loadWeekNumbers();
    OrdersViewModel.GetCurrentWeekNumber();

 
    ko.applyBindings(OrdersViewModel);
    

})();

