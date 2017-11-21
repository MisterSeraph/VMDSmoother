using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace VMD_Motion_Smoother
{
    class ModelMotion
    {
        private char[] version;
        private string nameModel;
        private uint keyframeCount;
        private uint facialCount;
        private List<Bone> bones;
        private List<Facial> facials;
        private Bone[][] bonesSorted;

        private static Encoding ShiftJISEncoding = Encoding.GetEncoding("shift-jis");

        public ModelMotion(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BinaryReader br = new BinaryReader(fs);

            version = br.ReadChars(30);
            nameModel = ReadVMDString(br, 20);
            keyframeCount = br.ReadUInt32();
            string boneName;
            uint frameNumber;
            float x, y, z, rX, rY, rZ, w;
            byte[] interpolation;
            bones = new List<Bone>();

            for (int i = 0; i < keyframeCount; i++)
            {
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

            facialCount = br.ReadUInt32();
            facials = new List<Facial>();

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

            br.Close();
            br = null;

            fs.Close();
            fs = null;
        }

        private string ReadVMDString(BinaryReader br, int length)
        {
            byte[] buffer = new byte[length];
            br.BaseStream.Read(buffer, 0, length);
            return ShiftJISEncoding.GetString(buffer);
        }

        public static void WriteVMDString(BinaryWriter bw, int length, string str, Encoding encoding = null)
        {
            if (encoding == null)
            {
                if (ShiftJISEncoding == null)
                {
                    ShiftJISEncoding = Encoding.GetEncoding("shift-jis");
                }

                encoding = ShiftJISEncoding;
            }
            
            str = str.Trim();

            str = str.Replace("\r\n", "\n");

            byte[] buffer = encoding.GetBytes(str);

            byte[] sendBuffer = new byte[length];
            for (int i = 0; i < length; i++)
            {
                sendBuffer[i] = 0x00;
            }
            Array.Copy(buffer, sendBuffer, Math.Min(buffer.Length, length));

            bw.BaseStream.Write(sendBuffer, 0, length);
        }

        public void printMotionContent()
        {
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("+ Output of the motion data");
            Console.WriteLine("+");
            Console.Write("+ Version of this motion: ");
            Console.WriteLine(version);
            Console.WriteLine("+ Name of the model this motion was made with: {0}", nameModel);
            Console.WriteLine("+");
            Console.WriteLine("+ Amount of bone frames: {0}", keyframeCount);
            Console.WriteLine("+ Amount of facial frames: {0}", facialCount);
            Console.WriteLine("+\n+ Output of all the Bone Frames in the motion\n+");

            for(int i = 0; i < keyframeCount; i++)
            {
                Console.WriteLine("+ {0}", bones[i].toString());
            }

            Console.WriteLine("+\n++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("+\n+ Output of all the Facials Frames in the motion\n");

            for (int j = 0; j < facialCount; j++)
            {
                Console.WriteLine("+ {0}", facials[j].toString());
            }

            Console.WriteLine("+\n++++++++++++++++++++++++++++++++++++++++++++++++++++++\n");
        }

        public void writeMotionFile(string filePath)
        {
            FileStream fsNew = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryWriter bw = new BinaryWriter(fsNew);

            WriteVMDString(bw, 30, "Vocaloid Motion Data 0002", Encoding.ASCII);
            WriteVMDString(bw, 20, this.nameModel);
            bw.Write(keyframeCount);

            for (int i = 0; i < keyframeCount; i++)
            {
                WriteVMDString(bw, 15, this.bones[i].name);
                bw.Write(this.bones[i].frameNumber);
                bw.Write(this.bones[i].x);
                bw.Write(this.bones[i].y);
                bw.Write(this.bones[i].z);
                bw.Write(this.bones[i].rX);
                bw.Write(this.bones[i].rY);
                bw.Write(this.bones[i].rZ);
                bw.Write(this.bones[i].w);
                bw.BaseStream.Write(this.bones[i].interpolation, 0, 64);
            }

            bw.Write(facialCount);

            for (int j = 0; j < facialCount; j++)
            {
                WriteVMDString(bw, 15, this.facials[j].name);
                bw.Write(this.facials[j].index);
                bw.Write(this.facials[j].weight);
            }

            bw.Close();
            bw = null;

            fsNew.Close();
            fsNew = null;
        }

        public void tweakKeys(int moveRange, int tolerance)
        {
            Random rnd = new Random();
            int offset = 0, negative = 0;


            for(int i = 0; i < keyframeCount; i++)
            {
                if(bones[i].frameNumber != 0)
                {
                    offset = rnd.Next(moveRange);
                    negative = rnd.Next(1);

                    if(negative == 0)
                    {
                        bones[i].frameNumber += (uint)offset;
                        //bones[i].frameNumber += (uint)safelyMoveRight(bones[i].name, bones[i].frameNumber, bones[i].frameNumber + (uint)offset, tolerance);
                    }
                    else
                    {
                        if((bones[i].frameNumber - (uint)offset) > 0)
                        {
                            bones[i].frameNumber -= (uint)offset;
                            //bones[i].frameNumber -= (uint)safelyMoveLeft(bones[i].name, bones[i].frameNumber, bones[i].frameNumber + (uint)offset, tolerance);
                        }
                    }
                }
            }
        }

        private uint safelyMoveRight(string nameBone, uint currentPosition, uint desiredPosition, int tolerance)
        {
            bool possible = true;
            for(int i = 0; i < keyframeCount; i++)
            {
                if(bones[i].name == nameBone && bones[i].frameNumber - tolerance > 5 && bones[i].frameNumber > currentPosition)
                {
                    if((desiredPosition > bones[i].frameNumber - tolerance))
                    {
                        possible = false;
                    }
                } 
            }

            if(possible)
            {
                return desiredPosition;
            }
            else
            {
                return 0;
            }
        }

        private uint safelyMoveLeft(string nameBone, uint currentPosition, uint desiredPosition, int tolerance)
        {
            bool possible = true;
            for (int i = 0; i < keyframeCount; i++)
            {
                if (bones[i].name == nameBone && bones[i].frameNumber - tolerance > 5 && bones[i].frameNumber < currentPosition)
                {
                    if ((desiredPosition < bones[i].frameNumber - tolerance))
                    {
                        possible = false;
                    }
                }
            }

            if (possible)
            {
                return desiredPosition;
            }
            else
            {
                return 0;
            }
        }

        public void tweakInterpolation(byte interpolRange)
        {
            /*
            byte min, max;
            for(int i = 0; i < keyframeCount; i++)
            {
                min = max = bones[i].interpolation[0];
                for(int j = 1; j < 64; j++)
                {
                    if(bones[i].interpolation[j] > max)
                    {
                        max = bones[i].interpolation[j];
                    }
                    if(bones[i].interpolation[j] < min)
                    {
                        min = bones[i].interpolation[j];
                    }
                }

                Random rnd = new Random();
                int sign;

                for(int j = 0; j < 64; j++)
                {
                    sign = rnd.Next(-1, 2);
                    bones[i].interpolation[j] += (byte)((rnd.Next(min, max) / 2)*sign);
                }
            }
            */

            byte[] interpol1 = { 20, 20, 0, 0, 20, 20, 20, 20, 107, 107, 107, 107, 107, 107, 107, 107, 20, 20, 20, 20, 20, 20, 20, 107, 107, 107, 107, 107, 107, 107, 107, 0, 20, 20, 20, 20, 20, 20, 107, 107, 107, 107, 107, 107, 107, 107, 0, 0, 20, 20, 20, 20, 20, 107, 107, 107, 107, 107, 107, 107, 107, 0, 0, 0 };
            byte[] interpol2 = { 20, 20, 0, 0, 20, 20, 20, 10, 107, 107, 107, 65, 107, 107, 107, 120, 20, 20, 65, 20, 20, 20, 10, 107, 107, 107, 65, 107, 107, 107, 120, 0, 20, 65, 20, 20, 20, 10, 107, 107, 107, 65, 107, 107, 107, 120, 0, 0, 65, 20, 20, 20, 10, 107, 107, 107, 65, 107, 107, 107, 120, 0, 0, 0 };
            byte[] interpol3 = { 20, 20, 0, 0, 20, 20, 20, 18, 107, 107, 107, 69, 107, 107, 107, 99, 20, 20, 47, 20, 20, 20, 18, 107, 107, 107, 69, 107, 107, 107, 99, 0, 20, 47, 20, 20, 20, 18, 107, 107, 107, 69, 107, 107, 107, 99, 0, 0, 47, 20, 20, 20, 18, 107, 107, 107, 69, 107, 107, 107, 99, 0, 0, 0 };
            byte[] interpol4 = { 20, 20, 0, 0, 20, 20, 20, 11, 107, 107, 107, 107, 107, 107, 107, 107, 20, 20, 66, 20, 20, 20, 11, 107, 107, 107, 107, 107, 107, 107, 107, 0, 20, 66, 20, 20, 20, 11, 107, 107, 107, 107, 107, 107, 107, 107, 0, 0, 66, 20, 20, 20, 11, 107, 107, 107, 107, 107, 107, 107, 107, 0, 0, 0 };
            List<byte[]> allInterpol = new List<byte[]>();

            allInterpol.Add(interpol1);
            allInterpol.Add(interpol2);
            allInterpol.Add(interpol3);
            allInterpol.Add(interpol4);
            //Console.WriteLine("{0}, {1}, {2}, {3}", interpol1.Length, interpol2.Length, interpol3.Length, interpol4.Length);

            Random rdn = new Random();

            for (int i = 0; i < keyframeCount; i++)
            {
                switch (rdn.Next(0, 15))
                {
                    case 0:
                        bones[i].interpolation = interpol1;
                        break;
                    case 1:
                        bones[i].interpolation = interpol2;
                        break;
                    case 2:
                        bones[i].interpolation = interpol3;
                        break;
                    case 3:
                        bones[i].interpolation = interpol4;
                        break;
                    default:
                        int mergeA, mergeB;
                        byte[] merge = new byte[64];
                        mergeA = rdn.Next(0, 4);
                        mergeB = rdn.Next(0, 4);
                        for (int j = 0; j < 64; j++)
                        {
                            merge[j] = (byte)((allInterpol[mergeA][j] + allInterpol[mergeB][j]) / 2);
                        }
                        bones[i].interpolation = merge;
                        break;
                }

            }

        }

        public void deleteNthFrames(int step)
        {
            int amountDeleted = 0;
            int maxFrame = 0;

            for(int i = 0; i < keyframeCount; i++)
            {
                if(bones[i].frameNumber > maxFrame)
                {
                    maxFrame = (int)bones[i].frameNumber;
                }
            }

            for(int x = step; x < maxFrame; x += step)
            {
                for(int y = 0; y < keyframeCount; y++)
                {
                    if(bones[y].frameNumber == x)
                    {
                        bones.RemoveAt(y);
                        keyframeCount--;
                        y--;
                    }
                }
            }

            //keyframeCount -= (uint)amountDeleted;
        }

        public void keepNthFrames(int step)
        {
            int amountDeleted = 0;
            int maxFrame = 0;

            for (int i = 0; i < keyframeCount; i++)
            {
                if (bones[i].frameNumber > maxFrame)
                {
                    maxFrame = (int)bones[i].frameNumber;
                }
            }

            for (int x = 1; x < maxFrame; x++)
            {
                if(x % step == 0)
                {
                    x++;
                }
                for (int y = 0; y < keyframeCount; y++)
                {

                    if (bones[y].frameNumber == x)
                    {
                        bones.RemoveAt(y);
                        keyframeCount--;
                        y--;
                    }
                }
            }

            //keyframeCount -= (uint)amountDeleted;
        }

        public void buildInGaps(int[,] pairs)
        {

            List<Bone> newBones = new List<Bone>();

            for (int i = 0; i < pairs.GetLength(0); i++)
            {
                Console.WriteLine("In Iterationsschritt {0}", i);

                int amountStartFrames = 0, amountEndFrames = 0, foundPairs = 0;
                Bone[] startBones;
                Bone[] endBones;
                string[] pairedBoneNames;
                float[,] rotationsStartEnd;
                const int tweakLimit = 3;

                for(int j = 0; j < keyframeCount; j++) //Ermittle Anzahl an relevanten Bones
                {
                    if(bones[j].frameNumber == pairs[i,0]) //Gehört es zum Startframe?
                    {
                        amountStartFrames++;
                    }
                    else if(bones[j].frameNumber == pairs[i,1]) //Oder gehört es zum Endframe?
                    {
                        amountEndFrames++;
                    }
                }

                Console.WriteLine("Habe {0} Startbones und {1} Endbones gefunden!", amountStartFrames, amountEndFrames);

                startBones = new Bone[amountStartFrames];
                endBones = new Bone[amountEndFrames];
                int indexStart = 0, indexEnd = 0;

                for(int j = 0; j < keyframeCount; j++) //Erstelle Referenze auf die relevanten Bones
                {
                    if (bones[j].frameNumber == pairs[i,0]) //Gehört es zum Startframe?
                    {
                        startBones[indexStart] = bones[j];
                        indexStart++;
                    }
                    else if (bones[j].frameNumber == pairs[i,1]) //Oder gehört es zum Endframe?
                    {
                        endBones[indexEnd] = bones[j];
                        indexEnd++;
                    }
                }

                for(int x = 0; x < amountStartFrames; x++) //Find the amount of relevant bone pairs
                {
                    for (int y = 0; y < amountEndFrames; y++)
                    {
                        if(startBones[x].name.Equals(endBones[y].name))
                        {
                            foundPairs++;
                        }
                    }
                }

                Console.WriteLine("Paare gefunden: {0}", foundPairs);

                pairedBoneNames = new string[foundPairs];
                rotationsStartEnd = new float[14,foundPairs];
                int indexNameArray = 0;

                for (int x = 0; x < amountStartFrames; x++) //Transfer the values
                {
                    for (int y = 0; y < amountEndFrames; y++)
                    {
                        if (startBones[x].name.Equals(endBones[y].name))
                        {
                            pairedBoneNames[indexNameArray] = startBones[x].name;

                            rotationsStartEnd[0, indexNameArray] = startBones[x].x;
                            rotationsStartEnd[1, indexNameArray] = startBones[x].y;
                            rotationsStartEnd[2, indexNameArray] = startBones[x].z;
                            rotationsStartEnd[3, indexNameArray] = startBones[x].rX;
                            rotationsStartEnd[4, indexNameArray] = startBones[x].rY;
                            rotationsStartEnd[5, indexNameArray] = startBones[x].rZ;
                            rotationsStartEnd[6, indexNameArray] = startBones[x].w;

                            rotationsStartEnd[7, indexNameArray] = endBones[y].x;
                            rotationsStartEnd[8, indexNameArray] = endBones[y].y;
                            rotationsStartEnd[9, indexNameArray] = endBones[y].z;
                            rotationsStartEnd[10, indexNameArray] = endBones[y].rX;
                            rotationsStartEnd[11, indexNameArray] = endBones[y].rY;
                            rotationsStartEnd[12, indexNameArray] = endBones[y].rZ;
                            rotationsStartEnd[13, indexNameArray] = endBones[y].w;

                            indexNameArray++;
                        }
                    }
                }

                int newFrame = pairs[i, 0], span = (pairs[i, 1] - pairs[i, 0]);
                int offsetFrame = (int)((span) / (pairs[i, 2] + 1));
                float[] newValues = new float[3];
                float rotationOffset;
                Random rnd = new Random();
                
                for(int x = 0; x < pairs[i, 2]; x++) //Anzahl an Posen, errechne am Anfang den neuen Frame
                {
                    newFrame += offsetFrame;

                    for(int y = 0; y < foundPairs; y++) //Berechne die neue Pose
                    {
                        Quaternion start, end, newOne, substract, offset1, offset2;
                        for (int z = 0; z < 3; z++)
                        {
                            newValues[z] = (rotationsStartEnd[7 + z, y] + rotationsStartEnd[z, y]) / 2;
                        }
                        /*
                        newValues[0] = rotationsStartEnd[10, y] - rotationsStartEnd[3, y];
                        newValues[0] = newValues[0] / span;
                        newValues[0] = newValues[0] * (offsetFrame * (y + 1));
                        newValues[0] = rotationsStartEnd[3, y] + newValues[0];

                        newValues[1] = rotationsStartEnd[11, y] - rotationsStartEnd[4, y];
                        newValues[1] = newValues[1] / span;
                        newValues[1] = newValues[1] * (offsetFrame * (y + 1));
                        newValues[1] = rotationsStartEnd[4, y] + newValues[1];

                        newValues[2] = rotationsStartEnd[12, y] - rotationsStartEnd[5, y];
                        newValues[2] = newValues[2] / span;
                        newValues[2] = newValues[2] * (offsetFrame * (y + 1));
                        newValues[2] = rotationsStartEnd[5, y] + newValues[2];

                        newValues[3] = rotationsStartEnd[13, y] - rotationsStartEnd[6, y];
                        newValues[3] = newValues[3] / span;
                        newValues[3] = newValues[3] * (offsetFrame * (y + 1));
                        newValues[3] = rotationsStartEnd[6, y] + newValues[3];
                        */

                        start = new Quaternion(rotationsStartEnd[3, y], rotationsStartEnd[4, y], rotationsStartEnd[5, y], rotationsStartEnd[6, y]);
                        end = new Quaternion(rotationsStartEnd[10, y], rotationsStartEnd[11, y], rotationsStartEnd[12, y], rotationsStartEnd[13, y]);

                        newOne = Quaternion.Slerp(start, end, 0.045f);

                        offset1 = new Quaternion(0.01790054f, -0.02586449f, -0.01698698f, 0.9993608f);
                        offset2 = new Quaternion(0.02706528f, -0.03419076f, -0.02523879f, 0.9987299f);

                        substract = offset1-offset2;

                        Console.WriteLine(substract.ToString());

                        Console.WriteLine(newOne.ToString());
                        
                        if(rnd.Next(0,2) == 0)
                        {
                            newOne += substract;
                        }
                        else
                        {
                            newOne -= substract;
                        }

                        /*
                        for (int z = 0; z < 4; z++)
                        {
                            rotationOffset = rnd.Next(-tweakLimit, tweakLimit);
                            rotationOffset = rotationOffset - (float)((1.0 - (float)(1 / rnd.Next(1, 10))) * rotationOffset < 0 ? -1.0 : 1.0);

                            newValues[z + 3] += rotationOffset;
                        }
                        */

                        //Console.WriteLine("Quaternion: {0}", quat.ToString());

                        newBones.Add(new Bone(pairedBoneNames[y], (uint)newFrame, newValues[0], newValues[1], newValues[2], newOne.X, newOne.Y, newOne.Z, newOne.W, Bone.getLinearInterpolation()));
                    }
                }
            }

            updateBoneList(newBones);
        }

        public void tweakKeysAndInterpolation(int moveRange, byte interpolRange, int tolerance)
        {
            this.tweakKeys(moveRange, tolerance);
            this.tweakInterpolation(interpolRange);
        }

        public void printInterpolation(int from, int to)
        {
            StreamWriter output = new StreamWriter(@"C:\Users\fgute\Desktop\MikuMikuDanceE_v926x64\UserFile\Motion\VMD Motion Smoother Test\Interpol.txt");
            for(int i = from; i < to; i++)
            {
                bones[i].printInterpolation(output);
            }
        }

        private void updateBoneList(List<Bone> addUp)
        {
            uint currentFrame, keyframeCountIncrease = 0;
            List<Bone> newList = new List<Bone>();
            int indexNewArray = 0;
            int sizeAdd = addUp.Count;

            for(int i = 0; i < keyframeCount; i++)
            {
                currentFrame = bones[i].frameNumber;
                newList.Add(bones[i]);
                if(i+1 < keyframeCount && currentFrame != bones[i+1].frameNumber && indexNewArray + 1 < sizeAdd)
                {
                    if(addUp[indexNewArray].frameNumber > currentFrame && addUp[indexNewArray].frameNumber < bones[i+1].frameNumber)
                    {
                     
                        while(indexNewArray < sizeAdd && addUp[indexNewArray].frameNumber < bones[i+1].frameNumber)
                        {
                            newList.Add(addUp[indexNewArray]);
                            keyframeCountIncrease++;
                            Console.WriteLine("{0} - {1}, {2} - {3}", keyframeCount, i, sizeAdd, indexNewArray);
                            if(keyframeCount + 1 == sizeAdd)
                            {
                                break;
                            }
                            indexNewArray++;
                        }
                    }
                }
            }

            bones = null;
            bones = newList;
            keyframeCount += keyframeCountIncrease;
        }
    }
}
