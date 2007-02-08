/* --------------------------------------------------------------------------

   Copyright (C) 2004 Sean Cier
   Copyright (C) 2000 Robert Kaye
   Copyright (C) Relatable

   This library is free software; you can redistribute it and/or
   modify it under the terms of the GNU Lesser General Public
   License as published by the Free Software Foundation; either
   version 2.1 of the License, or (at your option) any later version.

   This library is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
   Lesser General Public License for more details.

   You should have received a copy of the GNU Lesser General Public
   License along with this library; if not, write to the Free Software
   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

----------------------------------------------------------------------------*/
namespace musicbrainz {

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

public class TRM {
  private static readonly Encoding ASCII_ENCODING = new ASCIIEncoding();
  private static readonly Encoding UTF8_ENCODING  = new UTF8Encoding();

  private IntPtr trmObject;

  [DllImport("musicbrainz")]
  private static extern IntPtr trm_New();
  [DllImport("musicbrainz")]
  private static extern void trm_Delete(IntPtr o);

  public TRM() {
    trmObject = trm_New();
  }

  ~TRM() {
    trm_Delete(trmObject);
  }

  public static byte[] ToASCII(String s) {
    if (s == null) {
      return null;
    }
    int len = ASCII_ENCODING.GetByteCount(s);
    byte[] result = new byte[len];
    ASCII_ENCODING.GetBytes(s, 0, s.Length, result, 0);
    result[len-1] = 0;
    return result;
  }

  public static String FromASCII(byte[] bytes) {
    if (bytes == null) {
      return null;
    }
    int len = 0;
    while ((len < bytes.Length) && (bytes[len] != 0)) {
      len++;
    }
    return ASCII_ENCODING.GetString(bytes, 0, len);
  }

  public static byte[] ToUTF8(String s) {
    if (s == null) {
      return null;
    }
    int len = UTF8_ENCODING.GetByteCount(s);
    byte[] result = new byte[len];
    UTF8_ENCODING.GetBytes(s, 0, s.Length, result, 0);
    result[len-1] = 0;
    return result;
  }

  public static String FromUTF8(byte[] bytes) {
    if (bytes == null) {
      return null;
    }
    int len = 0;
    while ((len < bytes.Length) && (bytes[len] != 0)) {
      len++;
    }
    return UTF8_ENCODING.GetString(bytes, 0, len);
  }

  [DllImport("musicbrainz")]
  private static extern void trm_SetProxy(IntPtr o,
                                          byte[] proxyAddr,
                                          short proxyPort);
  public void SetProxy(String proxyAddr, short proxyPort) {
    trm_SetProxy(trmObject, ToUTF8(proxyAddr), proxyPort);
  }

  [DllImport("musicbrainz")]
  private static extern int trm_SetPCMDataInfo(IntPtr o,
                                               int samplesPerSecond,
                                               int numChannels,
                                               int bitsPerSample);
  public bool SetPCMDataInfo(int samplesPerSecond,
                             int numChannels,
                             int bitsPerSample) {
    int result = trm_SetPCMDataInfo(trmObject,
                                    samplesPerSecond,
                                    numChannels,
                                    bitsPerSample);
    return (result != 0);
  }

  [DllImport("musicbrainz")]
  private static extern void trm_SetSongLength(IntPtr o,
                                               long seconds);
  public void SetSongLength(long seconds) {
    trm_SetSongLength(trmObject, seconds);
  }

  [DllImport("musicbrainz")]
  private static extern int trm_GenerateSignature(IntPtr o,
                                                  byte[] data,
                                                  int size);
  public bool GenerateSignature(byte[] data) {
    return GenerateSignature(data, data.Length);
  }
  public bool GenerateSignature(byte[] data, int size) {
    int result = trm_GenerateSignature(trmObject, data, size);
    return (result != 0);
  }


  [DllImport("musicbrainz")]
  private static extern int trm_FinalizeSignature(IntPtr o,
                                                  byte[] signature,
                                                  byte[] collectionID);
  public bool FinalizeSignature(out byte[] signature) {
    return FinalizeSignature(null, out signature);
  }
  public bool FinalizeSignature(String collectionID, out byte[] signature) {
    // Create a MusicBrainz instance for the sole purpose of ensuring
    // a session is created and WSA_Init is called if neccessary.
    // Calls to WSA_Init are nestable, and cleanup will occur automatically
    // after this instance goes out-of-scope.
    MusicBrainz mb = new MusicBrainz();

    signature = new byte[17];
    int result =
      trm_FinalizeSignature(trmObject,
                            signature,
                            (collectionID == null) ? null :
                             ToUTF8(collectionID));

    // Unlike most mb_* methods, trm_FinalizeSignature returns 0 on success
    return (result == 0);
  }

  [DllImport("musicbrainz")]
  private static extern int trm_ConvertSigToASCII(IntPtr o,
                                                  byte[] signature,
                                                  byte[] ascii_sig);
  public void ConvertSigToASCII(byte[] signature, out String ascii_sig) {
    byte[] ascii_sigNative = new byte[37];
    trm_ConvertSigToASCII(trmObject, signature, ascii_sigNative);
    ascii_sig = FromASCII(ascii_sigNative);
  }
}

}
