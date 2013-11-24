﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SmartRoutes.Model.Odjfs;
using SmartRoutes.Model.Odjfs.ChildCares;

namespace SmartRoutes.OdjfsScraper.Test.Parsers.Support
{
    public class ChildCareTemplate<T> where T : ChildCare
    {
        private static readonly ICollection<KeyValuePair<string, Func<T, string>>> DefaultDetails = new Dictionary<string, Func<T, string>>
        {
            {"Number", c => c.ExternalId},
            {"Name", c => c.Name},
            {"Address", c => c.Address},
            {"City", c => c.City},
            {"State", c => c.State},
            {"Zip", c => Convert.ToString(c.ZipCode)},
            {"County", c => c.County.Name},
            {"Phone", c => c.PhoneNumber}
        };

        public ChildCareTemplate() : this(Enumerable.Empty<KeyValuePair<string, Func<T, string>>>())
        {
        }

        protected ChildCareTemplate(IEnumerable<KeyValuePair<string, Func<T, string>>> parentPageContentDetails)
        {
            // intialize properties
            Details = new Collection<KeyValuePair<string, Func<T, string>>>();

            // initialize the value getters
            foreach (var pair in DefaultDetails.Concat(parentPageContentDetails))
            {
                Details.Add(pair);
            }

            // set default properties on the model
            Model = Activator.CreateInstance<T>();
            Model.ExternalId = "ExternalId";
            Model.Name = "Name";
            Model.City = "City";
            Model.State = "State";
            Model.ZipCode = 99999;
            Model.County = new County {Name = "County"};
            Model.PhoneNumber = "PhoneNumber";
        }

        public ICollection<KeyValuePair<string, Func<T, string>>> Details { get; private set; }

        public void AddDetail(string key, Func<T, string> value)
        {
            Details.Add(new KeyValuePair<string, Func<T, string>>(key, value));
        }

        public void RemoveDetails(string key)
        {
            KeyValuePair<string, Func<T, string>> pair;
            do
            {
                pair = Details.FirstOrDefault(p => p.Key == key);
                Details.Remove(pair);
            } while (!pair.Equals(default(KeyValuePair<string, Func<T, string>>)));
        }

        public void ReplaceDetails(string key, Func<T, string> value)
        {
            RemoveDetails(key);
            AddDetail(key, value);
        }

        public T Model { get; private set; }

        public static byte[] GetBytes(string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public virtual byte[] GetDocument()
        {
            // fetch the values for the details
            IEnumerable<KeyValuePair<string, string>> details = Details.Select(p => new KeyValuePair<string, string>(p.Key, p.Value(Model)));

            var sb = new StringBuilder();
            BuildPageContent(sb, details);
            return GetBytes(sb.ToString());
        }

        protected static void BuildPageContent(StringBuilder sb, IEnumerable<KeyValuePair<string, string>> details)
        {
            KeyValuePair<string, string>[] detailArray = details.ToArray();
            int keyWidth = detailArray.Select(p => p.Key.Length).Max() + 2; // "<key>: "

            sb.AppendLine("<div id='PageContent'>");
            sb.AppendLine("  <table>");
            foreach (var detail in detailArray)
            {
                sb.Append("    <tr><td>");
                sb.Append((detail.Key + ":").PadRight(keyWidth));
                sb.Append(detail.Value);
                sb.Append("</td></tr>");
                sb.AppendLine();
            }
            sb.AppendLine("  </table>");
            sb.AppendLine("</div>");
            sb.AppendLine();
        }
    }
}