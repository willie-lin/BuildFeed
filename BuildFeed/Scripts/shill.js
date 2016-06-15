$(function() {
   $(".shill-eu-close")
      .click(function(event) {
         event.preventDefault();
         $(".shill-eu").hide();
         document.cookie = "no_shill=true; expires=Fri, 31 Dec 9999 23:59:59 GMT; path=/";
      });
})