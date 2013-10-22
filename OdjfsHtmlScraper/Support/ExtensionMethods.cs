﻿using System;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CsQuery;

namespace OdjfsHtmlScraper.Support
{
    public static class ExtensionMethods
    {
        public static string GetSha256Hash(this byte[] bytes)
        {
            var sha = new SHA256Managed();
            byte[] hashBytes = sha.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public static string ToNullIfEmpty(this string input)
        {
            return input == string.Empty ? null : input;
        }

        public static string GetCollapsedInnerText(this IDomElement e)
        {
            // get the text content
            string text = e.TextContent;

            // decode the entities
            text = WebUtility.HtmlDecode(text);

            // collapse the whitespace
            return Regex.Replace(text, @"\s+", " ").Trim();
        }
    }
}