window.app = window.todoApp || {};

window.app.suService = (function () {

	var baseUri = '/api/WeekMenu/';
	var serviceUrls = {
        weekMenu: function(numweek, year) {
            numweek = numweek == undefined ? '' : numweek;
            year = year == undefined ? '' : "/" + year;
            return baseUri + numweek + year;
        },
        weekNumbers: function() { return baseUri + 'WeekNumbers'; },
        currentweek: function () { return baseUri + 'curWeekNumber'; },
        dishesByCategory: function(id) { return "/api/Dishes/byCategory/" + id; },
        categories: function() { return baseUri + 'categories' },
        create: function() { return baseUri + 'create' }
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
            return ajaxRequest('get', serviceUrls.weekMenu(numweek, year));
        },
        GetCurrentWeekNumber: function () {
            return ajaxRequest('get', serviceUrls.currentweek());
        },
        LoadWeekNumbers: function () {
            return ajaxRequest('get', serviceUrls.weekNumbers());
        },
        update: function (item) {
            return ajaxRequest('put', baseUri, item);
        },
        create: function (item) {
            return ajaxRequest('post', serviceUrls.create(), item);
        },
        GetCategories: function () {
            return ajaxRequest('get', serviceUrls.categories());
        },
        DishesByCategory: function (id) {
            return ajaxRequest('get', serviceUrls.dishesByCategory(id));
        }
    };

})();
