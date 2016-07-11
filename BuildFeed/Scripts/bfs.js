var BuildFeed;
(function (BuildFeed) {
    function DropdownClick(ev) {
        ev.preventDefault();
        var link = this;
        link.parentElement.classList.toggle("open");
    }
    BuildFeed.DropdownClick = DropdownClick;
    function SwitchLanguage(ev) {
        ev.preventDefault();
        var link = this;
        document.cookie = "lang=" + link.dataset["lang"] + "; expires=Fri, 31 Dec 9999 23:59:59 GMT; path=/";
        location.reload(true);
    }
    BuildFeed.SwitchLanguage = SwitchLanguage;
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
        var ddLangs = document.getElementById("settings-lang-menu").getElementsByTagName("a");
        for (var i = 0; i < ddLangs.length; i++) {
            ddLangs[i].addEventListener("click", SwitchLanguage);
        }
    }
    BuildFeed.BuildFeedSetup = BuildFeedSetup;
})(BuildFeed || (BuildFeed = {}));
window.addEventListener("load", BuildFeed.BuildFeedSetup);
//# sourceMappingURL=bfs.js.map