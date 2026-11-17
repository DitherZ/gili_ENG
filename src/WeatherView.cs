using System;
using System.Drawing;
using System.Threading;

namespace Gili
{
    /// <summary>Global weather forecasts and rain alerts for the island.</summary>
    partial class IslandForm
    {
        WeatherReport weather;
        DateTime nextWeatherUtc = DateTime.MinValue;
        bool weatherBusy;
        string rainNotified = "";

        bool WeatherEnabled { get { return settings.GetBool("weather", true); } }

        /// <summary>Refreshes the Open-Meteo forecast hourly and retries sooner after a failure.</summary>
        void MaybeRefreshWeather()
        {
            if (!WeatherEnabled || weatherBusy || DateTime.UtcNow < nextWeatherUtc) return;
            if (!settings.Has("lat") || !settings.Has("lon")) return;
            weatherBusy = true;
            double lat = settings.GetDouble("lat", 0), lon = settings.GetDouble("lon", 0);
            string place = settings.Get("city", "");
            ThreadPool.QueueUserWorkItem(delegate
            {
                WeatherReport report = null;
                try { report = WeatherService.Fetch(lat, lon, place); }
                catch (Exception ex) { Log.Write("weather: " + ex.Message); }
                Post(() =>
                {
                    weatherBusy = false;
                    nextWeatherUtc = DateTime.UtcNow.AddMinutes(report != null ? 60 : 10);
                    if (report != null) weather = report;
                    Kick();
                });
            });
        }

        WeatherEntry CurrentWeather()
        {
            return WeatherEnabled && weather != null ? weather.Current(DateTime.Now) : null;
        }

        /// <summary>Shows a single heads-up when rain is expected within the next two hours.</summary>
        void CheckRain()
        {
            if (!WeatherEnabled || weather == null) return;
            var rain = weather.NextRain(DateTime.Now, 2);
            if (rain == null) return;
            string key = rain.Local.ToString("yyyyMMddHH");
            if (key == rainNotified) return;
            rainNotified = key;
            var t = ShowToast(View.Rain, 6, 2, false);
            if (t != null) t.Data = rain;
        }

        static string Temp(WeatherEntry entry) { return Math.Round(entry.TempC) + "°"; }
        static bool IsNight(DateTime local) { return local.Hour < 6 || local.Hour >= 18; }

        void DrawRainToast(float w, float h)
        {
            var entry = toast != null ? toast.Data as WeatherEntry : null;
            if (entry == null) return;
            float cy = h / 2;
            WeatherArt.Draw(paint, entry.Code, 24, cy, 20, IsNight(entry.Local));
            bool soon = entry.Local <= DateTime.Now;
            paint.Text(soon ? entry.Desc + " now" : entry.Desc + " at " + Hm(entry.Local),
                Theme.Semi(13.5f), Theme.Text, 42, cy, w - 42 - 100, StringAlignment.Near);
            paint.TextRight(weather != null ? weather.Place : "", Theme.Ui(12f), Theme.Sub, w - 16, cy);
        }
    }
}