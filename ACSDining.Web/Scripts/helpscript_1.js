$(document).ready(function () {


    $('#index').addClass("active-menu");

    var targets_1, targets_2, targets;
    $(function () {

        defineTargets();

        $(targets).click(function (e) {
            $.ajax({
                url: $(this).attr('data-href'),
                type: "POST",
                success: function (response) {
                    $('#targ').html(response);

                    $.each($('#targ  a'), function (i, object) {
                        $(object).click(object,oncklickTarget);
                    });
                    e.preventDefault();
                },

                error: function (xhr, status, error) {
                    $('.container body-content').html(error);
                }
            });
        });
    });

    var defineTargets = function () {

        targets_1 = $('#main-menu.nav li a');
        targets_2 = $('#targ  a');
        targets = $.merge(targets_1, targets_2);

        var url = $(location).attr('href');

            $(targets_1).removeClass("active-menu");
        
            if (url.indexOf("UsersAdmin") > -1) {

                $('#user').addClass("active-menu");
            }else if (url.indexOf("RolesAdmin") > -1) {
                
                $('#role').addClass("active-menu");
            } else if (url.indexOf("/AdminArea/Admin") > -1)
            {
                $('#index').addClass("active-menu");
            }


    };
    var oncklickTarget = function (el) {
        $.ajax({
            url: $(el).attr('href'),
            type: "POST",
            success: function (response) {
                $('#targ').html(response);

            },

            error: function (xhr, status, error) {
                $('.container body-content').html(error);
            }
        });

        defineTargets();
        return false;
    };
});