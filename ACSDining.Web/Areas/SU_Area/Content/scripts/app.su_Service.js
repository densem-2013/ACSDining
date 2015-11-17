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
        create: function() { return baseWeekMenuUri + 'createNextWeekMenu' }
    }

    var baseOrdersUri = '/api/Orders/';
    var serviceOrdersUrls = {
        ordersParams: function(numweek, year) {
            numweek = numweek == undefined ? '' : numweek;
            year = year == undefined ? '' : "/" + year;
            return numweek + year;
        },
        updateNextWeekOrders: function (week, year) { return baseOrdersUri + 'update' + serviceOrdersUrls.ordersParams(week, year) },
        createNextWeekOrders: function (week, year) { return baseOrdersUri + 'create' + serviceOrdersUrls.ordersParams(week, year) },
        calcsummary: function (week, year) { return baseOrdersUri + 'summary/' + serviceOrdersUrls.ordersParams(week, year) }
    }
    var baseDishesUri = '/api/Dishes/';
    var serviceDishesUrls = {
        byCategory: function (category) { return baseDishesUri + 'byCategory/' + category },
        update: function () { return baseDishesUri + 'update' },
        deleteDish: function (dishID) { return baseDishesUri + 'delete/' + dishID },
        create: function () { return baseDishesUri + 'create'}
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
        GetCurrentWeekNumber: function() {
            return ajaxRequest('get', serviceWeekMenuUrls.currentweek());
        },
        LoadWeekNumbers: function() {
            return ajaxRequest('get', serviceWeekMenuUrls.weekNumbers());
        },
        UpdateWeekMenu: function(item) {
            return ajaxRequest('put', baseWeekMenuUri + 'update', item);
        },
        CreateNextWeekMenu: function(item) {
            return ajaxRequest('post', serviceWeekMenuUrls.create(), item);
        },
        GetCategories: function() {
            return ajaxRequest('get', serviceWeekMenuUrls.categories());
        },
        CreateDish: function (dish) {
            return ajaxRequest('post', serviceDishesUrls.create(),dish);
        },
        DishesByCategory: function(category) {
            return ajaxRequest('get', serviceDishesUrls.byCategory(category));
        },
        UpdateDish:function(dish) {
            return ajaxRequest('put', serviceDishesUrls.update(), dish);
        },
        DeleteDish: function (dishID) {
            return ajaxRequest('delete', serviceDishesUrls.deleteDish(dishID));
        },
        LoadWeekOrders: function(numweek, year) {
            return ajaxRequest('get', baseOrdersUri + serviceOrdersUrls.ordersParams(numweek, year));
        },
        GetOrderSummary: function(week,year, item) {
            return ajaxRequest('put', serviceOrdersUrls.calcsummary(week, year), item);
        },
        UpdateOrder: function (week, year, item) {
            return ajaxRequest('put', serviceOrdersUrls.updateNextWeekOrders(week, year), item);
        },
        CreateOrdersNextweek: function (week,year) {
            return ajaxRequest('post', serviceOrdersUrls.createNextWeekOrders(week, year));
        }
    };

})();
