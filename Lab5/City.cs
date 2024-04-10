using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab5
{
    public enum Country
    {
        Unknown,
        USA,
        Russia,
        China,
        UK,
        India,
        Brazil,
        SouthKorea,
        Canada,
        Australia,
        Norway,
        Ireland
    }

    public class City
    {
        public string Name { get; set; }
        public int Population { get; set; }
        public Country Country { get; set; }
        public string EmblemPath { get; set; }

        public City() { }


        public City(string name, int population, Country country)
        {
            Name = name;
            Population = population;
            Country = country;
        }

        public City(string name, int population, Country country, string emblemPath)
        {
            Name = name;
            Population = population;
            Country = country;
            EmblemPath = emblemPath;
        }
    }
}
