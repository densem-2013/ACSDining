(function () {

    var workDayInfo = function(workday) {
        var self = this;
        self.WorkDayId = ko.observable(workday.workDayId);
        self.IsWorking = ko.observable(workday.isWorking);
        self.DayNumber = ko.observable(workday.dayNumber);
    }

    var workingWeekViewModel = function() {
        var self = this;
        self.WorkweekId = ko.observable();
        self.WeekNumber = ko.observable();
        self.YearNumber = ko.observable();
        self.WorkingDays = ko.observableArray();
        self.CanBeChanged = ko.observable();

        function onError(error) {
            self.Message('Error: ' + error.status + ' ' + error.statusText);
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

        self.loadWorkWeeks();
    }

    ko.applyBindings(new workingWeekViewModel());

}());