window.app = window.todoApp || {};

window.app.su_Service = (function () {

	var baseWeekMenuUri = '/api/WeekMenu/';
	var serviceWeekMenuUrls = {
        weekMenu: function(numweek, year) {
            numweek = numweek == undefined ? '' : numweek;
            year = year == undefined ? '' : "/" + year;
            return baseWeekMenuUri + numweek + year;
        },
        weekNumbers: function() { return baseWeekMenuUri + 'WeekNumbers'; },
        currentweek: function () { return baseWeekMenuUri + 'curWeekNumber'; },
        dishesByCategory: function(id) { return "/api/Dishes/byCategory/" + id; },
        categories: function() { return baseWeekMenuUri + 'categories' },
        create: function() { return baseWeekMenuUri + 'create' }
    }

    function ajaxRequest(type, url, data) {
        var options = {
            url: url,
            headers: {
                Accept: "application/json"
            },
            contentType: "application/json",
            cache: false,
            type: type,
            data: data ? ko.toJSON(data) : null
        };
        return $.ajax(options);
    }


    return {
        LoadWeekMenu: function (numweek, year) {
            return ajaxRequest('get', serviceWeekMenuUrls.weekMenu(numweek, year));
        },
        GetCurrentWeekNumber: function () {
            return ajaxRequest('get', serviceWeekMenuUrls.currentweek());
        },
        LoadWeekNumbers: function () {
            return ajaxRequest('get', serviceWeekMenuUrls.weekNumbers());
        },
        update: function (item) {
            return ajaxRequest('put', baseWeekMenuUri, item);
        },
        create: function (item) {
            return ajaxRequest('post', serviceWeekMenuUrls.create(), item);
        },
        GetCategories: function () {
            return ajaxRequest('get', serviceWeekMenuUrls.categories());
        },
        DishesByCategory: function (id) {
            return ajaxRequest('get', serviceWeekMenuUrls.dishesByCategory(id));
        }
    };

})();
