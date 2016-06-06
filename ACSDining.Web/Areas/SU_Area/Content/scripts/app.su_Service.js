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
ko.bindingHandlers.checkedRadioToBool = {
    init: function(element, valueAccessor, allBindingsAccessor) {
        var observable = valueAccessor(),
            interceptor = ko.computed({
                read: function() {
                    return observable().toString();
                },
                write: function(newValue) {
                    observable(newValue === "true");
                },
                owner: this
            });
        ko.applyBindingsToNode(element, { checked: interceptor });
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

window.app.su_Service = (function() {

    var baseWeekMenuUri = "/api/WeekMenu/";
    var serviceWeekMenuUrls = {
        weekNumbers: function() { return baseWeekMenuUri + "WeekNumbers"; },
        currentweek: function() { return baseWeekMenuUri + "curWeekYear"; },
        categories: function() { return baseWeekMenuUri + "categories" },
        create: function() { return baseWeekMenuUri + "create" },
        nextWeekYear: function() { return baseWeekMenuUri + "nextWeekYear" },
        isnextweekmenuexists: function() { return baseWeekMenuUri + "isnextweekmenuexists" },
        deleteWeekMenu: function(menuid) { return baseWeekMenuUri + "delete/" + menuid },
        sendmenuUpdateMessage: function() { return baseWeekMenuUri + "menuupdatemessage" },
        setasorderable: function() { return baseWeekMenuUri + "setasorderable" },
        workweekapply: function() { return baseWeekMenuUri + "workweekapply" }
    }

    var baseOrdersUri = "/api/Orders/";
    var serviceOrdersUrls = {
        weekorders: function (factorplan) { return baseOrdersUri + factorplan },
       // planweekorders: function() { return baseOrdersUri + "plan"  },
        updateWeekOrder: function() { return baseOrdersUri + "update" },
        createOrder: function() { return baseOrdersUri + "create" },
        calcsummary: function() { return baseOrdersUri + "summary/" } 
    }

    var baseDishesUri = "/api/Dishes/";
    var serviceDishesUrls = {
        byCategory: function(category) { return baseDishesUri + "byCategory/" + category },
        update: function() { return baseDishesUri + "update" },
        updateDeleted: function () { return baseDishesUri + "updateDeleted" },
        create: function() { return baseDishesUri + "create" }
    }

    var basePaimentsUri = "/api/Paiment/";
    var servicePaimentsUrls = {
        weekPaiments: function() { return basePaimentsUri },
        updatePaiment: function () { return basePaimentsUri + "updatePaiment" },
        updateNote: function () { return basePaimentsUri + "updateNote" }
    }
    var baseAccountsUri = "/api/Account/";
    var serviceAccountsUrls = {
        accounts: function() { return baseAccountsUri + "All" },
        updateEmail: function () { return baseAccountsUri + "updateEmail" },
        updateMakeBook: function () { return baseAccountsUri + "updateMakeBook" },
        updateExists: function () { return baseAccountsUri + "updateExists" },
        deleteAccount: function(id) { return baseAccountsUri + "delete/" + id }
    }

    var baseUserWeekOrder = "/api/Employee/";
    var serviceUserWeekOrders = {
        currentweek: function() { return baseUserWeekOrder + "curWeekYear"; },
        nextWeekOrderExists: function() { return baseUserWeekOrder + "nextWeekOrderExists" },
        isNextWeekYear: function() { return baseUserWeekOrder + "isNextWeekYear" },
        isCurWeekYear: function() { return baseUserWeekOrder + "isCurWeekYear" },
        updateuserweek: function() { return baseUserWeekOrder + "update" },
        canCreateOrderOnNextWeek: function() { return baseUserWeekOrder + "canCreateOrderOnNextWeek" },
        nextWeekYear: function() { return baseUserWeekOrder + "nextWeekYear" },
        emailexists: function() { return baseUserWeekOrder + "emailexists" },
        setemail: function() { return baseUserWeekOrder + "setemai" }
    }

    var baseExcelUri = "/api/GetExcel/";
    var serviceGetExcel = {
        paiments: function() { return baseExcelUri + "paiments" },
        menu: function() { return baseExcelUri + "menu" },
        orders: function() { return baseExcelUri + "orders" }
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
        GetNextWeekYear: function() {
            return ajaxRequest("get", serviceWeekMenuUrls.nextWeekYear());
        },
        IsNextWeekMenuExists: function() {
            return ajaxRequest("get", serviceWeekMenuUrls.isnextweekmenuexists());
        },
        GetMenuExcel:function(medto) {
            return ajaxRequest("put", serviceGetExcel.menu(), medto);
        },
        GetCurrentWeekYear: function() {
            return ajaxRequest("get", serviceWeekMenuUrls.currentweek());
        },
        ApplyWorkWeek: function(wwDto) {
            return ajaxRequest("put", serviceWeekMenuUrls.workweekapply(), wwDto);
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
        CreateNextWeekMenu: function() {
            return ajaxRequest("post", serviceWeekMenuUrls.create());
        },
        SendMenuUpdateMessage: function(message) {
            return ajaxRequest("put", serviceWeekMenuUrls.sendmenuUpdateMessage(), message);
        },
        SetAsOrderable: function(message) {
            return ajaxRequest("put", serviceWeekMenuUrls.setasorderable(), message);
        },
        GetCategories: function() {
            return ajaxRequest("get", serviceWeekMenuUrls.categories());
        },
        //Dishes
        CreateDish: function(dish) {
            return ajaxRequest("post", serviceDishesUrls.create(), dish);
        },
        DishesByCategory: function(category) {
            return ajaxRequest("get", serviceDishesUrls.byCategory(category));
        },
        UpdateDish: function(dish) {
            return ajaxRequest("put", serviceDishesUrls.update(), dish);
        },
        UpDelDish: function(updel) {
            return ajaxRequest("put", serviceDishesUrls.updateDeleted(), updel);
        },
        //Orders
        LoadWeekOrders: function(wyDto,factorplan) {
            return ajaxRequest("put", serviceOrdersUrls.weekorders(factorplan), wyDto);
        },
        UpdateOrder: function(item) {
            return ajaxRequest("put", serviceOrdersUrls.updateWeekOrder(), item);
        },
        CreateOrdersNextweek: function() {
            return ajaxRequest("post", serviceOrdersUrls.createOrder());
        },
        GetExcelOrders: function(feDto) {
            return ajaxRequest("put", serviceGetExcel.orders(), feDto);
        },
        //Paiments
        GetPaiments: function(wyDto) {
            return ajaxRequest("put", servicePaimentsUrls.weekPaiments(), wyDto);
        },
        UpdatePaiment: function(uwp) {
            return ajaxRequest("put", servicePaimentsUrls.updatePaiment(), uwp);
        },
        UpdateNote: function (uwnote) {
            return ajaxRequest("put", servicePaimentsUrls.updateNote(), uwnote);
        },
        GetExcelPaiments: function(wyDto) {
            return ajaxRequest("put", serviceGetExcel.paiments(), wyDto);
        },
        //Accounts
        GetAccounts: function() {
            return ajaxRequest("get", serviceAccountsUrls.accounts());
        },
        UpdateAccountEmail: function(accountEmail) {
            return ajaxRequest("put", serviceAccountsUrls.updateEmail(), accountEmail);
        },
        UpdateAccountMakebook: function (accmakebook) {
            return ajaxRequest("put", serviceAccountsUrls.updateMakeBook(), accmakebook);
        },
        UpdateAccountExists: function (acexists) {
            return ajaxRequest("put", serviceAccountsUrls.updateExists(), acexists);
        },
        DeleteAccount: function(accountId) {
            return ajaxRequest("delete", serviceAccountsUrls.deleteAccount(accountId));
        },
        //Employee
        GetCurrentWeekYearForEmployee: function() {
            return ajaxRequest("get", serviceUserWeekOrders.currentweek());
        },
        LoadUserWeekOrder: function(wyDto) {
            return ajaxRequest("put", baseUserWeekOrder, wyDto);
        },
        GetUserNextWeekYear: function() {
            return ajaxRequest("get", serviceUserWeekOrders.nextWeekYear());
        },
        IsNextWeekYear: function(wyDto) {
            return ajaxRequest("put", serviceUserWeekOrders.isNextWeekYear(), wyDto);
        },
        IsCurWeekYear: function(wyDto) {
            return ajaxRequest("put", serviceUserWeekOrders.isCurWeekYear(), wyDto);
        },
        NextWeekOrderExists: function() {
            return ajaxRequest("get", serviceUserWeekOrders.nextWeekOrderExists());
        },
        UserWeekUpdateOrder: function(item) {
            return ajaxRequest("put", serviceUserWeekOrders.updateuserweek(), item);
        },
        CanCreateOrderOnNextWeek: function() {
            return ajaxRequest("get", serviceUserWeekOrders.canCreateOrderOnNextWeek());
        },
        IsEmailExists: function() {
            return ajaxRequest("get", serviceUserWeekOrders.emailexists());
        },
        SetEmail: function(email) {
            return ajaxRequest("put", serviceUserWeekOrders.setemail(), email);
        }
    };

})();
