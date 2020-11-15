using Microsoft.AspNetCore.Components;
using System;

namespace Localist.Client.Components
{
    public class RelativeTimeBase : ComponentBase
    {
        [Parameter]
        public DateTimeOffset? InputDate { get; init; }

        public string RenderDate()
        {
            if (InputDate is null) return "n/a";

            var elapsed = DateTimeOffset.Now.Subtract(InputDate.Value);

            if (elapsed.TotalMinutes < 1)
            {
                return "just now";
            }
            else if (elapsed.TotalMinutes < 60)
            {
                return RelativeTime(elapsed.ToString("%m"), "minute");
            }
            else if (elapsed.TotalHours < 24)
            {
                return RelativeTime(elapsed.ToString("%h"), "hour");
            }
            else if (elapsed.TotalDays < 30)
            {
                return RelativeTime(elapsed.ToString("%d"), "day");
            }
            else if (elapsed.TotalDays < 365)
            {
                return RelativeTime(Math.Round(elapsed.TotalDays / 30).ToString(), "month");
            }
            else
            {
                return RelativeTime(Math.Round(elapsed.TotalDays / 365).ToString(), "year");
            }
        }

        static string RelativeTime(string quantity, string unit) =>
            $"{quantity} {unit}" + (quantity == "1" ? " ago" : "s ago");
    }
}
