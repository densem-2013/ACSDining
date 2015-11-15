 (function() {

 //    var AppSUviewModel = {

 //        MenuForWeekInfo: {},
 //        OrdersInfo: {},
 //        DishesInfo: {},
 //        PaimentInfo: {},
 //        myDate: ko.observable(new Date())
 //};
    var AppSUviewModel = {
        MenuForWeekInfo: ko.observable(),
        OrdersInfo: ko.observable(),
        DishesInfo: ko.observable(),
        PaimentInfo: ko.observable()
    };

    ko.observableArray.fn.pushAll = function(valuesToPush) {
        var underlyingArray = this();
        this.valueWillMutate();
        ko.utils.arrayPushAll(underlyingArray, valuesToPush);
        this.valueHasMutated();
        return this;
    };

    Date.prototype.getWeek = function () {
        var onejan = new Date(this.getFullYear(), 0, 1);
        var today = new Date(this.getFullYear(), this.getMonth(), this.getDate());
        var dayOfYear = ((today - onejan + 1) / 86400000);
        return Math.ceil(dayOfYear / 7);
    };

    //var model = new AppSUviewModel();
    window.app.AppViewModel = AppSUviewModel;
     //return AppSUviewModel;
   // ko.applyBindings(AppSUviewModel);
})();