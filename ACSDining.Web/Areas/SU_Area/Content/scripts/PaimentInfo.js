(function() {

    var UserPaimentModel = function(value) {

        var self = this;
        self.isEditMode = ko.observable(false);
        self.Paiment = ko.observable(value);

        self.clicked = function(item) {
            $(item).focusin();
        };
        self.doubleClick = function() {
            this.isEditMode(true);
        };
        self.onFocusOut = function() {
            this.isEditMode(false);
        };
    }
    var PaimentViewModel = function() {
        var self = this;

        self.UserPiments = ko.observableArray([]);
        self.WeekNumber = ko.observable();
        self.NumbersOfWeek = ko.observableArray([]);
        self.Message = ko.observable();
        self.CurrentWeekNumber = ko.observable();
        self.myDate = ko.observable(new Date());
        self.BeenChanged = ko.observable(false);
        self.Year = ko.observable();
        self.Categories = ko.observableArray([]);
        // Callback for error responses from the server.
        function onError(error) {
            OrdersViewModel.Message('Error: ' + error.status + ' ' + error.statusText);
        }
    }

    ko.applyBindings(new PaimentViewModel());

}());