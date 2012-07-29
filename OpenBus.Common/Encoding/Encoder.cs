using System;
using System.Linq;
using System.Text;
using System.Web;
using log4net;

namespace OpenBus.Common.Encoding
{
    public static class Encoder
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Encoder));

        /// <summary>
        /// Encodes a normal string to a hexidecimal string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string HexEncode(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                Logger.Error("Encoder: Could not HexEncode null/empty string.");
                return null;
            }

            StringBuilder hex = new StringBuilder();
            foreach (char c in str)
            {
                hex.Append(String.Format("{0:x2}", Convert.ToUInt32(((int)c).ToString())));
            }

            Logger.Debug(String.Format("Encoder: HexEncoded the string: '{0}' to hex: '{1}'.", str, hex));

            return hex.ToString();
        }

        /// <summary>
        /// Decodes a hexidecimal string to a normal string
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static string HexDecode(string hexString)
        {
            if (String.IsNullOrEmpty(hexString))
            {
                Logger.Error("Encoder: Could not HexDecode null/empty string.");
                return null;
            }

            StringBuilder strValue = new StringBuilder();
            while (hexString.Length > 0)
            {
                strValue.Append(Convert.ToChar(Convert.ToUInt32(hexString.Substring(0, 2), 16)).ToString());
                hexString = hexString.Substring(2, hexString.Length - 2);
            }

            Logger.Debug(String.Format("Encoder: HexDecoded the hex: '{0}' to string: '{1}'.", hexString, strValue));

            return strValue.ToString();
        }

        /// <summary>
        /// Html encodes a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string HtmlEncode(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                Logger.Error("Encoder: Could not HtmlEncode null/empty string.");
                return null;
            }

            string html = HttpUtility.HtmlEncode(str);

            Logger.Debug(String.Format("Encoder: HtmlEncoded the string: '{0}' to html: '{1}'.", str, html));

            return html;
        }

        /// <summary>
        /// Html decodes a string
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string HtmlDecode(string html)
        {
            if (String.IsNullOrEmpty(html))
            {
                Logger.Error("Could not HtmlDecode null/empty string.");
                return null;
            }

            string str = HttpUtility.HtmlDecode(html);

            Logger.Debug(String.Format("Encoder: HtmlDecoded the html: '{0}' to string: '{1}'.", html, str));

            return str;
        }

        /// <summary>
        /// Removes all special characters
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveSpecialCharacters(string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                Logger.Error("Encoder: Could not RemoveSpecialCharacters from null/empty string.");
                return null;
            }

            // In xml, all <, >, etc is illegal. This is taken care of by HttpUtility.
            // Furthermore, + is illegal, so we replace that.
            // Furthermore the encode could possible produce some %, which are also illegal.
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char character in str.Where(character => (character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z')))
            {
                stringBuilder.Append(character);
            }

            string cleanString = stringBuilder.ToString();

            Logger.Debug(String.Format("XmlHelper Cleaned the string: '{0}' to string: '{1}'.", str, cleanString));

            return cleanString;
        }

        /// <summary>
        /// Removes the characters that are illegal in an XmlDictionary.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SafeEncode(string str)
        {
            return !String.IsNullOrEmpty(str) ? str
                .Replace("+", "2B")
                .Replace("`", "0x60")
                .Replace("[", "0x5B")
                .Replace(",", "0x2C")
                .Replace(" ", "0x20")
                .Replace("=", "0x3D")
                .Replace("]", "0x5D") : str;
        }

        /// <summary>
        /// Reintroduces the characters that are illegal in an XmlDictionary.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SafeDecode(string str)
        {
            return !String.IsNullOrEmpty(str) ? str
                .Replace("2B", "+")
                .Replace("0x60", "`")
                .Replace("0x5B", "[")
                .Replace("0x2C", ",")
                .Replace("0x20", " ")
                .Replace("0x3D", "=")
                .Replace("0x5D", "]") : str;
        }
    }
}
