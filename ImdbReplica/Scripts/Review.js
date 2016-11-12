$(document).ready(function () {
    $("#tableWrapper").pageMe({ pagerSelector: "#myPager", showPrevNext: true, hidePageNumbers: false, perPage: 5 });

    $("#btnSortReviews").on("click", function () {
        var value = $("#sortReviews option:selected").val();
        sortReviews(value);
    });

    $("#btnSearchReview").on("click", function () {
        var value = $("#tbSearchInput").val();
        searchReview(value);
    });

    $("#tbSearchInput").on("input", function () {
        var value = $("#tbSearchInput").val();
        refreshReviews(value);
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

$.fn.pageMe = function (opts) {
    var $this = this,
        defaults = {
            perPage: 5,
            showPrevNext: false,
            hidePageNumbers: false
        },
        settings = $.extend(defaults, opts);

    var listElement = $this;
    var perPage = settings.perPage;
    var children = listElement.children();
    var pager = $(".pager");

    if (typeof settings.childSelector != "undefined") {
        children = listElement.find(settings.childSelector);
    }

    if (typeof settings.pagerSelector != "undefined") {
        pager = $(settings.pagerSelector);
    }

    var numItems = children.length;
    var numPages = Math.ceil(numItems / perPage);

    pager.data("curr", 0);

    if (settings.showPrevNext) {
        $('<li><a href="#" class="prev_link">«</a></li>').appendTo(pager);
    }

    var curr = 0;
    while (numPages > curr && (settings.hidePageNumbers == false)) {
        $('<li><a href="#" class="page_link">' + (curr + 1) + "</a></li>").appendTo(pager);
        curr++;
    }

    if (settings.showPrevNext) {
        $('<li><a href="#" class="next_link">»</a></li>').appendTo(pager);
    }

    pager.find(".page_link:first").addClass("active");
    pager.find(".prev_link").hide();
    if (numPages <= 1) {
        pager.find(".next_link").hide();
    }
    pager.children().eq(1).addClass("active");

    children.hide();
    children.slice(0, perPage).show();

    pager.find("li .page_link").click(function () {
        var clickedPage = $(this).html().valueOf() - 1;
        goTo(clickedPage, perPage);
        return false;
    });
    pager.find("li .prev_link").click(function () {
        previous();
        return false;
    });
    pager.find("li .next_link").click(function () {
        next();
        return false;
    });

    function previous() {
        var goToPage = parseInt(pager.data("curr")) - 1;
        goTo(goToPage);
    }

    function next() {
        goToPage = parseInt(pager.data("curr")) + 1;
        goTo(goToPage);
    }

    function goTo(page) {
        var startAt = page * perPage,
            endOn = startAt + perPage;

        children.css("display", "none").slice(startAt, endOn).show();

        if (page >= 1) {
            pager.find(".prev_link").show();
        }
        else {
            pager.find(".prev_link").hide();
        }

        if (page < (numPages - 1)) {
            pager.find(".next_link").show();
        }
        else {
            pager.find(".next_link").hide();
        }

        pager.data("curr", page);
        pager.children().removeClass("active");
        pager.children().eq(page + 1).addClass("active");

    }
};


function refreshReviews(value) {
    $.ajax({
        url: "/Review/RefreshReviews",
        type: "POST",
        dataType: "json",
        data: { value: value },
        success: function (data) {
            console.log(data);
            if (data.succeeded == true) {
                printHtml(data.reviews);
            }
            else {
                Command: toastr["error"]("Failed", "Couldn't get the list, try reloading the page.");
            }
        }
    });
}

function sortReviews(value) {
    $.ajax({
        url: "/Review/SortReviews",
        type: "POST",
        dataType: "json",
        data: { sortValue: value },
        success: function (data) {
            if (data.succeeded == true) {
                printHtml(data.reviews);
            }
            else {
                Command: toastr["error"]("Failed", "Couldn't sort the list, try reloading the page");
            }
        }
    });
}

function searchReview(value) {
    $.ajax({
        url: "/Review/SearchReviews",
        type: "POST",
        dataType: "json",
        data: { searchValue: value },
        success: function (data) {
            if (data.succeeded == true) {
                printHtml(data.reviews);
            }
            else {
                Command: toastr["error"]("Failed", "There was no review with that name, try reloading the page");
            }
        }
    });
}

function printHtml(data) {
    var table = $("#allReviewsTable");
    var body = $("#tableWrapper");
    body.html("");
    $.each(data, function (index, review) {
        var tr = $('<tr  style="display: table-row;"></tr>')

        tr.append("<td>" + review.Title + "</td>");
        tr.append("<td>" + review.Description + "</td>");
        tr.append("<td>" + review.CreatedDate + "</td>");
        tr.append("<td>" + review.UserRating + "</td>");
        tr.append("<td>" + review.TotalRating + "</td>");
        tr.append("<td>" + review.Type + "</td>");
        tr.append("<td>" + review.Likes + "</td>");
        tr.append("<td>" + review.DisLikes + "</td>");
        if (review.HasProfilePicture == true) {
            tr.append('<td><img id="all-review-img" src="/Images/ProfileImages/' + review.CreatorUserId + '.png"/>' +
                '<a href="/User/OtherUser?CreatorUserId=' + review.CreatorUserId + '">' + review.CreatedBy + "</a></td>");
        }
        else {
            tr.append('<td><img id="all-review-img" src="/Images/Shared/icon-user-default.png"/>' +
                '<a href="/User/OtherUser?CreatorUserId=' + review.CreatorUserId + '">' + review.CreatedBy + "</a></td>");
        }
        tr.append('<td><a href="/Review/ShowReview?ReviewId=' + review.ReviewId + '"><i class="btn btn-info btn-xs glyphicon glyphicon-eye-open"></i></a></td>');

        body.append(tr);
    });

    $("#myPager").empty();
    $("#tableWrapper").pageMe({ pagerSelector: "#myPager", showPrevNext: true, hidePageNumbers: false, perPage: 5 });
}