$(document)
    .ready(function () {

        $("#submit-btn").click(function(e) {
            var title = $("#title-input").val();
            var description = $("#description-input").val();

            if (title.indexOf("<") >= 0 || description.indexOf("<") >= 0) {
                e.preventDefault();
                alert("Dont write '<'");
            }
        })
    });