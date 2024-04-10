using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Lab5
{
    public class CityList : DataManipulation
    {
        public List<City> Cities { get; set; }
        public CityList() { }

        public CityList(List<City> cities) {
            this.Cities = cities;
        }
        public List<T> LoadFromJson<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File '{filePath}' not found.");
            }

            string jsonString = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<List<T>>(jsonString);
        }

        public List<T> LoadFromXml<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File '{filePath}' not found.");
            }

            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                return (List<T>)serializer.Deserialize(fileStream);
            }
        }

        public void SaveToJson<T>(List<T> data, string filePath)
        {
            string jsonString = JsonSerializer.Serialize(data);

            File.WriteAllText(filePath, jsonString);
        }

        public void SaveToXml<T>(List<T> data, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(fileStream, data);
            }
        }
    }
}
