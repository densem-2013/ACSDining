/// <reference path="../jquery-2.1.3.min.js" />
/// <reference path="../knockout-3.2.0.js" />


(function () {
    var viewModel = function () {
        var self = this;

        var IsUpdatable = false;

        self.MenuId = ko.observable();
        self.WeekNumber = ko.observable();
        self.SummaryPrice = ko.observable(0);
        self.MFD_models = ko.observableArray([]);

        self.Message = ko.observable("");

        
        var obj = {
            categories: ["Первое блюдо", "Второе блюдо", "Салат", "Напиток"],
            target: [4],
            sortFunc : function (value) {
                for (var i = 0; i < 4; i++) {
                    if (value.category == this.categories[i]) this.target[i] = value;
                }
            }
        }


        var MenuForDayInfo = function (item) {

            this.ID = ko.observable(item.id);
            this.DayOfWeek = ko.observable(item.dayOfWeek);
            this.TotalPrice = ko.observable(item.totalPrice);
            this.Dishes - ko.observable(item.dishes);

            this.Editing = ko.observable(false);
            this.Editable = ko.observable();

            this.Editable = function () {
                this.Editing(true);
            };

            this.UnEditable = function () {
                this.Editing(false);
            };

        }

        self.DishesByCategory = ko.observableArray();
        self.Category = ko.observable();

        self.SelectedDish = ko.observable();

        function loadDishes(id) {
            $.ajax({
                url: "/api/byCategory/" + id,
                type: "GET"
            }).done(function (resp) {
                $.each(resp, function (index, object) {
                    self.DishesByCategory.push({
                        DishId: object.dishID,
                        Title: object.title,
                        ProductImage: object.productImage,
                        Price: object.price,
                        Category: object.category,
                        IsSelected: object.dishID==id
                    });
                    if (object.dishID == id) {
                        self.SelectedDish(object.dishID);
                    }
                });
            });
        }


        self.showDishes = function (searchdish)
        {
            self.Category(searchdish.category);
            loadDishes(searchdish.dishID);
            $("#modalbox").modal("show");
        }

        self.changeSelected = function (clikedItem)
        {
            if (self.SelectedDish() !== clikedItem.DishId)
            {
                self.SelectedDish(clikedItem.DishId);
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
                    self.MFD_models.push(
                        {
                            ID: object.id,
                            DayOfWeek: object.dayOfWeek,
                            TotalPrice: object.totalPrice.toFixed(2),
                            Dishes: object.dishes,
                            Editing: ko.observable(false),
                            Editable: function () {
                                this.Editing(true);
                            },
                            UnEditable: function () {
                                this.Editing(false);
                            }
                        });
                });
            }).error(function (err) {
                self.Message("Error! " + err.status);
            });
        }

        //self.getSelected = function (per) {
        //    self.PersonId(per.PersonId);
        //    self.FirstName(per.FirstName);
        //    self.MiddleName(per.MiddleName);
        //    self.LastName(per.LastName);
        //    self.Address(per.Address);
        //    self.City(per.City);
        //    self.State(per.State);
        //    self.PhoneNo(per.PhoneNo);
        //    self.MobileNo(per.MobileNo);
        //    self.EmailAddress(per.EmailAddress);
        //    self.Occupation(per.Occupation);
        //    IsUpdatable = true;
        //    $("#modalbox").modal("show");
        //}

        self.save = function () {
            
            var models = self.MFD_models();
            $.each(models.Dishes, function (key, value) {
                if (value.DishId == self.SelectedDish()) {
                    models[key] = value;
                }
            });
            self.MFD_models(models);
            $("#modalbox").modal("show");
            //if (!IsUpdatable) {

            //    $.ajax({
            //        url: "/api/PersonAPI",
            //        type: "POST",
            //        data: PersonInfo,
            //        datatype: "json",
            //        contenttype: "application/json;utf-8"
            //    }).done(function (resp) {
            //        self.PersonId(resp.PersonId);
            //        $("#modalbox").modal("hide");
            //        loadInformation();
            //    }).error(function (err) {
            //        self.Message("Error! " + err.status);
            //    });
            //} else {
            //    $.ajax({
            //        url: "/api/PersonAPI/"+self.PersonId(),
            //        type: "PUT",
            //        data: PersonInfo,
            //        datatype: "json",
            //        contenttype: "application/json;utf-8"
            //    }).done(function (resp) {
            //        $("#modalbox").modal("hide");
            //        loadInformation();
            //        IsUpdatable = false;
            //    }).error(function (err) {
            //        self.Message("Error! " + err.status);
            //        IsUpdatable = false;
            //    });

            //}
        }

        //self.delete = function (per) {
        //    $.ajax({
        //        url: "/api/PersonAPI/" + per.PersonId,
        //        type: "DELETE",
        //    }).done(function (resp) {
        //        loadInformation();
        //    }).error(function (err) {
        //        self.Message("Error! " + err.status);
        //    });
        //}

    };

    var vm = new viewModel();
    console.log(vm);
    // ko.applyBindings(vm);

    // Activates knockout.js
    ko.applyBindings(vm);
})();
   