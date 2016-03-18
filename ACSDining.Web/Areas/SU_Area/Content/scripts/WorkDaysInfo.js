(function () {

    var workDayInfo = function(workday) {
        var self = this;
        self.WorkDayId = ko.observable(workday.workDayId);
        self.IsWorking = ko.observable(workday.isWorking);
        self.DayNumber = ko.observable(workday.dayNumber);
        self.DayName = ko.observable(workday.dayName);
    }

    var workingWeekViewModel = function() {
        var self = this;
        self.WorkweekId = ko.observable();
        self.WeekNumber = ko.observable();
        self.YearNumber = ko.observable();
        self.WorkingDays = ko.observableArray();
        self.CanBeChanged = ko.observable();
        self.Message = ko.observable();

        function onError(error) {
            self.Message("Error: " + error.status + " " + error.statusText);
            $("#modalMessage").modal("show");
        };

        self.loadWorkWeeks = function(week, year) {
            app.su_Service.GetWorkDays(week, year).then(function(resp) {
                self.WorkingDays([]);

                self.WorkweekId(resp.workWeekId);
                self.WeekNumber(resp.weekNumber);
                self.YearNumber(resp.yearNumber);
                self.CanBeChanged(resp.canBeChanged);

                self.WorkingDays().pushAll(ko.utils.arrayMap(resp, function(item) {
                    return new workDayInfo(item);
                }));
            }, onError);
        };
        self.updateWorkDays = function() {
            var item = {
                WorkweekId: self.WorkweekId(),
                WeekNumber: self.WeekNumber(),
                YearNumber: self.YearNumber(),
                WorkingDays: self.WorkingDays(),
                CanBeChanged: self.CanBeChanged()
            };
            app.su_Service.UpdateWorkDays(item).then(function(resp) {

            }, onError);
        }
        self.loadWorkWeeks();
    }

    ko.applyBindings(new workingWeekViewModel());

}());