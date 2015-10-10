$().ready(function () {

    $('#Modal').on('show.bs.modal', function (event) {

        var element = $(event.relatedTarget);


        var url = element.data('whatever');
        $.ajax({
            url: url,
            type: "GET",
            success: function (response) {
                $('.modal-content > .modal-body').html(response);

            },

            error: function (xhr, status, error) {
                $('.modal-content > .modal-body').html(error);
            }
        });

        $('#Modal').on('hide.bs.modal', function (event) {
            $('.modal-content > .modal-body').empty();
        });

        //var button = $(event.relatedTarget) // Button that triggered the modal
        //var recipient = button.data('whatever') // Extract info from data-* attributes
        //// If necessary, you could initiate an AJAX request here (and then do the updating in a callback).
        //// Update the modal's content. We'll use jQuery here, but you could use a data binding library or other methods instead.
        //var modal = $(this)
        //modal.find('.modal-title').text('New message to ' + recipient)
        //modal.find('.modal-body input').val(recipient)
    });
});