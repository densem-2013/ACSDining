(function() {

    $("#infoTitle span").attr({ 'data-bind': "text: WeekTitle" })
        .css({ 'background': "rgba(119, 222, 228, 0.61)", 'color': "rgb(232, 34, 208)", 'border': "3px solid rgb(50, 235, 213)" });


    $("ul.nav.navbar-nav li:nth-child(4)").addClass("active");
    $("#autorizeMessage span").css({ 'paddingLeft': "160px" });
    var excelButtonDiv = $('<div></div>').css({ 'whith': '100%', 'padding': '10px' });
    var sendButtonInput = $('<input type="button" id="btExcel" class="btn btn-info" value="Выгрузить в Excel" data-bind="click: GetExcel"/>');
    excelButtonDiv.append(sendButtonInput);
    $('#datepick').append(excelButtonDiv);

    var noteValueModel = function (value) {

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

    var paimValueModel = function (value) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Paiment = ko.observable(value);
        self.formattedPaiment = ko.pureComputed({
            read: function () {
                return self.Paiment();
            },
            write: function (value) {
                value = value.replace(",", ".");
                value = value.replace(new RegExp(/(.*)(\.)+(.*)(\.)(.*)/g), "$1$2$3");
                value = value.replace(/[^\.\d]/g, "");
                value = parseFloat(value);
                self.Paiment(isNaN(value) ? 0 : value); // Write to underlying storage
            },
            owner: self
        });
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

    var userPaimentModel = function(item) {

        var self = this;
        self.PaiId = ko.observable(item.paiId);
        self.Paiment = ko.observable(new paimValueModel(item.paiment.toFixed(2)));
        var summ = item.weekPaiments.pop();
        self.DishPaiments = ko.observableArray(item.weekPaiments);
        self.Balance = ko.observable(item.balance.toFixed(2));
        self.UserName = ko.observable(item.userName);
        self.Summary = ko.observable(summ.toFixed(2));
        self.Note = ko.observable(new noteValueModel(item.note));
        self.LastWeekBalance = ko.observable(item.prevWeekBalance);

    }
    var paimentViewModel = function() {
        var self = this;

        self.UserPaiments = ko.observableArray([]);
        self.WeekYear = ko.observable();

        self.IsNextWeekYear = ko.observable();

        self.DaysOfWeek = ko.observableArray([]);

        self.TotalCount = ko.observable();

        self.BeenChanged = ko.observable(false);


        self.CurrentWeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.WeekYear = ko.observable(new WeekYear({ week: 0, year: 0 }));

        self.Message = ko.observable();
        self.CurrentWeekYear = ko.observable();
        self.myDate = ko.observable(new Date());
        self.BeenChanged = ko.observable(false);
        self.Year = ko.observable();
        self.Categories = ko.observableArray([]);
        self.UnitPrices = ko.observableArray([]);
        self.UnitPricesTotal = ko.observableArray([]);
        self.TotalWeekPaiment = ko.observable();
        self.TotalBalance = ko.observable();

        self.PageSizes = ko.pureComputed(function () {
            var res = [2, 5, 7, 10, 15, 20, 25];
            var all = self.UserPaiments().length;
            if (all > 25) {
                res.push(all);
            }
            return res;

        });
        // Callback for error responses from the server.
        function onError(error) {
            self.Message("Error: " + error.status + " " + error.statusText);
        }

        self.DaysOfWeek = ko.observableArray([]);

        self.pageSize = ko.observable(10);

        self.pageIndex = ko.observable(0);

        self.pagedList = ko.dependentObservable(function () {
            var size = self.pageSize();
            var start = self.pageIndex() * size;
            return self.UserPaiments.slice(start, start + size);
        });

        self.maxPageIndex = ko.dependentObservable(function () {
            return Math.ceil(self.UserPaiments().length / self.pageSize()) - 1;
        });


        self.allPages = ko.dependentObservable(function () {
            var pages = [];
            for (var i = 0; i <= self.maxPageIndex() ; i++) {
                pages.push({ pageNumber: (i + 1) });
            }
            return pages;
        });

        self.moveToPage = function (index) {
            self.pageIndex(index);
        };


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

       

        self.allPages = ko.dependentObservable(function () {
            var pages = [];
            for (var i = 0; i <= self.maxPageIndex() ; i++) {
                pages.push({ pageNumber: (i + 1) });
            }
            return pages;
        });

        self.GetCurrentWeekYear = function() {

            app.su_Service.GetCurrentWeekYear().then(function(resp) {
                self.CurrentWeekYear(resp);

            }, onError);
        }

        self.IsCurrentWeek = ko.pureComputed(function () {

            return self.CurrentWeekYear().week === self.WeekYear().week && self.CurrentWeekYear().year === self.WeekYear().year;

        }, self);

        self.CalcBalance = function (uspaiobj) {

            var sum = uspaiobj.LastWeekBalance() - uspaiobj.Summary() + parseFloat(uspaiobj.Paiment().Paiment());
            uspaiobj.Balance(sum.toFixed(2));
        };

        self.update = function(uspaiobj) {
            var forupdate = {
                Id: uspaiobj.PaiId(),
                Paiment: uspaiobj.Paiment().Paiment()
            }
            self.CalcBalance(uspaiobj);
            app.su_Service.UpdatePaiment(forupdate).then(function(res) {
                self.BeenChanged(!res);
            });
        };
        self.GetExcel = function() {
            app.su_Service.GetExcelPaiments(self.WeekYear())
                .then(function (res) {
                    window.location.assign(res.fileName);
                });
        };
        self.LoadPaiments = function (wyDto) {

            app.su_Service.GetPaiments( wyDto).then(function (resp) {

                if (resp != null) {

                        self.WeekYear(resp.weekYearDto);
                        self.UnitPrices(resp.weekDishPrices);
                        self.UnitPricesTotal(resp.summaryDishPaiments);
                        self.DaysOfWeek(resp.dayNames);
                        self.UserPaiments(ko.utils.arrayMap(resp.userWeekPaiments,function(item) {
                            return new userPaimentModel(item);
                        }));
                    }
                }, onError);
        }


        self.myDate.subscribe = ko.computed(function () {
            var takedWeek = self.myDate().getWeek() - 1;
            var needObj = self.WeekYear();
            if (needObj != undefined && !isNaN(takedWeek)) {
                var curweek = needObj.week;
                if (!isNaN(takedWeek) && takedWeek !== curweek) {
                    var weekyear = {
                        Week: takedWeek,
                        Year: self.myDate().getFullYear()
                    };
                    if (!isNaN(weekyear.Week) && !isNaN(weekyear.Year)) {

                        self.LoadPaiments(weekyear);
                    }
                };
            };
        }, self);
        self.SetMyDateByWeek = function (wyDto) {
            var firstDay = new Date(wyDto.year, 0, 1).getDay();
            var d = new Date("Jan 01, " + wyDto.year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (wyDto.week);
            self.myDate(new Date(w));
        }.bind(self);

        self.WeekTitle = ko.computed(function () {
            var options = {
                weekday: "short",
                year: "numeric",
                month: "short",
                day: "numeric"
            };

            var year = self.WeekYear().year;
            var firstDay = new Date(year, 0, 1).getDay();

            var week = self.WeekYear().week;
            var d = new Date("Jan 01, " + year + " 01:00:00");
            var w = d.getTime() - (3600000 * 24 * (firstDay - 1)) + 604800000 * (week);
            var n1 = new Date(w);
            var n2 = new Date(w + 345600000);
            return "Неделя " + week + ", " + n1.toLocaleDateString("ru-RU", options) + " - " + n2.toLocaleDateString("ru-RU", options);
        }.bind(self));



        self.init = function () {

            self.LoadPaiments();
            app.su_Service.GetCategories().then(function (resp) {
                self.Categories(resp);
            }, onError);

            app.su_Service.GetCurrentWeekYear().then(function (resp) {

                self.CurrentWeekYear(resp);

                self.SetMyDateByWeek(self.CurrentWeekYear());


            }, onError).then(function () {
               // self.GetUnitWeekPaiments();
            }
            );
            //self.loadWeekNumbers();
            //self.GetCurrentWeekYear();

        }


        self.init();
    }

    ko.applyBindings(new paimentViewModel());

}());