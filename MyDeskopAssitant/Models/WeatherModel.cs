using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyDeskopAssitant.Models
{
    public class WeatherRoot
    {
        [JsonPropertyName("weather")]
        public List<WeatherDescription> Weather { get; set; }

        [JsonPropertyName("main")]
        public MainData Main { get; set; }

        [JsonPropertyName("name")]
        public string CityName { get; set; }
    }
    public class WeatherDescription
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } 

        [JsonPropertyName("icon")]
        public string Icon { get; set; } 
    }

    public class MainData
    {
        [JsonPropertyName("temp")]
        public double Temp { get; set; } 
    }
}
