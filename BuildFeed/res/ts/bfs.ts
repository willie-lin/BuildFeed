/// <reference path="../../scripts/typings/google.analytics/ga.d.ts" />
/// <reference path="../../scripts/typings/jsrender/jsrender.d.ts" />

module BuildFeed
{
   let ajax: XMLHttpRequest;
   let timeout: number;

   export function MobileMenuToggle(ev: MouseEvent)
   {
      ev.preventDefault();

      const button = this as HTMLButtonElement;
      button.nextElementSibling.classList.toggle("open");
   }

   export function DropdownClick(ev: MouseEvent)
   {
      ev.preventDefault();

      const link = this as HTMLAnchorElement;
      link.parentElement.classList.toggle("open");
   }

   export function SwitchTheme(ev: MouseEvent)
   {
      ev.preventDefault();

      const link = this as HTMLAnchorElement;
      document.cookie = `bf_theme=${link.dataset["theme"]}; expires=Fri, 31 Dec 9999 23:59:59 GMT; path=/`;
      location.reload(true);
   }

   export function SwitchLanguage(ev: MouseEvent)
   {
      ev.preventDefault();

      const link = this as HTMLAnchorElement;
      document.cookie = `bf_lang=${link.dataset["lang"]}; expires=Fri, 31 Dec 9999 23:59:59 GMT; path=/`;
      location.reload(true);
   }

   export function OpenSearch(ev: MouseEvent)
   {
      ev.preventDefault();

      const modal = document.getElementById("modal-search-overlay") as HTMLDivElement;
      modal.classList.add("open");
   }

   export function CloseSearch(ev: MouseEvent)
   {
      ev.preventDefault();

      const modal = document.getElementById("modal-search-overlay") as HTMLDivElement;
      modal.classList.remove("open");
   }

   export function StopClick(ev: MouseEvent)
   {
      ev.preventDefault();
      ev.stopPropagation();
   }

   export function InitiateSearch(ev: KeyboardEvent)
   {
      const resultPane = document.getElementById("modal-search-result") as HTMLDivElement;
      resultPane.innerHTML = "";

      if (typeof (timeout) !== "undefined")
      {
         clearTimeout(timeout);
      }

      if (typeof (ajax) !== "undefined" && ajax.readyState !== XMLHttpRequest.DONE)
      {
         ajax.abort();
      }

      timeout = setInterval(SendSearch, 200);
   }

   export function SendSearch()
   {
      if (typeof (timeout) !== "undefined")
      {
         clearTimeout(timeout);
      }

      const modalInput = document.getElementById("modal-search-input") as HTMLInputElement;

      ajax = new XMLHttpRequest();
      ajax.onreadystatechange = CompleteSearch;
      ajax.open("GET", `/api/GetSearchResult/${modalInput.value}/`, true);
      ajax.setRequestHeader("accept", "application/json");
      ajax.send(null);
   }

   export function CompleteSearch(ev: ProgressEvent)
   {
      if (ajax.readyState !== XMLHttpRequest.DONE || ajax.status !== 200)
      {
         return;
      }

      const resultPane = document.getElementById("modal-search-result") as HTMLDivElement;
      const templateContent = document.getElementById("result-template") as HTMLDivElement;
      const template = jsrender.templates(templateContent.innerHTML);
      const content = template.render(JSON.parse(ajax.responseText));
      resultPane.innerHTML = content;

      const resultLinks = resultPane.getElementsByTagName("a");
      for (let i = 0; i < resultLinks.length; i++)
      {
         resultLinks[i].addEventListener("click", (mev: MouseEvent) =>
         {
            mev.preventDefault();
            const modalInput = document.getElementById("modal-search-input") as HTMLInputElement;
            ga("send", "pageview", `/api/GetSearchResult/${modalInput.value}/`);
            location.assign((mev.currentTarget as HTMLAnchorElement).href);
         });
      }
   }

   export function BuildFeedSetup(ev: Event)
   {
      const ddParents = document.getElementsByClassName("dropdown-parent");
      for (let i = 0; i < ddParents.length; i++)
      {
         for (let j = 0; j < ddParents[i].childNodes.length; j++)
         {
            const el = ddParents[i].childNodes[j];

            if (el.nodeName === "A")
            {
               el.addEventListener("click", DropdownClick);
            }
         }
      }

      const ddThemes = document.getElementById("settings-theme-menu").getElementsByTagName("a");
      for (let i = 0; i < ddThemes.length; i++)
      {
         ddThemes[i].addEventListener("click", SwitchTheme);
      }

      const ddLangs = document.getElementById("settings-lang-menu").getElementsByTagName("a");
      for (let i = 0; i < ddLangs.length; i++)
      {
         ddLangs[i].addEventListener("click", SwitchLanguage);
      }

      const btnNav = document.getElementById("page-navigation-toggle");
      btnNav.addEventListener("click", MobileMenuToggle);

      const btnSearch = document.getElementById("page-navigation-search");
      btnSearch.addEventListener("click", OpenSearch);

      const modalOverlay = document.getElementById("modal-search-overlay") as HTMLDivElement;
      modalOverlay.addEventListener("click", CloseSearch);

      const modalDialog = document.getElementById("modal-search") as HTMLDivElement;
      modalDialog.addEventListener("click", StopClick);

      const modalInput = document.getElementById("modal-search-input") as HTMLInputElement;
      modalInput.addEventListener("keyup", InitiateSearch);
   }
}

window.addEventListener("load", BuildFeed.BuildFeedSetup);