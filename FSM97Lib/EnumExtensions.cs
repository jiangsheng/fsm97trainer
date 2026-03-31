using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;

namespace FSM97Lib
{
    public static class EnumExtensions
    {
        private static readonly ResourceManager _resourceManager =
        new ResourceManager("FSM97Lib.EnumResources", typeof(EnumResources).Assembly);

        public static string ToLocalizedString(this Enum value, CultureInfo culture = null)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if(culture==null)
                culture = CultureInfo.CurrentUICulture;

            string key = $"{value.GetType().Name}_{value}";
            string localized = _resourceManager.GetString(key, culture);

            return string.IsNullOrEmpty(localized) ? value.ToString() : localized;
        }
    }
}
