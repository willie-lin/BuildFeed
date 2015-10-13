var bfsAjax;
var bfsTimeout;

$(function () {
   $("#page-navigation-search").click(function (event) {
      event.preventDefault();
      $("#search-modal").modal('show');
   });
   $("#search-input").keyup(function () {
      var search = $(this);
      $(this).parent().find(".list-group").remove();

      if (typeof bfsTimeout != 'undefined') {
         clearTimeout(bfsTimeout);
      }

      if (typeof bfsAjax != 'undefined' && bfsAjax.readyState != 4) {
         bfsAjax.abort();
      }

      bfsTimeout = setTimeout(function (object) {
         bfsAjax = $.ajax("/api/GetSearchResult/" + $("#search-input").val() + '/').done(function (data) {
            var template = $.templates("#result-template");
            var content = $("<div class='list-group'></div>");

            var item = template.render(data);
            content.append(item);

            $("#search-results").html(content);

            $("#search-results a.list-group-item").click(function () {
               ga('send', 'pageview', '/api/GetSearchResult/' + $("#search-input").val() + '/');
            });
         });
      }, 200);
   });

   $("#lang-switcher a").click(function (event) {
      event.preventDefault();
      var lang = $(this).data("lang");
      document.cookie = "lang=" + lang + "; expires=Fri, 31 Dec 9999 23:59:59 GMT; path=/";

      location.reload(true);
   });
});