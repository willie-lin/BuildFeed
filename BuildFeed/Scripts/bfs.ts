module BuildFeed
{
   export function DropdownClick(ev: MouseEvent)
   {
      ev.preventDefault();

      const link = this as HTMLAnchorElement;
      link.parentElement.classList.toggle("open");
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