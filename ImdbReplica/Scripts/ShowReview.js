$(document).ready(function () {
    $("#add-comment").on("click", function () {
        var commentText = $("textarea#txt-area-comment").val();
        var reviewId = $("#ReviewId").val();
        addCommentToReview(commentText, reviewId);
    });

    $("#btn-like").on("click", function () {
        likeOrDislikeReviewFunction(1, $("#btn-like").val());
    });

    $("#btn-dislike").on("click", function () {
        likeOrDislikeReviewFunction(0, $("#btn-dislike").val());
    });

    $("#rate-review").on("click", function () {
        var radioValue = $("#rating-rdb:checked").val();
        var reviewID = $("#ReviewId").val();
        rateReview(radioValue, reviewID);
    });

    $("#rating").on("click", function () {
        var value = $("#rating").val();
        showAllRating(value)
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

function showAllRating(value) {
    $.ajax({
        url: "/Review/GetReviewRatings",
        type: "POST",
        dataType: "json",
        data: { reviewID: value },
        success: function (data) {
            console.log(data.succeeded);
            var div = $("#modal-wrapper");
            div.empty();
            if (data.succeeded === 1) {
                $.each(data.ratingsToReview, function (index, review) {
                    if (review.Rating != null) {
                        div.append("<p>" + review.Username + "</p>");
                        div.append("<p>" + "Rated this review with a: " + review.Rating);
                    }
                });
            }
            else if (data.succeeded === 2) {
                    Command: toastr["error"]("Error", "An error occured, reload the page and try again!");
            }
            else {
                div.append("<p>No one rated this review.</p>");
            }
        }
    });
}


function rateReview(radioValue, reviewId) {
    $.ajax({
        url: "/Review/RateReview",
        type: "POST",
        dataType: "json",
        data: { checkedValue: radioValue, reviewId: reviewId },
        success: function (data) {
            console.log(data);
            if (data.succeeded === 6) {
                Command: toastr["error"]("Error", "An error occured, reload the page and try again!");
            }
            else if (data.succeeded > 0) {
                    Command: toastr["success"]("Success", "You rated this review!!");
                $("#rating").text(data.rating);
            }
            else if (data.succeeded === 0) {
                    Command: toastr["warning"]("Error", "You already rated this review.");
            }
        }
    })
}

function addCommentToReview(commentText, reviewId) {
    $.ajax({
        url: "/Review/CreateCommentToReview",
        type: "POST",
        traditional: true,
        dataType: "json",
        data: { CommentToAdd: commentText, reviewId: reviewId },
        success: function (data) {
            console.log(data);
            if (data.allReviewsViewModel !== null) {
                $("textarea#txt-area-comment").val("");

                var comments = $("#comment-wrapper");

                var noCommentsText = $("#no-comments-text");

                noCommentsText.remove();

                comments.append("<p>" + data.allReviewsViewModel.CreatedBy + ": " + data.allReviewsViewModel.CommentToAdd + ", " + data.allReviewsViewModel.CreatedDate + "</p>")

                Command: toastr["success"]("Success", "Your comment is shared!")
            }
            else if (data.succeeded === false) {
                    Command: toastr["error"]("Error", "An error occured, reload the page and try again.")
            }
            else {
                Command: toastr["error"]("Error", "You need to write more than 3 or less than 150 characters.")
            }
        }
    });
}

function likeOrDislikeReviewFunction(likeOrDislike, likeOrDislikeValue) {
    $.ajax({
        url: "/Review/LikeOrDislikeReview",
        type: "POST",
        traditional: true,
        dataType: "json",
        data: { likeOrDislike: likeOrDislike, reviewId: likeOrDislikeValue },
        success: function (succeeded) {
            if (succeeded === 2) {
                Command: toastr["error"]("Dislike", "You dont like this review.");
                var value = parseInt($("#disLikesSpan").text());
                var newValue = value + 1;
                $("#disLikesSpan").text(newValue);
            }
            else if (succeeded === 1) {
                Command: toastr["success"]("Like", "You like this review!");
                var value = parseInt($("#likesSpan").text());
                var newValue = value + 1;
                $("#likesSpan").text(newValue);
            }
            else if (succeeded === 3) {
                Command: toastr["error"]("Error", "An error occured, reload the page and try again.");
            }
            else {
                Command: toastr["warning"]("Error", "You already liked/disliked this review!");
            }
        }
    });
}
