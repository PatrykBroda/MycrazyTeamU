using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class CommandLineReaderVK
{
    public static string[] GetCommandLineArgs()
    {
        return Environment.GetCommandLineArgs();
    }

    public static Dictionary<string, string> GetCustomArguments()
    {
        Dictionary<string, string> customArgsDict = new Dictionary<string, string>();
        string[] commandLineArgs = GetCommandLineArgs();
        string[] customArgBuffer;

        foreach (string customArg in commandLineArgs)
        {
            customArgBuffer = customArg.Split('=');
            if (customArgBuffer.Length == 2)
            {
                customArgsDict.Add(customArgBuffer[0], customArgBuffer[1]);
            }
            else
            {
                Debug.LogWarning("CommandLineReader.cs - GetCustomArguments() - The custom argument [" + customArg + "] seem to be malformed.");
            }
        }
        return customArgsDict;
    }

    public static string GetCustomArgument(string argumentName)
    {
        Dictionary<string, string> customArgsDict = GetCustomArguments();

        if (customArgsDict.ContainsKey(argumentName))
        {
            return customArgsDict[argumentName];
        }
        else
        {
            //Debug.LogError("CommandLineReader.cs - GetCustomArgument() - Can't retrieve any custom argument named [" + argumentName + "] in the command line [" + GetCommandLine() + "].");
            return "";
        }
    }

    public static string CreateMD5(string input)
    {
        // Use input string to calculate MD5 hash
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            //return Convert.ToHexString(hashBytes); // .NET 5 +
                                                   // Convert the byte array to hexadecimal string prior to .NET 5
            StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
