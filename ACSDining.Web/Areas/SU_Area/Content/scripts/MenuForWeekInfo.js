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
        self.Price = ko.observable(object.price);
        self.Category = ko.observable(object.category);

        self.update = function (dishupdate) {
            self.DishId(dishupdate.DishId);
            self.Title(dishupdate.Title);
            self.ProductImage(dishupdate.ProductImage);
            self.Price(dishupdate.Price);
            self.Category(dishupdate.Category);

        }
    }
    
    var MenuForDayInfo = function (mfdobject) {

        self = this;

        self.ID = ko.observable(mfdobject.id);
        self.DayOfWeek = ko.observable(mfdobject.dayOfWeek);
        self.TotalPrice = ko.observable(mfdobject.totalPrice.toFixed(2));
        self.Dishes = ko.observableArray([]).pushAll(mfdobject.dishes);

        self.Editing = ko.observable(false);

        self.Editable = function () {
            self.Editing(true);
        };

        self.UnEditable = function () {
            self.Editing(false);
        };

    }

    var obj = {
        categories: ["Первое блюдо", "Второе блюдо", "Салат", "Напиток"],
        target: [4],
        sortFunc: function (value) {
            for (var i = 0; i < 4; i++) {
                if (value.category == this.categories[i]) this.target[i] = value;
            }
        }
    }

    var viewModel = function () {
        var self = this;

        var IsUpdatable = false;

        self.MenuId = ko.observable();
        self.WeekNumber = ko.observable();
        self.SummaryPrice = ko.observable(0);
        self.MFD_models = ko.observableArray([]);

        self.Message = ko.observable("");

        

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
                self.DishesByCategory.pushAll(resp);
                $.each(resp, function (index, object) {
                    //self.DishesByCategory.push({
                    //    DishId: object.dishID,
                    //    Title: object.title,
                    //    ProductImage: object.productImage,
                    //    Price: object.price,
                    //    Category: object.category
                    //});
                    if (object.dishID == id) {
                        self.SelectedDish(object.dishID);
                    }
                });
            }).error(function (err) {
                self.Message("Error! " + err.status);
            });
        }


        self.showDishes = function (searchdish,index)
        {
            self.UpdatableMFD(index);
            self.Category(searchdish.category);
            loadDishes(searchdish.dishID);
            $("#modalbox").modal("show");
        }

        self.changeSelected = function (clikedItem)
        {
            if (self.SelectedDish() !== clikedItem.dishID)
            {
                self.SelectedDish(clikedItem.dishID);
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
                self.SummaryPrice(resp.summaryPrice.toFixed(2));
                $.each(resp.mfD_models, function (index, object) {
                    object.dishes.map(obj.sortFunc, obj);
                    object.dishes = obj.target;
                    self.MFD_models.push(new MenuForDayInfo(object));
                });
            }).error(function (err) {
                self.Message("Error! " + err.status);
            });
        }

        self.save = function (ind) {
            var catIndex = $.map(obj.categories, function (n, i) {
                if (self.Category()==n) 
                    return i;
                });
            var Dishes = self.DishesByCategory();
            var models = self.MFD_models();
            $.each(Dishes, function (key, value) {
                if (value.DishId == self.SelectedDish()) {

                    models[ind].Dishes[catIndex].update(value);
                }
            });
            self.MFD_models([]);
            self.MFD_models.pushAll(models);

            $("#modalbox").modal("hide");
        }


    };
    viewModel.MenuForDayModels = ko.dependentObservable(function () {
        self.Sum.load
    })

    var vm = new viewModel();
    console.log(vm);
    ko.applyBindings(vm);
})();
   