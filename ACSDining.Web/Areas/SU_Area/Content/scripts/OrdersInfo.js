/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />
/// <reference path="~/Scripts/jquery-ui-i18n.min.js" />
/// <reference path="~/Areas/AdminArea/Content/scripts/app.service.js" />
(function() {


    var quantValueModel = function(value) {

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

    var ordersViewModel = {
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
    };
    // Callback for error responses from the server.
    function onError(error) {
        ordersViewModel.Message('Error: ' + error.status + ' ' + error.statusText);
    }

    ordersViewModel.CalcSummary = function(item) {
        var source = {
            UserId: item.UserId(),
            UserName: item.UserName(),
            SummaryPrice: item.SummaryPrice(),
            WeekIsPaid: item.WeekIsPaid(),
            Dishquantities: $.map(item.Dishquantities(), function(value) {
                return value.Quantity;
            })
        };
        app.su_Service.GetOrderSummary(ordersViewModel.WeekNumber(), ordersViewModel.Year(), source).then(
            function(data) {
                item.SummaryPrice(data.toFixed(2));
            },
            function(error) {
                onError(error);
            });

    }.bind(ordersViewModel);

        
    
    var userWeekOrder = function(item) {

        var self = this;

        self.UserId = ko.observable(item.userId);
        self.UserName = ko.observable(item.userName);
        self.SummaryPrice = ko.observable(item.orderSummaryPrice.toFixed(2));
        self.WeekIsPaid = ko.observable(item.weekIsPaid);

        self.Dishquantities = ko.observableArray(ko.utils.arrayMap(item.dishquantities, function(value) {
            return new quantValueModel(value);
        }));


    }

    ordersViewModel.DaysOfWeek = ["Понедельник", "Вторник", "Среда", "Четверг", "Пятница"];

    ordersViewModel.pageSize = ko.observable(10);

    ordersViewModel.pageIndex = ko.observable(0);

    ordersViewModel.pagedList = ko.dependentObservable(function() {
        var size = ordersViewModel.pageSize();
        var start = ordersViewModel.pageIndex() * size;
        return ordersViewModel.UserOrders.slice(start, start + size);
    });

    ordersViewModel.maxPageIndex = ko.dependentObservable(function() {
        return Math.ceil(ordersViewModel.UserOrders().length / ordersViewModel.pageSize()) - 1;
    });

    ordersViewModel.previousPage = function() {
        if (ordersViewModel.pageIndex() > 0) {
            ordersViewModel.pageIndex(ordersViewModel.pageIndex() - 1);
        }
    };

    ordersViewModel.nextPage = function() {
        if (ordersViewModel.pageIndex() < ordersViewModel.maxPageIndex()) {
            ordersViewModel.pageIndex(ordersViewModel.pageIndex() + 1);
        }
    };

    ordersViewModel.allPages = ko.dependentObservable(function() {
        var pages = [];
        for (var i = 0; i <= ordersViewModel.maxPageIndex(); i++) {
            pages.push({ pageNumber: (i + 1) });
        }
        return pages;
    });

    ordersViewModel.moveToPage = function(index) {
        ordersViewModel.pageIndex(index);
    };

    ordersViewModel.loadWeekNumbers = function () {
        app.su_Service.LoadWeekNumbers().then(function (resp) {

            ordersViewModel.NumbersOfWeek.pushAll(resp);

        }, onError);
    };


    ordersViewModel.LoadOrders = function () {

        app.su_Service.LoadWeekOrders(ordersViewModel.WeekNumber(), ordersViewModel.Year()).then(
           function (resp) {
               ordersViewModel.UserOrders([]);
               ordersViewModel.WeekNumber(resp.weekNumber);
               ordersViewModel.Year(resp.yearNumber);

               ko.utils.arrayForEach(resp.userOrders, function (object) {

                   ordersViewModel.UserOrders.push(new userWeekOrder(object));

               });
           },
           function (error) {
               onError(error);
           });
    }

    ordersViewModel.GetCurrentWeekYear = function () {

        app.su_Service.GetCurrentWeekYear().then(function (resp) {
            ordersViewModel.CurrentWeekNumber(resp);

        }, onError);
    }

    ordersViewModel.IsCurrentWeek = ko.computed(function() {
        return ordersViewModel.CurrentWeekNumber() === ordersViewModel.WeekNumber();
    }.bind(ordersViewModel));


    app.su_Service.GetCategories().then(function (resp) {
        ordersViewModel.Categories.pushAll(resp);
    }, onError);

    ordersViewModel.LoadOrders();
    ordersViewModel.loadWeekNumbers();
    ordersViewModel.GetCurrentWeekYear();

 
    ko.applyBindings(ordersViewModel);
    

})();

