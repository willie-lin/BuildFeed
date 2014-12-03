$(function () {
    $("#search-input").keyup(function () {
        var search = $(this);
        $(this).parent().find(".list-group").remove();
        $.ajax("/api/GetSearchResult/?query=" + $(this).val()).done(function (data) {
            var template = $.templates("#result-template");
            var content = $("<div class='list-group'></div>");

            var item = template.render(data);
            content.append(item);

            search.after(content);
        });
    });
});