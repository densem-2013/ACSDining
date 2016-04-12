﻿window.app = window.todoApp || {};

ko.observableArray.fn.pushAll = function (valuesToPush) {
    var underlyingArray = this();
    this.valueWillMutate();
    ko.utils.arrayPushAll(underlyingArray, valuesToPush);
    this.valueHasMutated();
    return this;
};

//ko.observable.fn._isEqual = function (valuesToEqual) {
//    var temp = this();
//    this.valueWillMutate();
//    var thisobject = ko.mapping.toJS(temp);
//    var secobj = ko.mapping.toJS(valuesToEqual);
//    this.valueHasMutated();
//    return thisobject.EQUAL(secobj);
//};

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
                dateFormat: "dd/mm/yy",
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

 var WeekYear=function(wyObj) {
      var self = this;
      self.Week = ko.observable(wyObj.week);
      self.Year = ko.observable(wyObj.year);
    }

window.app.su_Service = (function() {

    var baseWeekMenuUri = "/api/WeekMenu/";
    var serviceWeekMenuUrls = {
        //weekMenu: function(numweek, year) {
        //    numweek = numweek == undefined ? "" : numweek;
        //    year = year == undefined ? "" : "/" + year;
        //    return baseWeekMenuUri + numweek + year;
        //},
        weekNumbers: function() { return baseWeekMenuUri + "WeekNumbers"; },
        currentweek: function() { return baseWeekMenuUri + "curWeekYear"; },
        categories: function() { return baseWeekMenuUri + "categories" },
        nextWeekYear: function () { return baseWeekMenuUri + "nextWeekYear" },
        prevWeekYear: function() { return baseWeekMenuUri + "prevWeekYear" },
        deleteWeekMenu: function (menuid) { return baseWeekMenuUri + "delete/" + menuid }
    }

    var baseOrdersUri = "/api/Orders/";
    var serviceOrdersUrls = {
        //ordersParams: function(numweek, year) {
        //    numweek = numweek == undefined ? "" : numweek;
        //    year = year == undefined ? "" : "/" + year;
        //    return numweek + year;
        //},
        updateOrder: function () { return baseOrdersUri + "update" /* + serviceOrdersUrls.ordersParams(week, year) */ },
        createOrder: function () { return baseOrdersUri + "create" /* + serviceOrdersUrls.ordersParams(week, year) */ },
        calcsummary: function () { return baseOrdersUri + "summary/" /* + serviceOrdersUrls.ordersParams(week, year) */ }
    }
    var baseDishesUri = "/api/Dishes/";
    var serviceDishesUrls = {
        byCategory: function(category) { return baseDishesUri + "byCategory/" + category },
        update: function() { return baseDishesUri + "update" },
        deleteDish: function(dishID) { return baseDishesUri + "delete/" + dishID },
        create: function() { return baseDishesUri + "create" }
    }

    var basePaimentsUri = "/api/Paiment/";
    var servicePaimentsUrls = {
        //paiments: function (wyDto) { return basePaimentsUri /* + serviceOrdersUrls.ordersParams(week, year) */ },
        updatePaiment: function(orderid) { return basePaimentsUri + "updatePaiment/" + orderid },
        totalPaimentsbyDish: function (wyDto) { return basePaimentsUri + "paimentsByDish/" /*+ serviceOrdersUrls.ordersParams(week, year) */ }
    }
    var baseAccountsUri = "/api/Account/";
    var serviceAccountsUrls = {
        accounts: function() { return baseAccountsUri + "All" },
        deleteAccount: function(id) { return baseAccountsUri + "delete/" + id }
    }
    var baseWorkDaysUri = "/api/WorkDays/";
    var serviceWorkDaysUrls = {
        //workDays: function (wyDto) { return baseWorkDaysUri /*+ serviceOrdersUrls.ordersParams(week, year)*/ },
        updateWorkDays: function () { return baseWorkDaysUri + "update"  }
    }

    var baseUserWeekOrder = "/api/Employee/";
    var serviceUserWeekOrders= {
        //weekorder: function (wyDto) { return baseUserWeekOrder /*+ serviceOrdersUrls.ordersParams(week, year) */ },
        isNextWeekYear: function () { return baseUserWeekOrder + "isNextWeekYear" },
        canCreateOrderOnNextWeek: function () { return baseUserWeekOrder + "canCreateOrderOnNextWeek" }
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
        LoadWeekMenu: function(wyDto) {
            return ajaxRequest("put", baseWeekMenuUri, wyDto);
        },
        GetNextWeekYear: function (wyDto) {
            return ajaxRequest("put", serviceWeekMenuUrls.nextWeekYear(), wyDto);
        },
        GetPrevWeekYear: function (wyDto) {
            return ajaxRequest("put", serviceWeekMenuUrls.prevWeekYear(), wyDto);
        },
        GetCurrentWeekYear: function() {
            return ajaxRequest("get", serviceWeekMenuUrls.currentweek());
        },
        LoadWeekNumbers: function() {
            return ajaxRequest("get", serviceWeekMenuUrls.weekNumbers());
        },
        UpdateWeekMenu: function(item) {
            return ajaxRequest("put", baseWeekMenuUri + "update", item);
        },
        DeleteNextWeekMenu: function(menuid) {
            return ajaxRequest("delete", serviceWeekMenuUrls.deleteWeekMenu(menuid));
        },
        GetCategories: function() {
            return ajaxRequest("get", serviceWeekMenuUrls.categories());
        },
        CreateDish: function(dish) {
            return ajaxRequest("post", serviceDishesUrls.create(), dish);
        },
        DishesByCategory: function(category) {
            return ajaxRequest("get", serviceDishesUrls.byCategory(category));
        },
        UpdateDish: function(dish) {
            return ajaxRequest("put", serviceDishesUrls.update(), dish);
        },
        DeleteDish: function(dishId) {
            return ajaxRequest("delete", serviceDishesUrls.deleteDish(dishId));
        },
        LoadWeekOrders: function (wyDto) {
            return ajaxRequest("put", baseOrdersUri, wyDto);
        },
        //GetOrderSummary: function(week, year, item) {
        //    return ajaxRequest("put", serviceOrdersUrls.calcsummary(week, year), item);
        //},
        UpdateOrder: function( item) {
            return ajaxRequest("put", serviceOrdersUrls.updateOrder(), item);
        },
        CreateOrdersNextweek: function () {
            return ajaxRequest("post", serviceOrdersUrls.createOrder());
        },
        GetPaiments: function (wyDto) {
            return ajaxRequest("put", basePaimentsUri, wyDto);
        },
        UpdatePaiment: function(orderid, pai) {
            return ajaxRequest("put", servicePaimentsUrls.updatePaiment(orderid), pai);
        },
        GetAccounts: function() {
             return ajaxRequest("get", serviceAccountsUrls.accounts());
        },
        DeleteAccount: function (accountId) {
            return ajaxRequest("delete", serviceAccountsUrls.deleteAccount(accountId));
        },
        GetWorkDays: function (wyDto) {
            return ajaxRequest("get", baseWorkDaysUri, wyDto);
        },
        UpdateWorkDays: function (weekinfo) {
            return ajaxRequest("put", serviceWorkDaysUrls.updateWorkDays(),weekinfo);
        },
        LoadUserWeekOrder: function(wyDto) {
            return ajaxRequest("put", /*serviceUserWeekOrders.weekorder(week,year)*/baseUserWeekOrder, wyDto);
        },
        IsNextWeekYear: function (wyDto) {
            return ajaxRequest("put", serviceUserWeekOrders.isNextWeekYear(), wyDto);
        },
        CanCreateOrderOnNextWeek: function () {
            return ajaxRequest("get", serviceUserWeekOrders.canCreateOrderOnNextWeek());
        }
    };

})();
