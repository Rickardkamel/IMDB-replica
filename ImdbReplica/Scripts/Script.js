$(document).ready(function () {
    $('#remove-reviews').on('click', function () {
        reviewDeleteFunction();
    });

    $('#add-comment').on('click', function () {
        var commentText = $('textarea#txt-area-comment').val();
        var reviewID = $('#ReviewID').val();
        addCommentToReview(commentText, reviewID);
    });

    $('#btn-like').on('click', function () {
        likeOrDislikeReviewFunction(1, $('#btn-like').val());
    });

    $('#btn-dislike').on('click', function () {
        likeOrDislikeReviewFunction(0, $('#btn-dislike').val());
    });

    $('#rate-review').on('click', function () {
        var radioValue = $('#rating-rdb:checked').val();
        var reviewID = $('#ReviewID').val();
        rateReview(radioValue, reviewID);
    });

    $('#btn-sort-reviews').on('click', function () {
        var value = $('#sort-reviews option:selected').val();
        sortReviews(value);
    });

    $('#search-review-btn').on('click', function () {
        var value = $('#search-review-btn').val();
        searchReview(value);
    });

    $('#search-review-input').on('input', function () {
        var value = $('#search-review-input').val();
        refreshReviews(value);
    });

    $('#rating').on('click', function () {
        var value = $('#rating').val();
        showAllRating(value);
    });
});

function showAllRating(value) {
    $.ajax({
        url: '/Review/GetReviewRatings',
        type: 'POST',
        dataType: 'json',
        data: { reviewID: value },
        success: function (data) {            
            var html = '';
            $.each(data, function (index, review) {
                html += review.Username + ' rated this review with a: ' + review.Rating + '<br /><br />';
            });

            Command: toastr["info"](html + '<br /><br /><button type="button" class="btn clear">Close</button>', "People who rated are:");
        }
    });
}

function refreshReviews(value) {
    $.ajax({
        url: '/Review/RefreshReviews',
        type: 'POST',
        dataType: 'json',
        data: { inputValue: value },
        success: function (data) {
            printHtml(data);
        }
    });
}

function sortReviews(value) {
    $.ajax({
        url: '/Review/SortReviews',
        type: 'POST',
        dataType: 'json',
        data: { sortValue: value },
        success: function (data) {
            printHtml(data);
        }
    });
}

function searchReview(value) {
    $.ajax({
        url: '/Review/SearchReviews',
        type: 'POST',
        dataType: 'json',
        data: { searchValue: value },
        success: function (data) {
            if (data.succeeded == true) {
                printHtml(data.allReviews);
            }
            else {
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

                Command: toastr["error"]("Failed", "No review with that name, try again.")
            }
        }
    });
}

function printHtml(data) {
    var table = $('#reviews-table');
    var body = $('#review-table-data');

    body.html('');
    $.each(data, function (index, review) {
        var tr = $('<tr></tr>')

        tr.append('<td><label for="' + review.Title + '"/>' + review.Title + '</td>');
        tr.append('<td><label for="' + review.Description + '"/>' + review.Description + '</td>');
        tr.append('<td><label for="' + review.CreatedDate + '"/>' + review.CreatedDate + '</td>');
        tr.append('<td><label for="' + review.UserRating + '"/>' + review.UserRating + '</td>');
        tr.append('<td><label for="' + review.TypeOfReviewValue + '"/>' + review.TypeOfReviewValue + '</td>');
        tr.append('<td><label for="' + review.Likes + '"/>' + review.Likes + '</td>');
        tr.append('<td><label for="' + review.DisLiked + '"/>' + review.DisLikes + '</td>');
        tr.append('<td><a href="/User/DifferentUser?CreatorUserId=' + review.CreatorUserID + '">' + review.CreatedByName + '</a></td>');
        tr.append('<td><a href="/Review/ShowReview?ReviewId=' + review.ReviewID + '"><i class="btn btn-info btn-xs glyphicon glyphicon-eye-open"></i></a></td>');
        body.append(tr);

    })
    //body.addClass("table table-bordered table-hover");
    body.addClass("table table-bordred table-striped");
}

function sortReviews(value) {
    $.ajax({
        url: '/Review/SortReviews',
        type: 'POST',
        dataType: 'json',
        data: { sortValue: value },
        success: function (data) {
            var table = $('#reviews-table');
            var body = $('#review-table-data');

            body.html('');
            $.each(data, function (index, review) {
                var tr = $('<tr></tr>')

                tr.append('<td><label for="' + review.Title + '"/>' + review.Title + '</td>');
                tr.append('<td><label for="' + review.Description + '"/>' + review.Description + '</td>');
                tr.append('<td><label for="' + review.CreatedDate + '"/>' + review.CreatedDate + '</td>');
                tr.append('<td><label for="' + review.UserRating + '"/>' + review.UserRating + '</td>');
                tr.append('<td><label for="' + review.TypeOfReviewValue + '"/>' + review.TypeOfReviewValue + '</td>');
                tr.append('<td><label for="' + review.Likes + '"/>' + review.Likes + '</td>');
                tr.append('<td><label for="' + review.DisLiked + '"/>' + review.DisLikes + '</td>');
                tr.append('<td><a href="/User/DifferentUser?CreatorUserId=' + review.CreatorUserID + '">' + review.CreatedByName + '</a></td>');
                tr.append('<td><a href="/Review/ShowReview?ReviewId=' + review.ReviewID + '"><i class="btn btn-info btn-xs glyphicon glyphicon-eye-open"></i></a></td>');
                body.append(tr);

            })
            //body.addClass("table table-bordered table-hover");
            body.addClass("table table-bordred table-striped");

        }
    });
}

function rateReview(radioValue, reviewId) {
    $.ajax({
        url: '/Review/RateReview',
        type: 'POST',
        dataType: 'json',
        data: { radioValue: radioValue, reviewId: reviewId },
        success: function (data) {
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

            if (data.succeeded > 0) {
                Command: toastr["success"]("Success", "You have rated this review!")
                $('#rating').text(data.rating);
            }
            else {
                Command: toastr["warning"]("Error", "You already rated this review.")
            }
        }
    })
}

function addCommentToReview(commentText, reviewId) {
    $.ajax({
        url: '/Review/CreateCommentToReview',
        type: 'POST',
        traditional: true,
        dataType: 'json',
        data: { CommentToAdd: commentText, ReviewId: reviewId },
        success: function (allReviewViewModel) {
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

            if (allReviewViewModel != null) {
                $('textarea#txt-area-comment').val('');

                $('#comment-wrapper').append('<p>' + allReviewViewModel.CreatedByName + ': ' + allReviewViewModel.CommentToAdd + ', ' + allReviewViewModel.CreatedDate + '</p>')

                Command: toastr["success"]("Success", "Your comment is shared!")
            }
            else {
                Command: toastr["error"]("Error", "You must type more than 3 and less than 150 characters.")
            }
        }
    });
}

function likeOrDislikeReviewFunction(likeOrDislike, likeOrDislikeValue) {
    $.ajax({
        url: '/Review/LikeOrDislikeReview',
        type: 'POST',
        traditional: true,
        dataType: 'json',
        data: { likeOrDislike: likeOrDislike, likeOrDislikeValue: likeOrDislikeValue },
        success: function (succeeded) {
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

            if (succeeded == 2) {
                Command: toastr["error"]("Dislike", "You don't like this review.");
            }
            else if (succeeded == 1) {
                Command: toastr["success"]("Like", "You like this review!");
            }
            else {
                Command: toastr["warning"]("Error", "Sorry, you already liked/disliked this review!");
            }
        }
    });
}

function reviewDeleteFunction() {
    var r = confirm("Do you really want to remove this/these review(s)?");
    if (r == true) {
        var selectedReview = new Array();
        $('.selected-review-items:checked').each(function () {
            selectedReview.push($(this));
        });

        RemoveSelectedReviews(selectedReview);
    }
}

function RemoveSelectedReviews(selectedReview) {
    var array = [];
    $.each(selectedReview, function (index, review) {
        array.push($(review).val());
    })

    $.ajax({
        url: '/Review/DeleteSelectedReviews',
        type: 'POST',
        traditional: true,
        dataType: 'json',
        data: { selectedReviewsIDs: array },
        success: function (succeeded) {
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

            if (succeeded != true) {
                Command: toastr["error"]("Error", "You must choose a review before deleting!")
            }
            else {
                console.log(selectedReview);
                $.each(selectedReview, function (index, reviewToRemove) {
                    $(reviewToRemove).parent().parent().remove();
                })

                Command: toastr["success"]("Deleted", "Delete was a success!")
            }
        }
    });
}
