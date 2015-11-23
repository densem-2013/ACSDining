(function() {

    $("#infoTitle span").attr({ 'data-bind': 'text: WeekTitle' });


    var NoteValueModel = function (value) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Note = ko.observable(value);

        self.clicked = function (item) {
            $(item).siblings("input").first().focusin();
        };
        self.doubleClick = function () {
            self.isEditMode(true);
        };
        self.onFocusOut = function () {
            self.isEditMode(false);
        };
    }

    var PaimValueModel = function (value) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Paiment = ko.observable(value);

        self.clicked = function (item) {
            $(item).find("input:first-child").first().focusin();
        };
        self.doubleClick = function () {
            self.isEditMode(true);
        };
        self.onFocusOut = function () {
            self.isEditMode(false);
        };
    }

    var UserPaimentModel = function(item) {

        var self = this;

        self.Paiment = ko.observable(new PaimValueModel(item.weekPaid.toFixed(2)));
        self.DishPaiments = ko.observableArray(item.paiments);
        self.Balance = ko.observable(item.balance.toFixed(2));
        self.UserName = ko.observable(item.userName);
        self.Summary = ko.observable(item.summaryPrice.toFixed(2));
        self.Note = ko.observable(new NoteValueModel(item.note));
        self.IsDinningRoomClient = ko.observable(item.isDinningRoomClient);

    }
    var PaimentViewModel = function() {
        var self = this;

        self.UserPaiments = ko.observableArray([]);
        self.WeekNumber = ko.observable();
        self.NumbersOfWeek = ko.observableArray([]);
        self.Message = ko.observable();
        self.CurrentWeekNumber = ko.observable();
        self.myDate = ko.observable(new Date());
        self.BeenChanged = ko.observable(false);
        self.Year = ko.observable();
        self.Categories = ko.observableArray([]);
        self.UnitPrices = ko.observableArray([]);
        self.UnitPricesTotal = ko.observableArray([]);

        // Callback for error responses from the server.
        function onError(error) {
            self.Message('Error: ' + error.status + ' ' + error.statusText);
        }

        self.DaysOfWeek = ["Понедельник", "Вторник", "Среда", "Четверг", "Пятница"];

        self.pageSize = ko.observable(10);

        self.pageIndex = ko.observable(0);

        self.pagedList = ko.dependentObservable(function() {
            var size = self.pageSize();
            var start = self.pageIndex() * size;
            return self.UserPaiments.slice(start, start + size);
        });

        self.maxPageIndex = ko.dependentObservable(function() {
            return Math.ceil(self.UserPaiments().length / self.pageSize()) - 1;
        });

        self.previousPage = function() {
            if (self.pageIndex() > 0) {
                self.pageIndex(self.pageIndex() - 1);
            }
        };

        self.nextPage = function() {
            if (self.pageIndex() < self.maxPageIndex()) {
                self.pageIndex(self.pageIndex() + 1);
            }
        };

        self.allPages = ko.dependentObservable(function() {
            var pages = [];
            for (i = 0; i <= self.maxPageIndex(); i++) {
                pages.push({ pageNumber: (i + 1) });
            }
            return pages;
        });

        self.moveToPage = function(index) {
            self.pageIndex(index);
        };

        self.loadWeekNumbers = function() {
            app.su_Service.LoadWeekNumbers().then(function(resp) {

                self.NumbersOfWeek.pushAll(resp);

            }, onError);
        };
        self.GetCurrentWeekNumber = function() {

            app.su_Service.GetCurrentWeekNumber().then(function(resp) {
                self.CurrentWeekNumber(resp);

            }, onError);
        }

        self.IsCurrentWeek = ko.computed(function() {
            return self.CurrentWeekNumber() === self.WeekNumber();
        }.bind(self));


        self.LoadPaiments = function (weeknumb, yearnum) {

            app.su_Service.GetPaiments(weeknumb, yearnum).then(
                function(resp) {
                    self.UserPaiments([]);
                    if (resp != null) {
                        var wn = resp.weekNumber;
                        self.WeekNumber(wn);
                        self.Year(resp.yearNumber);

                        self.UnitPrices([]);
                        self.UnitPricesTotal([]);
                        self.UnitPrices.pushAll(resp.unitPrices);
                        self.UnitPricesTotal.pushAll(resp.unitPricesTotal);

                        ko.utils.arrayForEach(resp.userPaiments, function (object) {

                            self.UserPaiments.push(new UserPaimentModel(object));

                        });
                    }
                },
                function(error) {
                    onError(error);
                });
        }


        self.myDate.subscribe = ko.computed(function () {
            var takedWeek = self.myDate().getWeek() + 1;
            var curweek = self.WeekNumber();
            if (takedWeek !== curweek) {
                self.LoadPaiments(takedWeek, self.myDate().getFullYear());
            }
        });

        self.SetMyDateByWeek = function (weeknumber) {
            var year = self.Year();
            var firstDay = new Date(year, 0, 1).getDay();
            var d = new Date("Jan 01, " + year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (weeknumber - 1);
            self.myDate(new Date(w));
        }.bind(self);


        self.init = function () {

            self.LoadPaiments();
            app.su_Service.GetCategories().then(function (resp) {
                self.Categories.pushAll(resp);
            }, onError);

            self.loadWeekNumbers();
            self.GetCurrentWeekNumber();

        }

        self.WeekTitle = ko.computed(function () {
            var options = {
                weekday: "short",
                year: "numeric",
                month: "short",
                day: "numeric"
            };
            var year = self.Year();
            var firstDay = new Date(year, 0, 1).getDay();

            var week = self.WeekNumber();
            var d = new Date("Jan 01, " + year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week - 1);
            var n1 = new Date(w);
            var n2 = new Date(w + 345600000);
            return "Неделя " + week + ", " + n1.toLocaleDateString("ru-RU", options) + " - " + n2.toLocaleDateString("ru-RU", options);
        }.bind(self));


        self.init();
    }

    ko.applyBindings(new PaimentViewModel());

}());