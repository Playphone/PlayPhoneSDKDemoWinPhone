//
//  MNVarStorage.cs
//  MultiNet client
//
//  Copyright 2012 PlayPhone. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;

namespace PlayPhone.MultiNet.Core
 {
  public class MNVarStorage
   {
    public MNVarStorage ()
     {
      tempStorage       = new Dictionary<string,string>();
      persistentStorage = new Dictionary<string,string>();

      LoadFromFile();
     }

    public string GetValue (string name)
     {
      string value;

      lock (thisLock)
       {
        if (tempStorage.TryGetValue(name,out value))
         {
          return value;
         }

        if (persistentStorage.TryGetValue(name,out value))
         {
          return value;
         }
        else
         {
          return null;
         }
       }
     }

    public void SetValue (string name, string value)
     {
      Dictionary<string,string> storage;

      lock (thisLock)
       {
        storage = StringHasTempPrefix(name) ? tempStorage : persistentStorage;

        if (value != null)
         {
          storage[name] = value;
         }
        else
         {
          storage.Remove(name);
         }
       }
     }

    public Dictionary<string,string> GetVariablesByMask (string mask)
     {
      string[] masks = { mask };

      return GetVariablesByMasks(masks);
     }

    public Dictionary<string,string> GetVariablesByMasks (string[] masks)
     {
      Dictionary<string,string> result      = null;
      Dictionary<string,string> tempResults = null;

      lock (thisLock)
       {
        result      = GetVariablesByMasks(masks,persistentStorage);
        tempResults = GetVariablesByMasks(masks,tempStorage);
       }

      foreach (var entry in tempResults)
       {
        result[entry.Key] = entry.Value;
       }

      return result;
     }

    private static Dictionary<string,string> GetVariablesByMasks (string[] masks, Dictionary<string,string> storage)
     {
      Dictionary<string,string> result = new Dictionary<string,string>();

      foreach (string mask in masks)
       {
        string maskPrefix = GetMaskPrefix(mask);

        if (maskPrefix != null)
         {
          if (maskPrefix.Length > 0)
           {
            foreach (var entry in storage)
             {
              if (entry.Key.StartsWith(maskPrefix))
               {
                result[entry.Key] = entry.Value;
               }
             }
           }
          else
           {
            foreach (var entry in storage)
             {
              result[entry.Key] = entry.Value;
             }

            return result;
           }
         }
        else
         {
          string val;

          if (storage.TryGetValue(mask,out val))
           {
            result[mask] = val;
           }
         }
       }

      return result;
     }

    public void RemoveVariablesByMask (string mask)
     {
      string[] masks = { mask };

      RemoveVariablesByMasks(masks);
     }

    public void RemoveVariablesByMasks (string[] masks)
     {
      lock (thisLock)
       {
        RemoveVariablesByMasks(masks,persistentStorage);
        RemoveVariablesByMasks(masks,tempStorage);
       }
     }

    private static void RemoveVariablesByMasks (string[] masks, Dictionary<string,string> storage)
     {
      LinkedList<string> keysToRemove = new LinkedList<string>();

      foreach (string mask in masks)
       {
        string maskPrefix = GetMaskPrefix(mask);

        if (maskPrefix != null)
         {
          if (maskPrefix.Length > 0)
           {
            foreach (var entry in storage)
             {
              if (entry.Key.StartsWith(maskPrefix))
               {
                keysToRemove.AddLast(entry.Key);
               }
             }
           }
          else
           {
            storage.Clear();

            return;
           }
         }
        else
         {
          storage.Remove(mask);
         }
       }

      foreach (string key in keysToRemove)
       {
        storage.Remove(key);
       }
     }

    public void WriteToFile ()
     {
      lock (thisLock)
       {
        IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

        try
         {
          using (var varStorageFileStream = new IsolatedStorageFileStream(VAR_STORAGE_FILE_NAME,FileMode.OpenOrCreate,store))
           {
            using (var varStorageFileWriter = new StreamWriter(varStorageFileStream))
             {
              foreach (var entry in persistentStorage)
               {
                varStorageFileWriter.Write(entry.Key);
                varStorageFileWriter.Write('=');
                varStorageFileWriter.WriteLine(entry.Value);
               }
             }
           }
         }
        catch (IOException)
         {
         }
        catch (IsolatedStorageException)
         {
         }
       }
     }

    private void LoadFromFile ()
     {
      lock (thisLock)
       {
        persistentStorage.Clear();

        IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

        try
         {
          using (var varStorageFileStream = new IsolatedStorageFileStream(VAR_STORAGE_FILE_NAME,FileMode.Open,store))
           {
            using (var varStorageFileReader = new StreamReader(varStorageFileStream))
             {
              while (!varStorageFileReader.EndOfStream)
               {
                string line = varStorageFileReader.ReadLine().Trim();

                int pos = line.IndexOf('=');

                if (pos >= 0)
                 {
                  persistentStorage[line.Substring(0,pos)] = line.Substring(pos + 1);
                 }
               }
             }
           }
         }
        catch (IOException)
         {
         }
        catch (IsolatedStorageException)
         {
         }
       }
     }

    private static string GetMaskPrefix (String mask)
     {
      if (mask.EndsWith(MASK_WILDCARD_CHAR))
       {
        return mask.Substring(0,mask.Length - MASK_WILDCARD_CHAR.Length);
       }
      else
       {
        return null;
       }
     }

    private static bool StringHasTempPrefix (string str)
     {
      return str.StartsWith(TMP_PREFIX) || str.StartsWith(PROP_PREFIX);
     }

    private const string MASK_WILDCARD_CHAR = "*";
    private const string TMP_PREFIX  = "tmp.";
    private const string PROP_PREFIX = "prop.";

    private static string VAR_STORAGE_FILE_NAME = MNPlatformWinPhone.MULTINET_DATA_DIR + "\\" + "mn_vars.dat";

    private Dictionary<string,string> tempStorage;
    private Dictionary<string,string> persistentStorage;
    private object                    thisLock = new object();
   }
 }
