using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ApplicationData 
{
    Dictionary<string, Action<string>> m_CommandDictionary = new Dictionary<string, Action<string>>();
    const string k_IPCmd = "ip";
    const string k_PortCmd = "port";
    const string k_QueryPortCmd = "queryPort";

    public static bool IsServerUnitTest;

    public static string IP()
    {
        return PlayerPrefs.GetString(k_IPCmd);
    }

    public static int Port()
    {
        return PlayerPrefs.GetInt(k_PortCmd);
    }

    public static int QPort()
    {
        return PlayerPrefs.GetInt(k_QueryPortCmd);
    }

    public ApplicationData()
    {
        SetIP("127.0.0.1");
        SetPort("7777");
        SetQueryPort("7787");
        m_CommandDictionary["-" + k_IPCmd] = SetIP;
        m_CommandDictionary["-" + k_PortCmd] = SetPort;
        m_CommandDictionary["-" + k_QueryPortCmd] = SetQueryPort;
        ProcessCommandLinearguments(Environment.GetCommandLineArgs());
    }
    void ProcessCommandLinearguments(string[] args)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Launch Args: ");
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = "";
            if (i + 1 < args.Length) // if we are evaluating the last item in the array, it must be a flag
                nextArg = args[i + 1];

            if (EvaluatedArgs(arg, nextArg))
            {
                sb.Append(arg);
                sb.Append(" : ");
                sb.AppendLine(nextArg);
                i++;
            }
        }
        Debug.Log(sb);
    }
    bool EvaluatedArgs(string arg, string nextArg)
    {
        if (!IsCommand(arg))
            return false;
        if (IsCommand(nextArg))
        {
            return false;
        }
        m_CommandDictionary[arg].Invoke(nextArg);
        return true;
    }
    bool IsCommand(string arg)
    {
        return !string.IsNullOrEmpty(arg) && m_CommandDictionary.ContainsKey(arg) && arg.StartsWith("-");
    }
    void SetIP(string ipArgument)
    {
        PlayerPrefs.SetString(k_IPCmd, ipArgument);
    }

    void SetPort(string portArgument)
    {
        if (int.TryParse(portArgument, out int parsedPort))
        {
            PlayerPrefs.SetInt(k_PortCmd, parsedPort);
        }
        else
        {
            Debug.LogError($"{portArgument} does not contain a parseable port!");
        }
    }

    void SetQueryPort(string qPortArgument)
    {
        if (int.TryParse(qPortArgument, out int parsedQPort))
        {
            PlayerPrefs.SetInt(k_QueryPortCmd, parsedQPort);
        }
        else
        {
            Debug.LogError($"{qPortArgument} does not contain a parseable query port!");
        }
    }
}
