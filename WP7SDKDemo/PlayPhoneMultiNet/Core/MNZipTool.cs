//
//  MNZipTool.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.IO;
using Ionic.Zip;

namespace PlayPhone.MultiNet.Core
 {
  public static class MNZipTool
   {
    public static byte[] GetFileDataFromArchiveStream (Stream sourceStream, string fileName)
     {
      byte[] data = null;

      ReadOptions readOptions = new ReadOptions();

      readOptions.Encoding = System.Text.Encoding.UTF8;

      try
       {
        using (ZipFile zip = ZipFile.Read(sourceStream,readOptions))
         {
          MemoryStream dataStream = new MemoryStream();
          ZipEntry     entry      = zip[fileName];

          entry.Extract(dataStream);

          data = dataStream.ToArray();
         }
       }
      catch (Exception e)
       {
        MNDebug.debug("zip extraction failed: " + e.ToString());
       }

      return data;
     }
   }
 }
