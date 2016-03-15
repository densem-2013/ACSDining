window.app = window.todoApp || {};

ko.observableArray.fn.pushAll = function (valuesToPush) {
    var underlyingArray = this();
    this.valueWillMutate();
    ko.utils.arrayPushAll(underlyingArray, valuesToPush);
    this.valueHasMutated();
    return this;
};
Date.prototype.getWeek = function () {
    var onejan = new Date(this.getFullYear(), 0, 1);
    var today = new Date(this.getFullYear(), this.getMonth(), this.getDate());
    var dayOfYear = ((today - onejan + 1) / 86400000);
    return Math.ceil(dayOfYear / 7);
};


ko.bindingHandlers.datepicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {

        var options = $.extend(
            {},
            $.datepicker.regional["ru"],
            {
                beforeShowDay: $.datepicker.noWeekends,
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
    var WeekYearModel=function(week, year) {
        var self = this;
        self.Week = ko.observable(week);
        self.Year = ko.observable(year);
    }

window.app.su_Service = (function() {

    var baseWeekMenuUri = '/api/WeekMenu/';
    var serviceWeekMenuUrls = {
        weekMenu: function(numweek, year) {
            numweek = numweek == undefined ? '' : numweek;
            year = year == undefined ? '' : "/" + year;
            return baseWeekMenuUri + numweek + year;
        },
        weekNumbers: function() { return baseWeekMenuUri + 'WeekNumbers'; },
        currentweek: function() { return baseWeekMenuUri + 'curWeekNumber'; },
        categories: function() { return baseWeekMenuUri + 'categories' },
        create: function() { return baseWeekMenuUri + 'create' },
        nextWeekMenu: function() { return baseWeekMenuUri + 'nextWeekMenu' },
        nextWeekYear: function() { return baseWeekMenuUri + 'nextWeekYear' },
        prevWeekYear: function() { return baseWeekMenuUri + 'prevWeekYear' },
        deleteWeekMenu: function(numweek) { return baseWeekMenuUri + 'delete/' + numweek }
    }

    var baseOrdersUri = '/api/Orders/';
    var serviceOrdersUrls = {
        ordersParams: function(numweek, year) {
            numweek = numweek == undefined ? '' : numweek;
            year = year == undefined ? '' : "/" + year;
            return numweek + year;
        },
        updateOrder: function(week, year) { return baseOrdersUri + 'update' + serviceOrdersUrls.ordersParams(week, year) },
        createOrder: function(week, year) { return baseOrdersUri + 'create' + serviceOrdersUrls.ordersParams(week, year) },
        calcsummary: function(week, year) { return baseOrdersUri + 'summary/' + serviceOrdersUrls.ordersParams(week, year) }
    }
    var baseDishesUri = '/api/Dishes/';
    var serviceDishesUrls = {
        byCategory: function(category) { return baseDishesUri + 'byCategory/' + category },
        update: function() { return baseDishesUri + 'update' },
        deleteDish: function(dishID) { return baseDishesUri + 'delete/' + dishID },
        create: function() { return baseDishesUri + 'create' }
    }

    var basePaimentsUri = '/api/Paiment/';
    var servicePaimentsUrls = {
        paiments: function(week, year) { return basePaimentsUri + serviceOrdersUrls.ordersParams(week, year) },
        updatePaiment: function(orderid) { return basePaimentsUri + 'updatePaiment/' + orderid },
        totalPaimentsbyDish: function(week, year) { return basePaimentsUri + 'paimentsByDish/' + serviceOrdersUrls.ordersParams(week, year) }
    }
    var baseAccountsUri = '/api/Account/';
    var serviseAccountsUrls = {
        accounts: function() { return baseAccountsUri + 'All' },
        deleteAccount: function(id) { return baseAccountsUri + 'delete/' + id }
    }
    var baseWorkDaysUri = '/api/WorkDays/';
    var serviseWorkDaysUrls = {
        workDays: function(week, year) { return baseWorkDaysUri + serviceOrdersUrls.ordersParams(week, year) },
        updateWorkDays: function(workweekid) { return baseAccountsUri + 'update/' + workweekid }
    }

    function ajaxRequest(type, url, data) {
        var options = {
            url: url,
            headers: {
                Accept: "application/json"
            },
            contentType: "application/json",
            cache: false,
            type: type,
            data: data ? ko.toJSON(data) : null
        };
        return $.ajax(options);
    }


    return {
        LoadWeekMenu: function(numweek, year) {
            return ajaxRequest('get', serviceWeekMenuUrls.weekMenu(numweek, year));
        },
        GetNextWeekYear: function(item) {
            return ajaxRequest('put', serviceWeekMenuUrls.nextWeekYear(), item);
        },
        GetPrevWeekYear: function(item) {
            return ajaxRequest('put', serviceWeekMenuUrls.prevWeekYear(), item);
        },
        GetCurrentWeekNumber: function() {
            return ajaxRequest('get', serviceWeekMenuUrls.currentweek());
        },
        LoadWeekNumbers: function() {
            return ajaxRequest('get', serviceWeekMenuUrls.weekNumbers());
        },
        UpdateWeekMenu: function(item) {
            return ajaxRequest('put', baseWeekMenuUri + 'update', item);
        },
        DeleteNextWeekMenu: function(numweek) {
            return ajaxRequest('delete', serviceWeekMenuUrls.deleteWeekMenu(numweek));
        },
        GetNextWeekMenu: function() {
            return ajaxRequest('get', serviceWeekMenuUrls.nextWeekMenu());
        },
        CreateNextWeekMenu: function() {
            return ajaxRequest('post', serviceWeekMenuUrls.create());
        },
        GetCategories: function() {
            return ajaxRequest('get', serviceWeekMenuUrls.categories());
        },
        CreateDish: function(dish) {
            return ajaxRequest('post', serviceDishesUrls.create(), dish);
        },
        DishesByCategory: function(category) {
            return ajaxRequest('get', serviceDishesUrls.byCategory(category));
        },
        UpdateDish: function(dish) {
            return ajaxRequest('put', serviceDishesUrls.update(), dish);
        },
        DeleteDish: function(dishID) {
            return ajaxRequest('delete', serviceDishesUrls.deleteDish(dishID));
        },
        LoadWeekOrders: function(numweek, year) {
            return ajaxRequest('get', baseOrdersUri + serviceOrdersUrls.ordersParams(numweek, year));
        },
        GetOrderSummary: function(week, year, item) {
            return ajaxRequest('put', serviceOrdersUrls.calcsummary(week, year), item);
        },
        UpdateOrder: function(week, year, item) {
            return ajaxRequest('put', serviceOrdersUrls.updateOrder(week, year), item);
        },
        CreateOrdersNextweek: function(week, year) {
            return ajaxRequest('post', serviceOrdersUrls.createOrder(week, year));
        },
        GetPaiments: function(week, year) {
            return ajaxRequest('get', servicePaimentsUrls.paiments(week, year));
        },
        UpdatePaiment: function(orderid, pai) {
            return ajaxRequest('put', servicePaimentsUrls.updatePaiment(orderid), pai);
        },
        GetAccounts: function() {
             return ajaxRequest('get', serviseAccountsUrls.accounts());
        },
        DeleteAccount: function (accountId) {
            return ajaxRequest('delete', serviseAccountsUrls.deleteAccount(accountId));
        },
        GetWorkDays: function (week,year) {
            return ajaxRequest('get', serviseWorkDaysUrls.workDays(week, year));
        },
        UpdateWorkDays: function (workWeekId, weekinfo) {
            return ajaxRequest('put', serviseWorkDaysUrls.updateWorkDays(workWeekId),weekinfo);
        }
    };

})();
