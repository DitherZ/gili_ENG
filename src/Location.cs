using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Gili
{
    /// <summary>Finds approximate coordinates from the public IP and caches the result in settings.</summary>
    static class LocationService
    {
        public class Result { public double Lat, Lon; public string City; }

        public static Result Detect()
        {
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)(3072 | 12288); } catch { } // TLS 1.2 and TLS 1.3
            var r = Fetch("https://ipwho.is/?fields=success,city,latitude,longitude", "latitude", "longitude", "city");
            if (r == null) r = Fetch("http://ip-api.com/json/?fields=status,city,lat,lon", "lat", "lon", "city");
            return r;
        }

        static Result Fetch(string url, string latKey, string lonKey, string cityKey)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = 8000;
                req.ReadWriteTimeout = 8000;
                req.UserAgent = "Gili/1.0";
                string json;
                using (var resp = req.GetResponse())
                using (var sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    json = sr.ReadToEnd();
                double lat, lon;
                if (!Number(json, latKey, out lat) || !Number(json, lonKey, out lon)) return null;
                return new Result { Lat = lat, Lon = lon, City = Text(json, cityKey) ?? "" };
            }
            catch (Exception ex)
            {
                Log.Write("location " + url + ": " + ex.Message);
                return null;
            }
        }

        static bool Number(string json, string key, out double value)
        {
            var m = Regex.Match(json, "\"" + key + "\"\\s*:\\s*(-?[0-9]+(?:\\.[0-9]+)?)");
            value = 0;
            return m.Success && double.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        static string Text(string json, string key)
        {
            var m = Regex.Match(json, "\"" + key + "\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"");
            return m.Success ? Regex.Unescape(m.Groups[1].Value) : null;
        }
    }
}