/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />


(function () {

    ko.observableArray.fn.pushAll = function (valuesToPush) {
        var underlyingArray = this();
        this.valueWillMutate();
        ko.utils.arrayPushAll(underlyingArray, valuesToPush);
        this.valueHasMutated();
        return this;  //optional
    };

    //var DishInfo = function (object) {

    //    self = this;

    //    self.DishId = ko.observable(object.dishID);
    //    self.Title = ko.observable(object.title);
    //    self.ProductImage = ko.observable(object.productImage);
    //    self.Price = ko.observable(object.price.toFixed(2));

    //    self.Category = ko.observable(object.category);

    //}

    //DishInfo.prototype.update = function (dishupdate) {
    //    this.DishId(dishupdate.DishId());
    //    this.Title(dishupdate.Title());
    //    this.ProductImage(dishupdate.ProductImage());
    //    this.Price(dishupdate.Price());

    //}

    var objForMap = function(){
        this.categories= ["Первое блюдо", "Второе блюдо", "Салат", "Напиток"];
        this.target = [4];
        this.sortFunc = function (value) {
            for (var i = 0; i < 4; i++) {

                if (value.category == this.categories[i]) {

                    var dinfo = {
                        DishId: ko.observable(value.dishID),
                        Title: ko.observable(value.title),
                        ProductImage: ko.observable(value.productImage),
                        Price: ko.observable(value.price.toFixed(2)),
                        Category: ko.observable(value.category)
                    };
                    dinfo.update = function (dishupdate) {
                        this.DishId(dishupdate.DishId());
                        this.Title(dishupdate.Title());
                        this.ProductImage(dishupdate.ProductImage());
                        this.Price(dishupdate.Price());

                    };
                    this.target[i] = dinfo;
                }
            }
        }
    }



    var viewModel =  {
        //var self = this;


        MenuId : ko.observable(),
        WeekNumber : ko.observable(),

        MFD_models : ko.observableArray([]),

        Total : ko.observable(),


       Message : ko.observable(""),


        DishesByCategory : ko.observableArray([]),
        Category : ko.observable(),

        SelectedDish : ko.observable(),

        UpdatableMFD: ko.observable(),

        SummaryPrice: ko.observable()
    

};
    viewModel.loadDishes = function (id) {
            $.ajax({
                url: "/api/byCategory/" + id,
                type: "GET"
            }).done(function (resp) {
                viewModel.DishesByCategory([]);

                for (var i = 0; i < resp.length; i++) {

                    var dinfo = {
                        DishId: ko.observable(resp[i].dishID),
                        Title: ko.observable(resp[i].title),
                        ProductImage: ko.observable(resp[i].productImage),
                        Price: ko.observable(resp[i].price.toFixed(2)),
                        Category: ko.observable(resp[i].category)
                    }

                    dinfo.update = function (dishupdate) {
                        this.DishId(dishupdate.DishId());
                        this.Title(dishupdate.Title());
                        this.ProductImage(dishupdate.ProductImage());
                        this.Price(dishupdate.Price());

                    };
                    viewModel.DishesByCategory.push(dinfo);

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
        if (viewModel.SelectedDish() !== clikedItem.DishId()) {
            viewModel.SelectedDish(clikedItem.DishId());
            }
            return true;
        }

        //loadInformation();

        viewModel.loadInformation = function () {


            $.ajax({
                url: "/api/WeekMenu",
                type: "GET"
            }).done(function (resp) {
                viewModel.MenuId(resp.id);
                viewModel.WeekNumber(resp.weekNumber);
                ko.utils.arrayForEach(resp.mfD_models, function (object, index) {
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

        viewModel.save = function () {

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
            if (dish != undefined) {

                viewModel.MFD_models()[viewModel.UpdatableMFD()].Dishes()[catIndex].update( dish);
            };

            console.log(viewModel.MFD_models()[viewModel.UpdatableMFD()].Dishes()[catIndex].Title());

            console.log(viewModel.UpdatableMFD());

            $("#modalbox").modal("hide");
        }


    viewModel.CalcTotal = function() {

        var sum = 0;
        var ind = -1;
        for (var ind = 0; ind < viewModel.MFD_models().length; ind++) {

            sum += parseFloat(viewModel.MFD_models()[ind].TotalPrice());
            console.log("ind=" + ind + " index=" + ind + " " + viewModel.MFD_models()[ind].DayOfWeek() + " - " + sum);

        }

        this.SummaryPrice(sum.toFixed(2));
    };

    viewModel.MFD_models.subscribe = ko.computed(viewModel.CalcTotal, viewModel);

    viewModel.loadInformation();

    ko.applyBindings(viewModel);

})();
