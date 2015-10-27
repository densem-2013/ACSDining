/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />


(function() {

    ko.observableArray.fn.pushAll = function (valuesToPush) {
        var underlyingArray = this();
        this.valueWillMutate();
        ko.utils.arrayPushAll(underlyingArray, valuesToPush);
        this.valueHasMutated();
        return this;
    };

    var UserWeekOrder = function (item) {

        self = this;

        self.UserId = ko.observable(item.userId);
        self.UserName = ko.observable(item.userName);
        self.Dishquantities = ko.observableArray(item.dishquantities);
        self.WeekIsPaid = ko.observable(item.weekIsPaid);
    }

    OrdersViewModel = {
        Id: ko.observable(),
        UserOrders: ko.observableArray([]),
        WeekNumber: ko.observable(),
        NumbersOfWeek: ko.observableArray([]),
        Message: ko.observable(),
        CurrentWeekNumber:ko.observable()

    }

    OrdersViewModel.DishCategories = ["Первое блюдо", "Второе блюдо", "Салат", "Напиток"];
    OrdersViewModel.DaysOfWeek = ["Понедельник", "Вторник", "Среда", "Четверг", "Пятница"];

    OrdersViewModel.pageSize = ko.observable(15);

    OrdersViewModel.pageIndex = ko.observable(0);

    OrdersViewModel.pagedList = ko.dependentObservable(function () {
        var size = OrdersViewModel.pageSize();
        var start = OrdersViewModel.pageIndex() * size;
        return OrdersViewModel.UserOrders.slice(start, start + size);
    });

    OrdersViewModel.maxPageIndex = ko.dependentObservable(function () {
        return Math.ceil(OrdersViewModel.UserOrders().length / OrdersViewModel.pageSize()) - 1;
    });

    OrdersViewModel.previousPage = function () {
        if (OrdersViewModel.pageIndex() > 0) {
            OrdersViewModel.pageIndex(OrdersViewModel.pageIndex() - 1);
        }
    };

    OrdersViewModel.nextPage = function () {
        if (OrdersViewModel.pageIndex() < OrdersViewModel.maxPageIndex()) {
            OrdersViewModel.pageIndex(OrdersViewModel.pageIndex() + 1);
        }
    };

    OrdersViewModel.allPages = ko.dependentObservable(function () {
        var pages = [];
        for (i = 0; i <= OrdersViewModel.maxPageIndex() ; i++) {
            pages.push({ pageNumber: (i + 1) });
        }
        return pages;
    });

    OrdersViewModel.moveToPage = function (index) {
        OrdersViewModel.pageIndex(index);
    };

    OrdersViewModel.loadWeekNumbers = function () {
        $.ajax({
            url: "/api/WeekMenu/WeekNumbers",
            type: "GET"
        }).done(function (resp) {

            OrdersViewModel.NumbersOfWeek.pushAll(resp);

        }).error(function (err) {
            OrdersViewModel.Message("Error! " + err.status);
        });
    };

    OrdersViewModel.LoadOrders = function (numweek, year) {

        numweek = numweek == undefined ? '' : numweek;
        year = year == undefined ? '' : "/" + year;
        $.ajax({
            url: "/api/Orders/" + numweek + year,
            type: "GET"
        }).done(function (resp) {

            OrdersViewModel.UserOrders([]);

            OrdersViewModel.Id(resp.id);

            ko.utils.arrayForEach(resp.userOrders, function (object) {

                OrdersViewModel.UserOrders.push(new UserWeekOrder(object));

            });

        }).error(function (err) {
            OrdersViewModel.Message("Error! " + err.status);
        });
    }
    OrdersViewModel.GetCurrentWeekNumber = function () {

        $.ajax({
            url: "/api/WeekMenu/CurrentWeek",
            type: "GET"
        }).done(function (resp) {
            OrdersViewModel.CurrentWeekNumber(resp);
        });
    }

    OrdersViewModel.IsCurrentWeek = ko.computed(function () {
        return OrdersViewModel.CurrentWeekNumber() == OrdersViewModel.WeekNumber();
    }.bind(OrdersViewModel));

    OrdersViewModel.LoadOrders();
    OrdersViewModel.loadWeekNumbers();
    OrdersViewModel.GetCurrentWeekNumber();

    ko.applyBindings(OrdersViewModel);
})();