module BuildFeed
{
   export function DropdownClick(ev: MouseEvent)
   {
      ev.preventDefault();

      var link = this as HTMLAnchorElement;
      var menus = link.parentElement.getElementsByClassName("dropdown-menu");

      if (menus.length > 0)
      {
         menus[0].classList.toggle("open");
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
   }
}

window.addEventListener("load", BuildFeed.BuildFeedSetup);