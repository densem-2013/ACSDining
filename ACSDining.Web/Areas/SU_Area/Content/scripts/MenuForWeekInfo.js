/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />


(function () {
    $("#infoTitle span").attr({ 'data-bind': 'text: WeekTitle' });

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
                    hideIfNoPrevNext: true
                }
            );
            $(element).datepicker(options);

            ko.utils.registerEventHandler($(element), "change", function() {
                var observable = valueAccessor();
                observable($(element).datepicker("getDate"));
            });

            ko.utils.domNodeDisposal.addDisposeCallback(element, function() {
                $(element).datepicker("destroy");
            });

        },
        update: function(element, valueAccessor) {
            var value = ko.utils.unwrapObservable(valueAccessor());
            $(element).datepicker("setDate", value);
        }
    };


    var DishInfo = function (dinfo) {

        self = this;

        self.DishId = ko.observable(dinfo.dishID);
        self.Title = ko.observable(dinfo.title);
        self.ProductImage = ko.observable(dinfo.productImage);
        self.Price = ko.observable(dinfo.price.toFixed(2));
        self.Category = ko.observable(dinfo.category);

        self.update = function (dishupdate) {
            this.DishId(dishupdate.DishId());
            this.Title(dishupdate.Title());
            this.ProductImage(dishupdate.ProductImage());
            this.Price(dishupdate.Price());

        };
    }

    ko.observableArray.fn.pushAll = function (valuesToPush) {
        var underlyingArray = this();
        this.valueWillMutate();
        ko.utils.arrayPushAll(underlyingArray, valuesToPush);
        this.valueHasMutated();
        return this;
    };

    var objForMap = function () {
        this.categories = ["Первое блюдо", "Второе блюдо", "Салат", "Напиток"];
        this.target = [];
        this.sortFunc = function (value) {
            for (var i = 0; i < 4; i++) {

                if (value.category == this.categories[i]) {

                    this.target.push(new DishInfo(value));
                }
            }
        }
    }


    var viewModel = {
        MenuId: ko.observable(),
        CurrentWeekNumber: ko.observable(),
        WeekNumber: ko.observable(),

        MFD_models: ko.observableArray([]),


        Message: ko.observable(""),

        myDate: ko.observable(new Date()),

        DishesByCategory: ko.observableArray([]),
        Category: ko.observable(),

        SelectedDish: ko.observable(),

        UpdatableMFD: ko.observable(),

        SummaryPrice: ko.observable(),

        NumbersWeeks: ko.observableArray(),

        BeenChanged: ko.observable(false),

        ChangeSaved: ko.observable(false),
        Year: ko.observable()
    };
    viewModel.WeekTitle = ko.computed(function () {
        var options = {
            weekday: "short", year: "numeric", month: "short",
            day: "numeric"
        };
        var year = viewModel.Year();
        var firstDay = new Date(year, 0, 1).getDay();
        //console.log(firstDay);
        //var year = 2015;
        var week = viewModel.WeekNumber();
        var d = new Date("Jan 01, " + year + " 01:00:00");
        var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week - 1);
        var n1 = new Date(w);
        var n2 = new Date(w + 345600000);
        return "Неделя " + week + ", " + n1.toLocaleDateString("ru-RU", options) + " - " + n2.toLocaleDateString("ru-RU", options);
    }.bind(viewModel));

    viewModel.pageSize = ko.observable(7);

    viewModel.pageIndex = ko.observable(0);

    viewModel.pagedList = ko.dependentObservable(function () {
        var size = viewModel.pageSize();
        var start = viewModel.pageIndex() * size;
        return viewModel.DishesByCategory.slice(start, start + size);
    });

    viewModel.maxPageIndex = ko.dependentObservable(function () {
        return Math.ceil(viewModel.DishesByCategory().length / viewModel.pageSize()) - 1;
    });

    viewModel.previousPage = function () {
        if (viewModel.pageIndex() > 0) {
            viewModel.pageIndex(viewModel.pageIndex() - 1);
        }
    };

    viewModel.nextPage = function () {
        if (viewModel.pageIndex() < viewModel.maxPageIndex()) {
            viewModel.pageIndex(viewModel.pageIndex() + 1);
        }
    };

    viewModel.allPages = ko.dependentObservable(function () {
        var pages = [];
        for (i = 0; i <= viewModel.maxPageIndex() ; i++) {
            pages.push({ pageNumber: (i + 1) });
        }
        return pages;
    });

    viewModel.moveToPage = function (index) {
        viewModel.pageIndex(index);
    };

    viewModel.SaveToServer = function () {
        var source = {
            ID: viewModel.MenuId(),
            WeekNumber: viewModel.WeekNumber(),
            MFD_models: this.MFD_models(),
            SummaryPrice: this.SummaryPrice()

        }

        var objToServer = ko.toJSON(source);
        $.ajax({
            url: '/api/WeekMenu/' + this.WeekNumber(),
            type: 'put',
            data: objToServer,
            contentType: 'application/json'
        }).done(function (data) {

            viewModel.BeenChanged(false);
            viewModel.ChangeSaved(true);
            ko.utils.arrayForEach(viewModel.MFD_models(), function (item) {
                item.UnEditable();
            });
        }).error(function (err) {
            viewModel.Message("Error! " + err.status);
        });
    };

    viewModel.loadWeekNumbers = function () {
        $.ajax({
            url: "/api/WeekMenu/WeekNumbers",
            type: "GET"
        }).done(function (resp) {

            for (var i = 0; i < resp.length; i++) {

                viewModel.NumbersWeeks.push(resp[i]);

            };

        }).error(function (err) {
            viewModel.Message("Error! " + err.status);
        });
    };

    viewModel.loadDishes = function (id) {
        $.ajax({
            url: "/api/Dishes/byCategory/" + id,
            type: "GET"
        }).done(function (resp) {
            viewModel.DishesByCategory([]);

            for (var i = 0; i < resp.length; i++) {

                viewModel.DishesByCategory.push(new DishInfo(resp[i]));

                if (resp[i].dishID == id) {
                    viewModel.SelectedDish(resp[i].dishID);
                };
            };

        }).error(function (err) {
            viewModel.Message("Error! " + err.status);
        });
    }


    viewModel.showDishes = function (searchdish, index) {
        this.UpdatableMFD(index);
        this.Category(searchdish.Category());
        this.loadDishes(searchdish.DishId());


        $("#modalbox").modal("show");
    }

    viewModel.changeSelected = function (clikedItem) {
        if (viewModel.SelectedDish() == clikedItem.DishId()) {
            viewModel.BeenChanged(true);
            viewModel.SelectedDish(clikedItem.DishId());
        }
        return true;
    }


    viewModel.LoadWeekMenu = function (numweek, year) {

        numweek = numweek == undefined ? '' : numweek;
        year = year == undefined ? '' : "/" + year;


        $.ajax({
            url: "/api/WeekMenu/" + numweek  + year,
            type: "GET"
        }).done(function (resp) {

            viewModel.MFD_models([]);

            viewModel.MenuId(resp.id);
            viewModel.WeekNumber(resp.weekNumber);
            viewModel.Year(resp.yearNumber);
            ko.utils.arrayForEach(resp.mfD_models, function (object) {
                var obj = new objForMap();
                object.dishes.map(obj.sortFunc, obj);
                object.dishes = obj.target;

                var MenuForDayInfo = {

                    ID: ko.observable(object.id),
                    DayOfWeek: ko.observable(object.dayOfWeek),
                    Dishes: ko.observableArray(obj.target),
                    Editing: ko.observable(false),
                    TotalPrice: ko.observable(),

                    Editable: function () {
                        this.Editing(true);
                    },

                    UnEditable: function () {
                        this.Editing(false);
                    }


                }

                MenuForDayInfo.Dishes.subscribe = ko.computed(function () {
                    var sum = 0;
                    var valsum;
                    for (var i = 0; i < MenuForDayInfo.Dishes().length; i++) {

                        valsum = parseFloat(MenuForDayInfo.Dishes()[i].Price());
                        sum += valsum;
                    };


                    MenuForDayInfo.TotalPrice(sum.toFixed(2));

                }.bind(this));

                viewModel.MFD_models.push(MenuForDayInfo);

            });

        }).error(function (err) {
            viewModel.Message("Error! " + err.status);
        });
    }

    viewModel.myDate.subscribe = ko.computed(function () {
        var takedWeek = viewModel.myDate().getWeek()+1;
        if (takedWeek !== viewModel.WeekNumber()) {
            viewModel.LoadWeekMenu(takedWeek, viewModel.Year());
        }
    }, viewModel);

    viewModel.GetCurrentWeekNumber = function () {
        
        $.ajax({
            url: "/api/WeekMenu/CurrentWeek",
            type: "GET"
        }).done(function (resp) {
            viewModel.CurrentWeekNumber(resp);
        });
    }
    
    viewModel.IsCurrentWeek = ko.computed(function () {
        return viewModel.CurrentWeekNumber() == viewModel.WeekNumber();
    }.bind(viewModel));

    viewModel.applyChanges = function () {

        var obj = new objForMap();
        var catIndex = $.map(obj.categories, function (n, i) {
            if (viewModel.Category() == n)
                return i;
        });


        var dish = ko.utils.arrayFirst(viewModel.DishesByCategory(), function (value) {
            if (value.DishId() === viewModel.SelectedDish()) {
                return value;
            }
        });
        if (dish != undefined) {

            viewModel.MFD_models()[viewModel.UpdatableMFD()].Dishes()[catIndex].update(dish);
            this.CalcTotal();
        };



        $("#modalbox").modal("hide");
    }


    viewModel.CalcTotal = function () {

        var sum = 0;
        var ind = -1;
        for (var ind = 0; ind < viewModel.MFD_models().length; ind++) {
            sum += parseFloat(viewModel.MFD_models()[ind].TotalPrice());

        }

        this.SummaryPrice(sum.toFixed(2));
    }.bind(viewModel);

    viewModel.MFD_models.subscribe = ko.computed(viewModel.CalcTotal, viewModel);

    viewModel.LoadWeekMenu();
    viewModel.loadWeekNumbers();
    viewModel.GetCurrentWeekNumber();

    ko.applyBindings(viewModel);

})();
