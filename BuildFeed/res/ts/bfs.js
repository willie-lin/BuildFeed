var BuildFeed;
(function (BuildFeed) {
    let ajax;
    let timeout;
    function MobileMenuToggle(ev) {
        ev.preventDefault();
        const button = this;
        button.nextElementSibling.classList.toggle("open");
    }
    BuildFeed.MobileMenuToggle = MobileMenuToggle;
    function DropdownClick(ev) {
        ev.preventDefault();
        const link = this;
        link.parentElement.classList.toggle("open");
    }
    BuildFeed.DropdownClick = DropdownClick;
    function SwitchTheme(ev) {
        ev.preventDefault();
        const link = this;
        document.cookie = `bf_theme=${link.dataset["theme"]}; expires=Fri, 31 Dec 9999 23:59:59 GMT; path=/`;
        location.reload(true);
    }
    BuildFeed.SwitchTheme = SwitchTheme;
    function SwitchLanguage(ev) {
        ev.preventDefault();
        const link = this;
        document.cookie = `bf_lang=${link.dataset["lang"]}; expires=Fri, 31 Dec 9999 23:59:59 GMT; path=/`;
        location.reload(true);
    }
    BuildFeed.SwitchLanguage = SwitchLanguage;
    function OpenSearch(ev) {
        ev.preventDefault();
        const modal = document.getElementById("modal-search-overlay");
        modal.classList.add("open");
    }
    BuildFeed.OpenSearch = OpenSearch;
    function CloseSearch(ev) {
        ev.preventDefault();
        const modal = document.getElementById("modal-search-overlay");
        modal.classList.remove("open");
    }
    BuildFeed.CloseSearch = CloseSearch;
    function StopClick(ev) {
        ev.preventDefault();
        ev.stopPropagation();
    }
    BuildFeed.StopClick = StopClick;
    function InitiateSearch(ev) {
        const resultPane = document.getElementById("modal-search-result");
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
        const modalInput = document.getElementById("modal-search-input");
        ajax = new XMLHttpRequest();
        ajax.onreadystatechange = CompleteSearch;
        ajax.open("GET", `/api/GetSearchResult/${modalInput.value}/`, true);
        ajax.setRequestHeader("accept", "application/json");
        ajax.send(null);
    }
    BuildFeed.SendSearch = SendSearch;
    function CompleteSearch(ev) {
        if (ajax.readyState !== XMLHttpRequest.DONE || ajax.status !== 200) {
            return;
        }
        const resultPane = document.getElementById("modal-search-result");
        const templateContent = document.getElementById("result-template");
        const template = jsrender.templates(templateContent.innerHTML);
        const content = template.render(JSON.parse(ajax.responseText));
        resultPane.innerHTML = content;
        const resultLinks = resultPane.getElementsByTagName("a");
        for (let i = 0; i < resultLinks.length; i++) {
            resultLinks[i].addEventListener("click", (mev) => {
                mev.preventDefault();
                const modalInput = document.getElementById("modal-search-input");
                ga("send", "pageview", `/api/GetSearchResult/${modalInput.value}/`);
                location.assign(mev.currentTarget.href);
            });
        }
    }
    BuildFeed.CompleteSearch = CompleteSearch;
    function BuildFeedSetup(ev) {
        const ddParents = document.getElementsByClassName("dropdown-parent");
        for (let i = 0; i < ddParents.length; i++) {
            for (let j = 0; j < ddParents[i].childNodes.length; j++) {
                const el = ddParents[i].childNodes[j];
                if (el.nodeName === "A") {
                    el.addEventListener("click", DropdownClick);
                }
            }
        }
        const ddThemes = document.getElementById("settings-theme-menu").getElementsByTagName("a");
        for (let i = 0; i < ddThemes.length; i++) {
            ddThemes[i].addEventListener("click", SwitchTheme);
        }
        const ddLangs = document.getElementById("settings-lang-menu").getElementsByTagName("a");
        for (let i = 0; i < ddLangs.length; i++) {
            ddLangs[i].addEventListener("click", SwitchLanguage);
        }
        const btnNav = document.getElementById("page-navigation-toggle");
        btnNav.addEventListener("click", MobileMenuToggle);
        const btnSearch = document.getElementById("page-navigation-search");
        btnSearch.addEventListener("click", OpenSearch);
        const modalOverlay = document.getElementById("modal-search-overlay");
        modalOverlay.addEventListener("click", CloseSearch);
        const modalDialog = document.getElementById("modal-search");
        modalDialog.addEventListener("click", StopClick);
        const modalInput = document.getElementById("modal-search-input");
        modalInput.addEventListener("keyup", InitiateSearch);
    }
    BuildFeed.BuildFeedSetup = BuildFeedSetup;
})(BuildFeed || (BuildFeed = {}));
window.addEventListener("load", BuildFeed.BuildFeedSetup);
//# sourceMappingURL=bfs.js.map