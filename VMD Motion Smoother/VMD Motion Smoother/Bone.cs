using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VMD_Motion_Smoother
{
    class Bone
    {
        public string name;
        public uint frameNumber;
        public float x, y, z, rX, rY, rZ, w;
        public byte[] interpolation;

        public Bone(string name, uint frameNumber, float x, float y, float z, float rX, float rY, float rZ, float w, byte[] interpolation)
        {
            this.name = name;
            this.frameNumber = frameNumber;
            this.x = x;
            this.y = y;
            this.z = z;
            this.rX = rX;
            this.rY = rY;
            this.rZ = rZ;
            this.w = w;
            this.interpolation = interpolation;
        }

        public String toString()
        {
            return (String.Format("{0} in Frame {1} hat die Koordinaten ({2}|{3}|{4}) mit der Rotation ({5}|{6}|{7}) ({8})", name, frameNumber, x, y, z, rX, rY, rZ, w));
        }

        public void printInterpolation(StreamWriter fs)
        {
            fs.WriteLine();
            for(int i = 0; i < interpolation.Length; i++)
            {
                fs.Write(String.Format("{0},", interpolation[i]));
            }
            fs.WriteLine();
            Console.WriteLine("Done");
        }

        public byte[] getAverageInterpolation(Bone second)
        {
            byte[] output = new byte[64];

            for(int i = 0; i < 64; i++)
            {
                output[i] = (byte)((this.interpolation[i] + second.interpolation[i]) / 2);
            }

            return output;
        }

        public static byte[] getLinearInterpolation()
        {
            byte[] output = {20, 20, 0, 0, 20, 20, 20, 20, 107, 107, 107, 107, 107, 107, 107, 107, 20, 20, 20, 20, 20, 20, 20, 107, 107, 107, 107, 107, 107, 107, 107, 0, 20, 20, 20, 20, 20, 20, 107, 107, 107, 107, 107, 107, 107, 107, 0, 0, 20, 20, 20, 20, 20, 107, 107, 107, 107, 107, 107, 107, 107, 0, 0, 0};
            return output;
        }
        
    }
}
