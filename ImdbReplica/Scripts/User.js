$(document).ready(function () {
    $("#remove-reviews").on("click", function () {
        deleteReview();
    });

    $(function () {
        var availableUsers = [];

        $.ajax({
            url: "/User/SearchUsers",
            type: "POST",
            dataType: "json",
            success: function (data) {
                console.log(data);
                if (data.succeeded === 1) {
                    $.each(data.user, function (index, username) {
                        availableUsers.push(username);
                    });
                }
                else if (data.succeeded === 2) {
                    availableUsers.push("(No users to show!)");
                }

                else {
                    Command: toastr["error"]("Error", "An error occured when trying to search for users, please reload the page and try again.")
                }
            }
        });

        //$("#user-search-input").autocomplete({
        //    source: availableUsers
        //});
    });

    $("#user-search-button").on("click", function () {
        var value = $("#user-search-input").val();
        goToSearchedUser(value);
    });

    // Toastr-options
    toastr.options = {
        "closeButton": false,
        "debug": false,
        "newestOnTop": false,
        "progressBar": false,
        "positionClass": "toast-bottom-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "500",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }
});

function deleteReview() {
    var removeConfirmation = confirm("Do you really want to delete this review?");
    if (removeConfirmation === true) {
        var selectedReview = new Array();
        $(".selected-review-items:checked").each(function () {
            selectedReview.push($(this));
        });

        removeSelectedReviews(selectedReview);
    }
}

function removeSelectedReviews(selectedReview) {
    var array = [];
    $.each(selectedReview,
        function(index, review) {
            array.push($(review).val());
        });

    $.ajax({
        url: "/Review/DeleteSelectedReviews",
        type: "POST",
        traditional: true,
        dataType: "json",
        data: { selectedReviewsIds: array },
        success: function (succeeded) {
            if (succeeded !== true) {
                command: toastr["error"]("Error", "You must mark the review you want to delete!");
            }
            else {
                $.each(selectedReview,
                    function(index, reviewToRemove) {
                        $(reviewToRemove).parent().parent().remove();
                    });

                command: toastr["success"]("Deleted", "Review(s) deleted successfully!");
            }
        }
    });
}

function goToSearchedUser(value) {
    $.ajax({
        url: "/User/GoToSearchedUser",
        type: "POST",
        dataType: "json",
        data: { username: value },
        success: function (data) {
            if (data.succeeded === 1) {
                if (data.currentUsername !== value) {
                    var url = "/User/DifferentUser?CreatorUserId=" + data.searchedUserId;
                    window.location.href = url;
                }
                else {
                    var url = "/User/";
                    window.location.href = url;
                }
            }
            else if (data.succeeded === 2) {
                    Command: toastr["error"]("Error", "An error occured, please reload the page and try again.")
            }
            else {
                Command: toastr["error"]("Failed", "No user found with the name you provided, please reload the page and try again.")
            }
        }
    });
}