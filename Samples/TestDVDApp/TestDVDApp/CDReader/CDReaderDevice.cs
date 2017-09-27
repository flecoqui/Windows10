using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDVDApp.CDReader
{
    public class CDReaderDevice
    {
        public CDReaderDevice(string name, string id)
        {
            Id = id;
            Name = name;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public override string ToString()
        {
            return string.Format("Name: {0} Id: {1} ", Name, Id);
        }
    }
}
