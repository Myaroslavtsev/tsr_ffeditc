// COPYRIGHT 2013, 2014, 2015 by the Open Rails project.
// 
// This file is part of Open Rails.

using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
//using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace tsr_ffeditc
{
    /// <summary>
    /// Structured Block Reader can read compressed binary or uncompressed unicode files.
    /// Its intended to replace the KujuBinary classes ( which are binary only ).
    /// Every block must be closed with either Skip() or VerifyEndOfBlock()
    /// </summary>
    public abstract class SBR : IDisposable
    {
        //public TokenID ID;
        public string Label;  // First data item may be a label ( usually a 0 byte )

        public static SBR Open(string filename)
        {
            Stream fb = new FileStream(filename, FileMode.Open, FileAccess.Read);

            byte[] buffer = new byte[34];
            fb.Read(buffer, 0, 2);

            bool unicode = (buffer[0] == 0xFF && buffer[1] == 0xFE);  // unicode header

            string headerString;
            if (unicode)
            {
                fb.Read(buffer, 0, 32);
                headerString = System.Text.Encoding.Unicode.GetString(buffer, 0, 16);
            }
            else
            {
                fb.Read(buffer, 2, 14);
                headerString = System.Text.Encoding.ASCII.GetString(buffer, 0, 8);
            }

            // SIMISA@F  means compressed
            // SIMISA@@  means uncompressed
            if (headerString.StartsWith("SIMISA@F"))
            {
                fb = new InflaterInputStream(fb);
            }
            else if (headerString.StartsWith("\r\nSIMISA"))
            {
                // ie us1rd2l1000r10d.s, we are going to allow this but warn
                Console.Error.WriteLine("Improper header in " + filename);
                fb.Read(buffer, 0, 4);
            }
            else if (!headerString.StartsWith("SIMISA@@"))
            {
                throw new System.Exception("Unrecognized header \"" + headerString + "\" in " + filename);
            }

            // Read SubHeader
            string subHeader;
            if (unicode)
            {
                fb.Read(buffer, 0, 32);
                subHeader = System.Text.Encoding.Unicode.GetString(buffer, 0, 16);
            }
            else
            {
                fb.Read(buffer, 0, 16);
                subHeader = System.Text.Encoding.ASCII.GetString(buffer, 0, 8);
            }

            // Select for binary vs text content
            if (subHeader[7] == 't')
            {
                return new UnicodeFileReader(fb, filename, unicode ? Encoding.Unicode : Encoding.ASCII);
            }
            else if (subHeader[7] != 'b')
            {
                throw new System.Exception("Unrecognized subHeader \"" + subHeader + "\" in " + filename);
            }

            // And for binary types, select where their tokens will appear in our TokenID enum
            if (subHeader[5] == 'w')  // and [7] must be 'b'
            {
                return new BinaryFileReader(fb, filename, 300);
            }
            else
            {
                return new BinaryFileReader(fb, filename, 0);
            }
        }

        //public abstract SBR ReadSubBlock();

        /// <summary>
        /// Skip to the end of this block
        /// </summary>
        //public abstract void Skip();
        public abstract string ReadString();

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// Structured unicode text file reader
    /// </summary>
    public class UnicodeFileReader : UnicodeBlockReader
    {
        public UnicodeFileReader(Stream inputStream, string filename, Encoding encoding)
        {
            f = new StreamReader(inputStream, encoding); //new STFReader(inputStream, filename, encoding, false);
        }
    }

    /// <summary>
    /// Structured unicode text file reader
    /// </summary>
    public class UnicodeBlockReader : SBR
    {
        protected StreamReader f;
        
        public override string ReadString() { return f.ReadLine(); }
    }

    /// <summary>
    /// Structured kuju binary file reader
    /// </summary>
    public class BinaryFileReader : BinaryBlockReader
    {
        /// <summary>
        /// Assumes that fb is positioned just after the SIMISA@F header
        /// filename is provided for error reporting purposes
        /// Each block has a token ID.  It's value corresponds to the value of
        /// the TokenID enum.  For some file types, ie .W files, the token value's 
        /// will be offset into the TokenID table by the specified tokenOffset.
        /// </summary>
        /// <param name="fb"></param>
        public BinaryFileReader(Stream inputStream, string filename, int tokenOffset)
        {
            Filename = filename;
            InputStream = new BinaryReader(inputStream);
            TokenOffset = tokenOffset;
        }
    }

    /// <summary>
    /// Structured kuju binary file reader
    /// </summary>
    public class BinaryBlockReader : SBR
    {
        public string Filename;  // for error reporting
        public BinaryReader InputStream;
        public uint RemainingBytes;  // number of bytes in this block not yet read from the stream
        public uint Flags;
        protected int TokenOffset;     // the binaryTokens are offset by this amount, ie for binary world files 

        public override string ReadString()
        {
            ushort count = InputStream.ReadUInt16();
            if (count > 0)
            {
                byte[] b = InputStream.ReadBytes(count * 2);
                string s = System.Text.Encoding.Unicode.GetString(b);
                RemainingBytes -= (uint)(count * 2 + 2);
                return s;
            }
            else
            {
                return "";
            }
        }
    }
}
