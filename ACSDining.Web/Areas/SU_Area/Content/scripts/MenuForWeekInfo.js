/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />


(function () {

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

    var objForMap = function(){
        this.categories= ["Первое блюдо", "Второе блюдо", "Салат", "Напиток"];
        this.target = [];
        this.sortFunc = function (value) {
            for (var i = 0; i < 4; i++) {

                if (value.category == this.categories[i]) {

                    this.target.push(new DishInfo(value)); 
                }
            }
        }
    }



    var viewModel =  {


        MenuId : ko.observable(),
        WeekNumber : ko.observable(),

        MFD_models : ko.observableArray([]),


       Message : ko.observable(""),


        DishesByCategory : ko.observableArray([]),
        Category : ko.observable(),

        SelectedDish : ko.observable(),

        UpdatableMFD: ko.observable(),

        SummaryPrice: ko.observable(),
    
        NumbersWeeks: ko.observableArray(),

        BeenChanged: ko.observable(false),

        ChangeSaved: ko.observable(false)
    };

    viewModel.SaveToServer = function() {
        var source= {
            MenuId: this.MenuId(),
            WeekNumber: this.WeekNumber(),
            MFD_models: this.MFD_models(),
            SummaryPrice: this.SummaryPryce()

        }

        $.ajax({
            url: '/api/WeekMenu/' + this.WeekNumber(),
            type: 'post',
            data: ko.toJSON(source),
            contentType: 'application/json'
        }).done( function (data) {

            this.BeenChanged(false);
            this.ChangeSaved(true);

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


        viewModel.LoadWeekMenu = function (numweek) {


            $.ajax({
                url: "/api/WeekMenu/" + numweek,
                type: "GET"
            }).done(function (resp) {

                viewModel.MFD_models([]);

                viewModel.MenuId(resp.id);
                viewModel.WeekNumber(resp.weekNumber);
                ko.utils.arrayForEach(resp.mfD_models, function (object) {
                    var obj = new objForMap();
                    object.dishes.map(obj.sortFunc, obj);
                    object.dishes = obj.target;

                    var MenuForDayInfo =  {

                        ID: ko.observable(object.id),
                        DayOfWeek: ko.observable(object.dayOfWeek),
                        Dishes : ko.observableArray(obj.target),
                        Editing: ko.observable(false),
                        TotalPrice: ko.observable(),

                        Editable : function () {
                            this.Editing(true);
                        },

                        UnEditable : function () {
                            this.Editing(false);
                        }


                    }

                    MenuForDayInfo.Dishes.subscribe = ko.computed(function () {
                        var sum = 0;
                        var valsum;
                        //var ind = -1;
                        for (var i = 0; i < MenuForDayInfo.Dishes().length; i++) {
                            var dishes = MenuForDayInfo.Dishes();
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

        viewModel.applyChanges = function () {

            var obj = new objForMap();
            var catIndex = $.map(obj.categories, function (n, i) {
                if (viewModel.Category() == n)
                    return i;
            });


            var dish = ko.utils.arrayFirst(viewModel.DishesByCategory(), function(value) {
                if (value.DishId() == viewModel.SelectedDish()) {
                    return value;
                }
            });
            if (dish != undefined ) {

                viewModel.MFD_models()[viewModel.UpdatableMFD()].Dishes()[catIndex].update(dish);
                this.CalcTotal();
            };


            //console.log(viewModel.MFD_models()[viewModel.UpdatableMFD()].Dishes()[catIndex].Title());

            //console.log(viewModel.UpdatableMFD());

            $("#modalbox").modal("hide");
        }


    viewModel.CalcTotal = function() {

        var sum = 0;
        var ind = -1;
        for (var ind = 0; ind < viewModel.MFD_models().length; ind++) {
           // var mdfs = viewModel.MFD_models();
            sum += parseFloat(viewModel.MFD_models()[ind].TotalPrice());
           // console.log("ind=" + ind + " index=" + ind + " " + viewModel.MFD_models()[ind].DayOfWeek() + " - " + sum.toFixed(2));

        }

        this.SummaryPrice(sum.toFixed(2));
    };

    viewModel.MFD_models.subscribe = ko.computed(viewModel.CalcTotal, viewModel);

    viewModel.LoadWeekMenu();
    viewModel.loadWeekNumbers();

    ko.applyBindings(viewModel);

})();
