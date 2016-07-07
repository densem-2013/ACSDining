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
                newValueAsNum = (isNaN(newValue) || newValue == undefined) ? 0 : parseFloat(+newValue),
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
	return Math.ceil((((today - onejan) / millisecsInDay) + onejan.getDay()) / 7);
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
	init: function (element, valueAccessor, allBindingsAccessor) {
		var observable = valueAccessor(),
            interceptor = ko.computed({
            	read: function () {
            		return observable().toString();
            	},
            	write: function (newValue) {
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

var WeekYear = function (wyObj) {
	var self = this;
	self.Week = ko.observable(wyObj.week);
	self.Year = ko.observable(wyObj.year);
};

window.app.EmployeeService = (function() {

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
        setemail: function() { return baseUserWeekOrder + "setemai" },
        prevweekorder: function () { return baseUserWeekOrder + "getprevweekorder" },
        setasprev: function () { return baseUserWeekOrder + "setasprev" },
        allbyone: function () { return baseUserWeekOrder + "allbyone" },
        updateAll: function () { return baseUserWeekOrder + "updateAll" }
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
        },
    	GetPrevWeekOrderQuantity: function(weekord) {
    		return ajaxRequest("put", serviceUserWeekOrders.prevweekorder(), weekord);
    	},
    	SetOrderAsPrevWeek: function (weekord) {
    	    return ajaxRequest("put", serviceUserWeekOrders.setasprev(), weekord);
    	},
    	SetAllByOne: function (weekord) {
    	    return ajaxRequest("put", serviceUserWeekOrders.allbyone(), weekord);
    	},
    	UpdateAll: function (weekord) {
    	    return ajaxRequest("put", serviceUserWeekOrders.updateAll(), weekord);
    	}
    };
})();