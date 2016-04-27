window.app = window.todoApp || {};

ko.observableArray.fn.pushAll = function (valuesToPush) {
    var underlyingArray = this();
    this.valueWillMutate();
    ko.utils.arrayPushAll(underlyingArray, valuesToPush);
    this.valueHasMutated();
    return this;
};

ko.bindingHandlers.hover = {
    init: function (element, valueAccessor) {
        var value = valueAccessor();
        ko.applyBindingsToNode(element, {
            event: {
                mouseenter: function () { value(true) },
                mouseleave: function () { value(false) }
            }
        });
    }
}

ko.extenders.numeric = function (target, precision) {
    //create a writable computed observable to intercept writes to our observable
    var result = ko.pureComputed({
        read: target,  //always return the original observables value
        write: function (newValue) {
            var current = target(),
                roundingMultiplier = Math.pow(10, precision),
                newValueAsNum = (isNaN(newValue)||newValue==undefined) ? 0 : parseFloat(+newValue),
                valueToWrite = Math.round(newValueAsNum * roundingMultiplier) / roundingMultiplier;

            //only write if it changed
            if (valueToWrite !== current) {
                target(valueToWrite);
            } else {
                //if the rounded value is the same, but a different value was written, force a notification for the current field
                if (newValue !== current) {
                    target.notifySubscribers(valueToWrite);
                }
            }
        }
    }).extend({ notify: "always" });

    //initialize with current value to make sure it is rounded appropriately
    result(target());

    //return the new computed observable
    return result;
};
Date.prototype.getWeek = function () {
    //var onejan = new Date(this.getFullYear(), 0, 1);
    var today = new Date(this.getFullYear(), this.getMonth(), this.getDate());
    //var dayOfYear = ((today - onejan +1) / 86400000);
    //return Math.ceil((dayOfYear) / 7);

    var onejan = new Date(today.getFullYear(), 0, 1);
    var millisecsInDay = 86400000;
    return Math.ceil((((today - onejan ) / millisecsInDay) + onejan.getDay() ) / 7);
};

ko.bindingHandlers.datepicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {

        var options = $.extend(
            {},
            $.datepicker.regional["ru"],
            {
                beforeShowDay: $.datepicker.noWeekends,
                dateFormat: "dd/mm/yy",
               // showButtonPanel: true,
                gotoCurrent: true,
                showOtherMonths: true,
                selectOtherMonths: true, 
                firstDay: 1,
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

var WeekYear = function(wyObj) {
    var self = this;
    self.Week = ko.observable(wyObj.week);
    self.Year = ko.observable(wyObj.year);
};

 //var weekTitleCalculate = function (weekyear) {
 //    var options = {
 //        weekday: "short",
 //        year: "numeric",
 //        month: "short",
 //        day: "numeric"
 //    };
 //    var weekYearObj = weekyear;
 //    var year = weekYearObj.year;
 //    var firstDay = new Date(year, 0, 1).getDay();

 //    var week = weekYearObj.week;
 //    var d = new Date("Jan 01, " + year + " 01:00:00");
 //    var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week);
 //    var n1 = new Date(w);
 //    var n2 = new Date(w + 345600000);
 //    return "Неделя " + week + ", " + n1.toLocaleDateString("ru-RU", options) + " - " + n2.toLocaleDateString("ru-RU", options);
 //};

window.app.su_Service = (function() {

    var baseWeekMenuUri = "/api/WeekMenu/";
    var serviceWeekMenuUrls = {
        weekNumbers: function() { return baseWeekMenuUri + "WeekNumbers"; },
        currentweek: function() { return baseWeekMenuUri + "curWeekYear"; },
        categories: function() { return baseWeekMenuUri + "categories" },
        create: function() { return baseWeekMenuUri + "create" },
        nextWeekYear: function() { return baseWeekMenuUri + "nextWeekYear" },
        isnextweekmenuexists: function() { return baseWeekMenuUri + "isnextweekmenuexists" },
        //prevWeekYear: function() { return baseWeekMenuUri + "prevWeekYear" },
        deleteWeekMenu: function(menuid) { return baseWeekMenuUri + "delete/" + menuid },
        sendmenuUpdateMessage: function() { return baseWeekMenuUri + "menuupdatemessage" },
        setasorderable: function () { return baseWeekMenuUri + "setasorderable" },
        workweekapply: function (){ return baseWeekMenuUri + "workweekapply" }
    }

    var baseOrdersUri = "/api/Orders/";
    var serviceOrdersUrls = {
        factweekorders: function () { return baseOrdersUri + "fact" },
        planweekorders: function () { return baseOrdersUri + "plan" },
        updateWeekOrders: function () { return baseOrdersUri + "update" },
        createOrder: function () { return baseOrdersUri + "create"  },
        calcsummary: function () { return baseOrdersUri + "summary/"  }
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
        updatePaiment: function(orderid) { return basePaimentsUri + "updatePaiment/" + orderid },
        totalPaimentsbyDish: function (wyDto) { return basePaimentsUri + "paimentsByDish/"  }
    }
    var baseAccountsUri = "/api/Account/";
    var serviceAccountsUrls = {
        accounts: function() { return baseAccountsUri + "All" },
        deleteAccount: function(id) { return baseAccountsUri + "delete/" + id }
    }
    //var baseWorkDaysUri = "/api/WorkDays/";
    //var serviceWorkDaysUrls = {
    //    updateWorkDays: function () { return baseWorkDaysUri + "update"  }
    //}

    var baseUserWeekOrder = "/api/Employee/";
    var serviceUserWeekOrders= {
        nextWeekOrderExists: function () { return baseUserWeekOrder + "nextWeekOrderExists" },
        isNextWeekYear: function () { return baseUserWeekOrder + "isNextWeekYear" },
        updateuserweek: function () { return baseUserWeekOrder + "update" },
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
        GetNextWeekYear: function(wyDto) {
            return ajaxRequest("put", serviceWeekMenuUrls.nextWeekYear(), wyDto);
        },
        //GetPrevWeekYear: function(wyDto) {
        //    return ajaxRequest("put", serviceWeekMenuUrls.prevWeekYear(), wyDto);
        //},
        IsNextWeekMenuExists: function() {
            return ajaxRequest("get", serviceWeekMenuUrls.isnextweekmenuexists());
        },
        GetCurrentWeekYear: function() {
            return ajaxRequest("get", serviceWeekMenuUrls.currentweek());
        },
        ApplyWorkWeek: function(wwDto) {
            return ajaxRequest("put", workweekapply, wwDto);
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
        CreateNextWeekMenu: function () {
            return ajaxRequest("post", serviceWeekMenuUrls.create());
        },
        SendMenuUpdateMessage: function(message) {
            return ajaxRequest("put", serviceWeekMenuUrls.sendmenuUpdateMessage(), message);
        },
        SetAsOrderable: function (message) {
            return ajaxRequest("put", serviceWeekMenuUrls.setasorderable(), message);
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
        FactLoadWeekOrders: function (wyDto) {
            return ajaxRequest("put", serviceOrdersUrls.factweekorders(), wyDto);
        },
        PlanLoadWeekOrders: function (wyDto) {
            return ajaxRequest("put", serviceOrdersUrls.planweekorders(), wyDto);
        },
        UpdateOrders: function( item) {
            return ajaxRequest("put", serviceOrdersUrls.updateWeekOrders(), item);
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
        //GetWorkDays: function (wyDto) {
        //    return ajaxRequest("get", baseWorkDaysUri, wyDto);
        //},
        //UpdateWorkDays: function (weekinfo) {
        //    return ajaxRequest("put", serviceWorkDaysUrls.updateWorkDays(),weekinfo);
        //},
        LoadUserWeekOrder: function(wyDto) {
            return ajaxRequest("put", baseUserWeekOrder, wyDto);
        },
        IsNextWeekYear: function (wyDto) {
            return ajaxRequest("put", serviceUserWeekOrders.isNextWeekYear(), wyDto);
        },
        NextWeekOrderExists: function () {
            return ajaxRequest("get", serviceUserWeekOrders.nextWeekOrderExists());
        },
        UserWeekUpdateOrder: function (item) {
            return ajaxRequest("put", serviceUserWeekOrders.updateuserweek(), item);
        },
        CanCreateOrderOnNextWeek: function () {
            return ajaxRequest("get", serviceUserWeekOrders.canCreateOrderOnNextWeek());
        }
    };

})();
