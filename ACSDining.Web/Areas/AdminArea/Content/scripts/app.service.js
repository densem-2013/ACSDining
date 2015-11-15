window.app = window.todoApp || {};

window.app.service = (function () {
    var baseUri = '/api/accounts/';
    var serviceUrls = {
        users: function() { return baseUri + 'users'; },
        byName: function(username) { return baseUri + '?username=' + username; },
        byId: function(id) { return baseUri + id; },
        createWeekMenu: function() { return baseUri + 'createWeekMenu' }
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
        allUsers: function () {
            return ajaxRequest('get', serviceUrls.users());
        },
        byName: function (username) {
            return ajaxRequest('get', serviceUrls.byName(username));
        },
        UpdateWeekMenu: function (item) {
            return ajaxRequest('put', serviceUrls.byId(item.ID), item);
        },
        createAccount: function (item) {
            return ajaxRequest('post', serviceUrls.createWeekMenu(), item);
        },
        deleteAccount: function (item) {
            return ajaxRequest('post', serviceUrls.byId(item.ID), item);
        }
    };
})();
