using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace VMD_Motion_Smoother
{
    class Program
    {
        private static Encoding ShiftJISEncoding = Encoding.GetEncoding("shift-jis");

        public static string ReadVMDString(BinaryReader br, int length)
        {
            byte[] buffer = new byte[length];
            br.BaseStream.Read(buffer, 0, length);
            return ShiftJISEncoding.GetString(buffer);
        }

        static void Main(string[] args)
        {
            /*
            //Console.Write("Pfad der VMD Datei angeben: ");
            //string path = Console.ReadLine();
            FileStream fs = new FileStream(@"C:\Users\fgute\Documents\Visual Studio Projects\VMD Motion Smoother\armpit fart.vmd", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BinaryReader br = new BinaryReader(fs);

            char[] version = br.ReadChars(30);
            Console.WriteLine(version);
            string nameModel = ReadVMDString(br, 20);
            Console.WriteLine(nameModel);
            uint keyframeCount = br.ReadUInt32();
            string boneName;
            uint frameNumber;
            float x, y, z, rX, rY, rZ, w;
            byte[] interpolation;
            List<Bone> bones = new List<Bone>();
            Console.WriteLine(keyframeCount);

            for (int i = 0; i < keyframeCount; i++)
            {
                //Console.WriteLine(i);
                boneName = ReadVMDString(br, 15);
                frameNumber = br.ReadUInt32();
                x = System.BitConverter.ToSingle(br.ReadBytes(4), 0);
                y = System.BitConverter.ToSingle(br.ReadBytes(4), 0);
                z = System.BitConverter.ToSingle(br.ReadBytes(4), 0);
                rX = System.BitConverter.ToSingle(br.ReadBytes(4), 0);
                rY = System.BitConverter.ToSingle(br.ReadBytes(4), 0);
                rZ = System.BitConverter.ToSingle(br.ReadBytes(4), 0);
                w = System.BitConverter.ToSingle(br.ReadBytes(4), 0);
                interpolation = br.ReadBytes(64);

                bones.Add(new Bone(boneName, frameNumber, x, y, z, rX, rY, rZ, w, interpolation));
            }

            Console.WriteLine("{0} with {1} frames, first bone ", nameModel, keyframeCount);
            for (int j = 0; j < keyframeCount; j++)
            {
                Console.WriteLine(bones[j].toString());
                //bones[j].printInterpolation();
            }

            Console.WriteLine("\n\n\n");

            uint facialCount = br.ReadUInt32();
            List<Facial> facials = new List<Facial>();

            string facialName;
            uint index;
            float weight;
            for (int i = 0; i < facialCount; i++)
            {
                facialName = ReadVMDString(br, 15);
                index = br.ReadUInt32();
                weight = System.BitConverter.ToSingle(br.ReadBytes(4), 0);

                facials.Add(new Facial(facialName, index, weight));
            }

            Console.WriteLine("And now the facials:");
            for(int j = 0; j < facialCount; j++)
            {
                Console.WriteLine(facials[j].toString());
            }


            br.Close();
            br = null;

            */

            /*
             * Nun wird die VMD wieder zurueckgeschrieben
             */

            /*
            FileStream fsNew = new FileStream(@"C:\Users\fgute\Documents\Visual Studio Projects\VMD Motion Smoother\armpit fart output.vmd", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fsNew);

            bw.Write(version);
            bw.Write(nameModel);
            bw.Write(keyframeCount);
            
            for(int i = 0; i < keyframeCount; i++)
            {
                bw.Write(bones[i].name);
                bw.Write(bones[i].frameNumber);
                bw.Write(bones[i].x);
                bw.Write(bones[i].y);
                bw.Write(bones[i].z);
                bw.Write(bones[i].rX);
                bw.Write(bones[i].rY);
                bw.Write(bones[i].rZ);
                bw.Write(bones[i].w);
                bw.Write(bones[i].interpolation);
            }

            bw.Write(facialCount);

            for(int j = 0; j < facialCount; j++)
            {
                bw.Write(facials[j].name);
                bw.Write(facials[j].index);
                bw.Write(facials[j].weight);
            }
            */

            ModelMotion motionIn = new ModelMotion(@"C:\Users\fgute\Desktop\MikuMikuDanceE_v926x64\UserFile\Motion\VMD Motion Smoother Test\Bowser Knuddelout for edit.vmd");

            /*
            int[,] frames = new int[2, 3];

            frames[0, 0] = 44;
            frames[0, 1] = 66;
            frames[0, 2] = 1;

            frames[1, 0] = 106;
            frames[1, 1] = 142;
            frames[1, 2] = 2;

            /*

            //motionIn.buildInGaps(frames);


            /*
            motionIn.printMotionContent();

            motionIn.writeMotionFile(@"C:\Users\fgute\Desktop\MikuMikuDanceE_v926x64\UserFile\Motion\VMD Motion Smoother Test\Raw Improved 2.vmd");

            Quaternion start, end;

            start = new Quaternion(0.01790054f, -0.02586449f, -0.01698698f, 0.9993608f);
            end = new Quaternion(0.02706528f, -0.03419076f, -0.02523879f, 0.9987299f);

            Console.WriteLine((start - end).ToString());
            */

            //motionIn.printMotionContent();
            //motionIn.printInterpolation(0, 5);

            //motionIn.deleteNthFrames(2);
            //motionIn.deleteNthFrames(3);
            motionIn.keepNthFrames(5);
            //motionIn.printMotionContent();


            motionIn.tweakKeys(3, 5);

            //motionIn.tweakInterpolation(10);

            motionIn.writeMotionFile(@"C:\Users\fgute\Desktop\MikuMikuDanceE_v926x64\UserFile\Motion\VMD Motion Smoother Test\Bowser Knuddelout for edit 2.vmd");

            //ModelMotion motionOut = new ModelMotion(@"C:\Users\fgute\Documents\Visual Studio Projects\VMD Motion Smoother\armpit fart output.vmd");

            //motionOut.printMotionContent();
        }
    }
}
