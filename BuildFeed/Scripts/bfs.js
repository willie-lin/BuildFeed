var BuildFeed;
(function (BuildFeed) {
    function DropdownClick(ev) {
        ev.preventDefault();
        var link = this;
        var menus = link.parentElement.getElementsByClassName("dropdown-menu");
        if (menus.length > 0) {
            menus[0].classList.toggle("open");
        }
    }
    BuildFeed.DropdownClick = DropdownClick;
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
    }
    BuildFeed.BuildFeedSetup = BuildFeedSetup;
})(BuildFeed || (BuildFeed = {}));
window.addEventListener("load", BuildFeed.BuildFeedSetup);
//# sourceMappingURL=bfs.js.map