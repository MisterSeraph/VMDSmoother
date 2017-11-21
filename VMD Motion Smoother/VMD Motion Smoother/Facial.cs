using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMD_Motion_Smoother
{
    class Facial
    {
        public string name;
        public uint index;
        public float weight;

        public Facial(string name, uint index, float weight)
        {
            this.name = name;
            this.index = index;
            this.weight = weight;
        }

        public String toString()
        {
            return String.Format("Facial {0} in Frame {1} hat den Wert {2}", name, index, weight);
        }
    }
}
