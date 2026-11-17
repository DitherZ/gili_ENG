using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace Gili
{
    class WeatherEntry
    {
        public DateTime Local;
        public double TempC;
        public int Humidity, Code;
        public string Desc;
        public bool IsRain
        {
            get { return (Code >= 51 && Code <= 67) || (Code >= 80 && Code <= 82) || Code >= 95; }
        }
    }

    class WeatherReport
    {
        public string Place = "";
        public List<WeatherEntry> Entries = new List<WeatherEntry>();
        public DateTime FetchedUtc;

        public WeatherEntry Current(DateTime localNow)
        {
            WeatherEntry best = null;
            double bestDist = double.MaxValue;
            foreach (var entry in Entries)
            {
                double distance = Math.Abs((localNow - entry.Local).TotalMinutes);
                if (distance < bestDist) { bestDist = distance; best = entry; }
            }
            return best;
        }

        public WeatherEntry NextRain(DateTime localNow, double hours)
        {
            foreach (var entry in Entries)
                if (entry.IsRain && entry.Local > localNow.AddMinutes(-30) && entry.Local <= localNow.AddHours(hours))
                    return entry;
            return null;
        }
    }

    /// <summary>Global weather forecasts from Open-Meteo, queried directly by latitude and longitude.</summary>
    static class WeatherService
    {
        const string UserAgent = "Gili/1.0 (Windows dynamic island)";

        public static WeatherReport Fetch(double latitude, double longitude, string place)
        {
            string url = string.Format(CultureInfo.InvariantCulture,
                "https://api.open-meteo.com/v1/forecast?latitude={0:0.####}&longitude={1:0.####}&current=temperature_2m,relative_humidity_2m,weather_code&hourly=temperature_2m,relative_humidity_2m,weather_code,precipitation_probability&forecast_days=2&timezone=auto",
                latitude, longitude);
            var root = Json(url) as Dictionary<string, object>;
            if (root == null) return null;
            var hourly = root.ContainsKey("hourly") ? root["hourly"] as Dictionary<string, object> : null;
            if (hourly == null) return null;

            var times = Array(hourly, "time");
            var temps = Array(hourly, "temperature_2m");
            var humidity = Array(hourly, "relative_humidity_2m");
            var codes = Array(hourly, "weather_code");
            int count = Math.Min(times.Length, Math.Min(temps.Length, Math.Min(humidity.Length, codes.Length)));
            var report = new WeatherReport { Place = place ?? "", FetchedUtc = DateTime.UtcNow };
            for (int i = 0; i < count; i++)
            {
                DateTime local;
                if (!DateTime.TryParse(Convert.ToString(times[i], CultureInfo.InvariantCulture),
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out local)) continue;
                report.Entries.Add(new WeatherEntry {
                    Local = local,
                    TempC = Number(temps[i]),
                    Humidity = (int)Math.Round(Number(humidity[i])),
                    Code = (int)Math.Round(Number(codes[i])),
                    Desc = Description((int)Math.Round(Number(codes[i])))
                });
            }
            return report.Entries.Count > 0 ? report : null;
        }

        static object[] Array(Dictionary<string, object> data, string key)
        {
            object value;
            return data.TryGetValue(key, out value) ? value as object[] ?? new object[0] : new object[0];
        }

        static double Number(object value)
        {
            try { return Convert.ToDouble(value, CultureInfo.InvariantCulture); }
            catch { return 0; }
        }

        static string Description(int code)
        {
            if (code == 0) return "Clear";
            if (code <= 3) return "Partly cloudy";
            if (code == 45 || code == 48) return "Fog";
            if (code >= 51 && code <= 55) return "Drizzle";
            if (code >= 56 && code <= 57) return "Freezing drizzle";
            if (code >= 61 && code <= 65) return "Rain";
            if (code >= 66 && code <= 67) return "Freezing rain";
            if (code >= 71 && code <= 77) return "Snow";
            if (code >= 80 && code <= 82) return "Rain showers";
            if (code >= 85 && code <= 86) return "Snow showers";
            if (code >= 95) return "Thunderstorm";
            return "Cloudy";
        }

        static object Json(string url)
        {
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)(3072 | 12288); }
            catch { }
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 12000;
            request.ReadWriteTimeout = 12000;
            request.UserAgent = UserAgent;
            request.AllowAutoRedirect = true;
            using (var response = request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                return new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 }.DeserializeObject(reader.ReadToEnd());
        }
    }

    static class WeatherArt
    {
        static readonly Color SunColor = Color.FromArgb(255, 255, 196, 61);
        static readonly Color RainColor = Color.FromArgb(255, 96, 176, 255);

        /// <summary>Small vector weather icon for a WMO weather code, centred on (cx, cy).</summary>
        public static void Draw(Painter p, int code, float cx, float cy, float size, bool night)
        {
            float s = size / 20f;
            bool partly = code == 1 || code == 2;
            bool cloudy = code == 3 || code == 45 || code == 48 || code >= 51;
            bool rain = (code >= 51 && code <= 67) || (code >= 80 && code <= 82), storm = code >= 95, fog = code == 45 || code == 48;

            if (fog)
            {
                for (int i = 0; i < 3; i++)
                    p.G.DrawLine(p.P(Theme.Sub, 1.8f * s), cx - (8 - i * 2) * s, cy + (i * 5 - 5) * s, cx + (8 - i) * s, cy + (i * 5 - 5) * s);
                return;
            }
            if (!cloudy || partly)
            {
                float ox = partly ? cx - 3.5f * s : cx, oy = partly ? cy - 3.5f * s : cy, r = partly ? 4.5f * s : 5.5f * s;
                if (night) DrawMoon(p, ox, oy, r * 1.25f);
                else
                {
                    p.FillCircle(ox, oy, r, SunColor);
                    if (!partly)
                        for (int i = 0; i < 8; i++)
                        {
                            double a = i * Math.PI / 4;
                            p.G.DrawLine(p.P(SunColor, 1.6f * s), ox + (float)Math.Cos(a) * r * 1.45f, oy + (float)Math.Sin(a) * r * 1.45f,
                                         ox + (float)Math.Cos(a) * r * 1.85f, oy + (float)Math.Sin(a) * r * 1.85f);
                        }
                }
                if (!partly) return;
            }
            float cyCloud = rain || storm ? cy - 2.5f * s : cy + (partly ? 2 * s : 0);
            DrawCloud(p, cx + (partly ? 2 * s : 0), cyCloud, s);
            if (rain)
                for (int i = 0; i < 3; i++)
                {
                    float x = cx + (i - 1) * 4.5f * s;
                    p.G.DrawLine(p.P(RainColor, 1.6f * s), x + 1 * s, cy + 5 * s, x - 1 * s, cy + 9 * s);
                }
            if (storm)
            {
                using (var bolt = new GraphicsPath())
                {
                    bolt.AddPolygon(new[] {
                        new PointF(cx + 1 * s, cy + 2 * s), new PointF(cx - 2.5f * s, cy + 7.5f * s), new PointF(cx, cy + 7.5f * s),
                        new PointF(cx - 1.5f * s, cy + 11 * s), new PointF(cx + 3 * s, cy + 5.5f * s), new PointF(cx + 0.5f * s, cy + 5.5f * s) });
                    p.G.FillPath(p.B(SunColor), bolt);
                }
            }
        }

        static void DrawCloud(Painter p, float cx, float cy, float s)
        {
            using (var path = new GraphicsPath(FillMode.Winding))
            {
                path.AddEllipse(cx - 8 * s, cy - 1 * s, 8 * s, 7 * s);
                path.AddEllipse(cx - 4.5f * s, cy - 6 * s, 9.5f * s, 9.5f * s);
                path.AddEllipse(cx + 1 * s, cy - 2.5f * s, 7.5f * s, 8.5f * s);
                path.AddRectangle(new RectangleF(cx - 4 * s, cy + 1.5f * s, 8.5f * s, 4.5f * s));
                p.G.FillPath(p.B(Theme.WithAlpha(Theme.Text, 0.9f)), path);
            }
        }

        static void DrawMoon(Painter p, float cx, float cy, float r)
        {
            using (var moon = new GraphicsPath())
            using (var bite = new GraphicsPath())
            {
                moon.AddEllipse(cx - r, cy - r, r * 2, r * 2);
                bite.AddEllipse(cx - r * 0.35f, cy - r * 1.25f, r * 2, r * 2);
                using (var region = new Region(moon))
                {
                    region.Exclude(bite);
                    p.G.FillRegion(p.B(Color.FromArgb(255, 255, 226, 140)), region);
                }
            }
        }
    }
}