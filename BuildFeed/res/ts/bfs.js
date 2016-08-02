/// <reference path="../../scripts/typings/google.analytics/ga.d.ts" />
/// <reference path="../../scripts/typings/jsrender/jsrender.d.ts" />
var BuildFeed;
(function (BuildFeed) {
    var ajax;
    var timeout;
    function MobileMenuToggle(ev) {
        ev.preventDefault();
        var button = this;
        button.nextElementSibling.classList.toggle("open");
    }
    BuildFeed.MobileMenuToggle = MobileMenuToggle;
    function DropdownClick(ev) {
        ev.preventDefault();
        var link = this;
        link.parentElement.classList.toggle("open");
    }
    BuildFeed.DropdownClick = DropdownClick;
    function SwitchTheme(ev) {
        ev.preventDefault();
        var link = this;
        document.cookie = "bf_theme=" + link.dataset["theme"] + "; expires=Fri, 31 Dec 9999 23:59:59 GMT; path=/";
        location.reload(true);
    }
    BuildFeed.SwitchTheme = SwitchTheme;
    function SwitchLanguage(ev) {
        ev.preventDefault();
        var link = this;
        document.cookie = "bf_lang=" + link.dataset["lang"] + "; expires=Fri, 31 Dec 9999 23:59:59 GMT; path=/";
        location.reload(true);
    }
    BuildFeed.SwitchLanguage = SwitchLanguage;
    function OpenSearch(ev) {
        ev.preventDefault();
        var modal = document.getElementById("modal-search-overlay");
        modal.classList.add("open");
    }
    BuildFeed.OpenSearch = OpenSearch;
    function CloseSearch(ev) {
        ev.preventDefault();
        var modal = document.getElementById("modal-search-overlay");
        modal.classList.remove("open");
    }
    BuildFeed.CloseSearch = CloseSearch;
    function StopClick(ev) {
        ev.preventDefault();
        ev.stopPropagation();
    }
    BuildFeed.StopClick = StopClick;
    function InitiateSearch(ev) {
        var resultPane = document.getElementById("modal-search-result");
        resultPane.innerHTML = "";
        if (typeof (timeout) !== "undefined") {
            clearTimeout(timeout);
        }
        if (typeof (ajax) !== "undefined" && ajax.readyState !== XMLHttpRequest.DONE) {
            ajax.abort();
        }
        timeout = setInterval(SendSearch, 200);
    }
    BuildFeed.InitiateSearch = InitiateSearch;
    function SendSearch() {
        if (typeof (timeout) !== "undefined") {
            clearTimeout(timeout);
        }
        var modalInput = document.getElementById("modal-search-input");
        ajax = new XMLHttpRequest();
        ajax.onreadystatechange = CompleteSearch;
        ajax.open("GET", "/api/GetSearchResult/" + modalInput.value + "/", true);
        ajax.send(null);
    }
    BuildFeed.SendSearch = SendSearch;
    function CompleteSearch(ev) {
        if (ajax.readyState !== XMLHttpRequest.DONE || ajax.status !== 200) {
            return;
        }
        var resultPane = document.getElementById("modal-search-result");
        var templateContent = document.getElementById("result-template");
        var template = jsrender.templates(templateContent.innerHTML);
        var content = template.render(JSON.parse(ajax.responseText));
        resultPane.innerHTML = content;
        var resultLinks = resultPane.getElementsByTagName("a");
        for (var i = 0; i < resultLinks.length; i++) {
            resultLinks[i].addEventListener("click", function () {
                var modalInput = document.getElementById("modal-search-input");
                ga("send", "pageview", "/api/GetSearchResult/" + modalInput.value + "/");
            });
        }
    }
    BuildFeed.CompleteSearch = CompleteSearch;
    function BuildFeedSetup(ev) {
        var ddParents = document.getElementsByClassName("dropdown-parent");
        for (var i = 0; i < ddParents.length; i++) {
            for (var j = 0; j < ddParents[i].childNodes.length; j++) {
                var el = ddParents[i].childNodes[j];
                if (el.nodeName === "A") {
                    el.addEventListener("click", DropdownClick);
                }
            }
        }
        var ddThemes = document.getElementById("settings-theme-menu").getElementsByTagName("a");
        for (var i = 0; i < ddThemes.length; i++) {
            ddThemes[i].addEventListener("click", SwitchTheme);
        }
        var ddLangs = document.getElementById("settings-lang-menu").getElementsByTagName("a");
        for (var i = 0; i < ddLangs.length; i++) {
            ddLangs[i].addEventListener("click", SwitchLanguage);
        }
        var btnNav = document.getElementById("page-navigation-toggle");
        btnNav.addEventListener("click", MobileMenuToggle);
        var btnSearch = document.getElementById("page-navigation-search");
        btnSearch.addEventListener("click", OpenSearch);
        var modalOverlay = document.getElementById("modal-search-overlay");
        modalOverlay.addEventListener("click", CloseSearch);
        var modalDialog = document.getElementById("modal-search");
        modalDialog.addEventListener("click", StopClick);
        var modalInput = document.getElementById("modal-search-input");
        modalInput.addEventListener("keyup", InitiateSearch);
    }
    BuildFeed.BuildFeedSetup = BuildFeedSetup;
})(BuildFeed || (BuildFeed = {}));
window.addEventListener("load", BuildFeed.BuildFeedSetup);
//# sourceMappingURL=bfs.js.map