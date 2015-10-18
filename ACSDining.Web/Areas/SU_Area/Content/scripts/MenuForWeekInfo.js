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

    var DishInfo = function (object) {

        self = this;

        self.DishId = ko.observable(object.dishID);
        self.Title = ko.observable(object.title);
        self.ProductImage = ko.observable(object.productImage);
        self.Price = ko.observable(object.price.toFixed(2));

        self.Category = ko.observable(object.category);

    }
    
    DishInfo.prototype.update = function (dishupdate) {
        this.DishId(dishupdate.DishId());
        this.Title(dishupdate.Title());
        this.ProductImage(dishupdate.ProductImage());
        this.Price(dishupdate.Price());

    }

    var obj = {
        categories: ["Первое блюдо", "Второе блюдо", "Салат", "Напиток"],
        target: [4],
        sortFunc: function (value) {
            for (var i = 0; i < 4; i++) {
                if (value.category == this.categories[i]) this.target[i] = new DishInfo(value);
            }
        }
    }

    var MenuForDayInfo = function (mfdobject) {

        self = this;
        self.ID = ko.observable();
        self.DayOfWeek = ko.observable(mfdobject.dayOfWeek);
        self.Dishes = ko.observableArray([]);
        self.Editing = ko.observable(false);

        if (mfdobject.hasOwnProperty('id')) {

            self.ID(mfdobject.id);
            self.DayOfWeek(mfdobject.dayOfWeek);

            mfdobject.dishes.map(obj.sortFunc, obj);

            self.Dishes(obj.target);

        }
        else {
            self.ID(mfdobject.ID);
            self.DayOfWeek(mfdobject.DayOfWeek);
            self.Dishes(mfdobject.Dishes);

        }

        self.TotalPrice = function () {
            var sum = 0;
            var valsum;
            var ind = -1;
            ko.utils.arrayForEach(this.Dishes(), function (dish, index) {
                if (ind != index) {
                    valsum = parseFloat(dish.Price());
                    sum += valsum;
                    ind++;

                }
            });

            return sum.toFixed(2);

        }.bind(this);


    }

    MenuForDayInfo.prototype.Editable = function () {
        this.Editing(true);
    };

    MenuForDayInfo.prototype.UnEditable = function () {
        this.Editing(false);
    };


    var viewModel = function () {
        var self = this;

        var IsUpdatable = false;

        self.MenuId = ko.observable();
        self.WeekNumber = ko.observable();

        self.MFD_models = ko.observableArray([]);

        self.Total = ko.observable();

        self.SummaryPrice = function () {

                var sum = 0;
                var ind = -1;
                ko.utils.arrayForEach(self.MFD_models(), function (value, index) {
                    if (ind != index) {

                        sum += parseFloat(value.TotalPrice());
                        console.log("ind=" + ind + " index=" + ind + " " + value.DayOfWeek() + " - " + sum);
                        ind++;

                    }
                });
                return sum.toFixed(2);
        }.bind(self);

        self.Message = ko.observable("");


        self.locArray = [];

        self.DishesByCategory = ko.observableArray([]);
        self.Category = ko.observable();

        self.SelectedDish = ko.observable();

        self.UpdatableMFD = ko.observable();

        function loadDishes(id) {
            $.ajax({
                url: "/api/byCategory/" + id,
                type: "GET"
            }).done(function (resp) {
                self.DishesByCategory([]);

                ko.utils.arrayForEach(resp, function (key, value) {

                    self.DishesByCategory.push(new DishInfo(key));

                    if (key.dishID == id) {
                        self.SelectedDish(key.dishID);
                    }
                });

            }).error(function (err) {
                self.Message("Error! " + err.status);
            });
        }


        self.showDishes = function (searchdish,index)
        {
            self.UpdatableMFD(index);
            self.Category(searchdish.Category());
            loadDishes(searchdish.DishId());
            $("#modalbox").modal("show");
        }

        self.changeSelected = function (clikedItem)
        {
            if (self.SelectedDish() !== clikedItem.DishId())
            {
                self.SelectedDish(clikedItem.DishId());
            }
            return true;
        }

        loadInformation();

        function loadInformation() {
         

            $.ajax({
                url: "/api/WeekMenu",
                type:"GET"
            }).done(function(resp) {
                self.MenuId(resp.id);
                self.WeekNumber(resp.weekNumber);

                ko.utils.arrayForEach(resp.mfD_models, function (object,index ) {
                    object.dishes.map(obj.sortFunc, obj);
                    object.dishes = obj.target;

                    self.MFD_models.push(new MenuForDayInfo(object));

                });

            }).error(function (err) {
                self.Message("Error! " + err.status);
            });
        }

        self.save = function () {


            var catIndex = $.map(obj.categories, function (n, i) {
                if (self.Category()==n) 
                    return i;
            });


            ko.utils.arrayForEach(self.DishesByCategory(), function (value, index) {
                if (value.DishId() == self.SelectedDish()) {

                    ko.utils.arrayForEach(self.MFD_models(), function (mfd, ind) {
                        if (ind == self.UpdatableMFD()) {

                            ko.utils.arrayForEach(mfd.Dishes(), function (dish, numcategory) {
                                if (numcategory == catIndex) {

                                    dish.update(value);
                                };
                            });
                        };
                    });
                };
            });

            console.log(self.MFD_models()[self.UpdatableMFD()].Dishes()[catIndex].Title());

            console.log(self.UpdatableMFD());

            $("#modalbox").modal("hide");
        }


    };


    ko.applyBindings(new viewModel());
})();
   