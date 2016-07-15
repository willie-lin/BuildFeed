module BuildFeed
{
   export function DropdownClick(ev: MouseEvent)
   {
      ev.preventDefault();

      const link = this as HTMLAnchorElement;
      link.parentElement.classList.toggle("open");
   }

   export function SwitchTheme(ev: MouseEvent) {
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
      for (let i = 0; i < ddThemes.length; i++) {
         ddThemes[i].addEventListener("click", SwitchTheme);
      }

      const ddLangs = document.getElementById("settings-lang-menu").getElementsByTagName("a");
      for (let i = 0; i < ddLangs.length; i++)
      {
         ddLangs[i].addEventListener("click", SwitchLanguage);
      }
   }
}

window.addEventListener("load", BuildFeed.BuildFeedSetup);