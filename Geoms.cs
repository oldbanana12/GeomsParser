using System;
using System.Collections;
using System.IO;

namespace GeomsParser
{
    internal class Geoms
    {
        private Header header;
        private Section2 section2;
        private Section3 section3;
        private Section4 section4;
        private Section5[] section5;

        private struct Vector3
        {
            public float x;
            public float y;
            public float z;
        }

        private struct Header
        {
            public uint version;
            public uint endOfHeaderOffset;
            public uint fileLength;

            public uint u1;
            public uint u2;
            public uint u3;
            public uint u4;
            public uint u5;
        } //Header

        private struct Section2
        {
            public uint u1;
            public uint u2;
            public uint u3; // type of some sort?
            public uint u4;
            public uint u5;
            public uint u6;
            public uint nextSectionRelativeOffset;
            public uint u8;
        }

        private struct Section3
        {
            public uint u1;
            public uint u2;
            public uint u3; // type of some sort?
            public uint nextSectionRelativeOffset;
        }

        private struct Section4
        {
            public Vector3 p1;
            public ushort u1;
            public ushort section5count;
            public Vector3 p2;
            public uint nextSectionOffset;
        }

        private struct Section5
        {
            public byte[] data;
        }

        public Geoms()
        {
        }

        internal void read(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                try
                {
                    BinaryReader reader = new BinaryReader(stream);
                    header = new Header();
                    section2 = new Section2();
                    section3 = new Section3();

                    ReadHeader(reader);
                    reader.BaseStream.Position = header.endOfHeaderOffset;

                    ReadSection2(reader);
                    reader.BaseStream.Position = header.endOfHeaderOffset + section2.nextSectionRelativeOffset;

                    ReadSection3(reader);

                    if (section3.u3 == 0x00)
                    {
                        reader.BaseStream.Position = header.endOfHeaderOffset + section2.nextSectionRelativeOffset + section3.nextSectionRelativeOffset;

                        section4 = new Section4();
                        ReadSection4(reader);

                        reader.BaseStream.Position = header.endOfHeaderOffset + section2.nextSectionRelativeOffset + section3.nextSectionRelativeOffset + section4.nextSectionOffset;

                        section5 = new Section5[section4.section5count];
                        ReadSection5Entries(reader);

                        reader.BaseStream.Position += 64;

                        using (StreamWriter file = new StreamWriter("geoms.obj"))
                            ReadData(reader, file);
                    } else if (section3.u3 == 0x06) {
                        reader.BaseStream.Position = header.endOfHeaderOffset + section2.nextSectionRelativeOffset + section3.nextSectionRelativeOffset;

                        reader.BaseStream.Position += 16;

                        var offset = reader.ReadUInt32();

                        reader.BaseStream.Position += offset - 20;

                        using (StreamWriter file = new StreamWriter("geoms.obj"))
                            ReadData(reader, file);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                    Console.Write(e.StackTrace);
                }
                finally
                {
                    stream.Close();
                }
            }
        }

        private void ReadHeader(BinaryReader reader)
        {
            header.version = reader.ReadUInt32();
            header.endOfHeaderOffset = reader.ReadUInt32();
            header.fileLength = reader.ReadUInt32();

            header.u1 = reader.ReadUInt32();
            header.u2 = reader.ReadUInt32();
            header.u3 = reader.ReadUInt32();
            header.u4 = reader.ReadUInt32();
            header.u5 = reader.ReadUInt32();
        }

        private void ReadSection2(BinaryReader reader)
        {
            section2.u1 = reader.ReadUInt32();
            section2.u2 = reader.ReadUInt32();
            section2.u3 = reader.ReadUInt32();
            section2.u4 = reader.ReadUInt32();
            section2.u5 = reader.ReadUInt32();
            section2.u6 = reader.ReadUInt32();
            section2.nextSectionRelativeOffset = reader.ReadUInt32();
            section2.u8 = reader.ReadUInt32();
        }

        private void ReadSection3(BinaryReader reader)
        {
            section3.u1 = reader.ReadUInt32();
            section3.u2 = reader.ReadUInt32();
            section3.u3 = reader.ReadUInt32();
            section3.nextSectionRelativeOffset = reader.ReadUInt32();
        }

        private void ReadSection4(BinaryReader reader)
        {
            section4.p1 = new Vector3();
            section4.p2 = new Vector3();

            section4.p1.x = reader.ReadSingle();
            section4.p1.y = reader.ReadSingle();
            section4.p1.z = reader.ReadSingle();

            section4.u1 = reader.ReadUInt16();
            section4.section5count = reader.ReadUInt16();

            section4.p2.x = reader.ReadSingle();
            section4.p2.y = reader.ReadSingle();
            section4.p2.z = reader.ReadSingle();

            section4.nextSectionOffset = reader.ReadUInt32();
        }

        private void ReadSection5Entries(BinaryReader reader)
        {
            for (int i = 0; i < section4.section5count; i++)
            {
                var entry = section5[i];
                entry.data = reader.ReadBytes(32);
                section5[i] = entry;
            }
        }

        private void ReadData(BinaryReader reader, StreamWriter file)
        {
            uint v = 0;
            string facesBuffer = "";
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var first = reader.ReadUInt32();
                Console.WriteLine(first.ToString("x"));
                if (first == 0x01020004)
                {
                    Console.WriteLine("Position chunk thing");

                    reader.BaseStream.Position += 28;
                    var p1 = new Vector3();
                    p1.x = reader.ReadSingle();
                    p1.y = reader.ReadSingle();
                    p1.z = reader.ReadSingle();
                    reader.BaseStream.Position += 4;

                    var p2 = new Vector3();
                    p2.x = reader.ReadSingle();
                    p2.y = reader.ReadSingle();
                    p2.z = reader.ReadSingle();
                    reader.BaseStream.Position += 4;

                    Console.WriteLine("p1: {0} {1} {2}, p2: {3} {4} {5}", p1.x, p1.y, p1.z, p2.x, p2.y, p2.z);
                }
                else if ((first & 0x00000002) == 0x00000002 && (first & 0xff000000) != 0)
                {
                    var count = (first & 0xff000000) >> 24;
                    Console.WriteLine("Found a face chunk with {0} faces", count);
                    reader.BaseStream.Position += 28;

                    for(int i = 0; i < count; i++)
                    {
                        ushort index;
                        ArrayList indices = new System.Collections.ArrayList();
                        for (int j = 0; j < 4; j++)
                        {
                            index = reader.ReadUInt16();
                            if (index != 0xffff)
                            {
                                indices.Add(index + 1 + v);
                            }
                        };

                        reader.ReadUInt16();

                        facesBuffer += String.Format("f {0}\n", string.Join(" ", indices.ToArray()));
                        Console.WriteLine("{0}: {1}", i, string.Join(",", indices.ToArray()));
                    }

                    if (reader.BaseStream.Position % 16 != 0)
                    {
                        reader.BaseStream.Position += 16 - (reader.BaseStream.Position % 16);
                    }
                }
                else if (first == 0x3468e148)
                {
                    reader.BaseStream.Position -= 4;
                    long startPos = reader.BaseStream.Position;
                    ReadSection2(reader);
                    reader.BaseStream.Position = startPos + section2.nextSectionRelativeOffset;
                    ReadSection3(reader);

                    if (section3.u3 == 0x00)
                    {
                        reader.BaseStream.Position = startPos + section2.nextSectionRelativeOffset + section3.nextSectionRelativeOffset;

                        section4 = new Section4();
                        ReadSection4(reader);

                        reader.BaseStream.Position = startPos + section2.nextSectionRelativeOffset + section3.nextSectionRelativeOffset + section4.nextSectionOffset;

                        section5 = new Section5[section4.section5count];
                        ReadSection5Entries(reader);

                        reader.BaseStream.Position += 64;
                        Console.WriteLine("------------------------------------------");
                    } else if (section3.u3 == 0x06)
                    {
                        reader.BaseStream.Position = startPos + section2.nextSectionRelativeOffset + section3.nextSectionRelativeOffset;

                        reader.BaseStream.Position += 16;

                        var offset = reader.ReadUInt32();

                        reader.BaseStream.Position += offset - 20;

                        Console.WriteLine("------------------------------------------");
                    } else
                    {
                        Console.WriteLine("Found a chunk we didn't understand. Bailing out.");
                        break;
                    }
                }
                else
                {
                    reader.BaseStream.Position += 44;

                    for (int i = 0; i < first - 1; i++)
                    {
                        var p1 = new Vector3();
                        p1.x = reader.ReadSingle();
                        p1.y = reader.ReadSingle();
                        p1.z = reader.ReadSingle();
                        reader.BaseStream.Position += 4;

                        file.WriteLine("v {0} {1} {2}", p1.x, p1.y, p1.z);
                        v++;
                        Console.WriteLine("p1: {0} {1} {2}", p1.x, p1.y, p1.z);
                    }

                    file.Write(facesBuffer);
                    facesBuffer = "";
                }
            }
        }
    }
}