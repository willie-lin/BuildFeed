using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BuildFeed.Code
{
   public static class MvcExtensions
   {
      public static IHtmlString CheckboxListForEnum<T>(this HtmlHelper html, string id, T currentItem) where T : struct
      {
         StringBuilder sb = new StringBuilder();

         foreach (T enumItem in Enum.GetValues(typeof(T)).Cast<T>())
         {
            long enumValue = Convert.ToInt64(enumItem);
            long currentValue = Convert.ToInt64(currentItem);

            if (enumValue == 0)
            {
               // skip 0-valued bitflags, they're for display only.
               continue;
            }

            TagBuilder wrapper = new TagBuilder("div");
            wrapper.Attributes.Add("class", "checkbox");

            TagBuilder label = new TagBuilder("label");

            TagBuilder input = new TagBuilder("input");
            if ((enumValue & currentValue) != 0)
            {
               input.MergeAttribute("checked", "checked");
            }
            input.MergeAttribute("type", "checkbox");
            input.MergeAttribute("value", enumValue.ToString());
            input.MergeAttribute("name", id);

            label.InnerHtml = input.ToString(TagRenderMode.SelfClosing);
            label.InnerHtml += GetDisplayTextForEnum(enumItem);

            wrapper.InnerHtml = label.ToString(TagRenderMode.Normal);

            sb.Append(wrapper.ToString(TagRenderMode.Normal));
         }

         return new HtmlString(sb.ToString());
      }

      public static string GetDisplayTextForEnum(object o)
      {
         string result = null;
         DisplayAttribute display = o.GetType().GetMember(o.ToString()).First().GetCustomAttributes(false).OfType<DisplayAttribute>().LastOrDefault();

         if (display != null)
         {
            result = display.GetName();
         }

         return result ?? o.ToString();
      }

      public static string ToLongDateWithoutDay(this DateTime dt)
      {
         string s = CultureInfo.CurrentUICulture.DateTimeFormat.LongDatePattern;
         s = s.Replace("dddd", "").Replace("ddd", "");
         s = s.Trim(' ', ',');

         return dt.ToString(s);
      }
   }
}