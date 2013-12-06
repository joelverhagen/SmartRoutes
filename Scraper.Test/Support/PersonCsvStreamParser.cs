﻿using System.Collections.Generic;

namespace SmartRoutes.Scraper.Test.Support
{
    public class PersonCsvStreamParser : CsvStreamParser<Person>
    {
        protected override Person ConstructItem(IDictionary<string, string> values)
        {
            return new Person
            {
                Name = values["Name"],
                FavoriteColor = values["FavoriteColor"]
            };
        }
    }
}