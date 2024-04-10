using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab5
{
    internal interface DataManipulation
    {
        void SaveToJson<T>(List<T> data, string filePath);
        List<T> LoadFromJson<T>(string filePath);

        void SaveToXml<T>(List<T> data, string filePath);
        List<T> LoadFromXml<T>(string filePath);
    }
}
