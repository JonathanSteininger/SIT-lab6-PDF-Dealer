using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace forms
{
    public class MySerializer<T>
    {
        public T ReadFile(string file)
        {
            if (!File.Exists(file)) return default(T);
            BinaryFormatter formatter = new BinaryFormatter();
            using(FileStream stream = new FileStream(file, FileMode.Open))
            {
                return (T)formatter.Deserialize(stream);
            }
        }
        public void WriteFile(string file, T obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(file, FileMode.Create))
            {
                formatter.Serialize(stream, obj);
            }
        }

    }
}
