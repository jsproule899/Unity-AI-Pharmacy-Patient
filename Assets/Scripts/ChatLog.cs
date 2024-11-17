using System;
using System.Collections.Generic;
using System.IO;
using OpenAI;
using UnityEngine;
using JSBrowserUtilities;

public class ChatLog
{
    private string filepath = Application.persistentDataPath + "/chatlog.txt";

    public ChatLog(string filename)
    {
#if UNITY_EDITOR
        this.filepath = Application.persistentDataPath + "/" + filename + ".txt";

        if (File.Exists(filepath))
        {
            // File.Delete(filepath);
            Debug.Log("File already exists, creating file with datetime prefix");
            string timestamp = DateTime.Now.ToString("yyyyddMM_HHmmss");

            this.filepath = Application.persistentDataPath + $"/{filename}_{timestamp}.txt";
        }
        File.CreateText(filepath).Close();

#elif !UNITY_EDITOR && UNITY_WEBGL
        BrowserHelper.JS_TextFile_CreateObject();
#endif
    }

    public bool WriteConfigToChatLog()
    {
        try
        {
#if UNITY_EDITOR
            using (StreamWriter writer = File.AppendText(filepath))
            {
                writer.WriteLine("Student: " + Config.Student.Id);
                writer.WriteLine("----------Scenario-----------");
                writer.WriteLine(Config.Scenario.Context);
                //TODO - add more scenario identifiers.

            }
#elif !UNITY_EDITOR && UNITY_WEBGL
                BrowserHelper.JS_TextFile_Append("Student: " + Config.Student.Id);
                BrowserHelper.JS_TextFile_Append("----------Scenario-----------");
                BrowserHelper.JS_TextFile_Append(Config.Scenario.Context);
#endif

            return true;
        }
        catch (Exception err)
        {
            TextWriter errorWriter = Console.Error;
            errorWriter.WriteLine(err.Message);
            return false;
        }
    }

    public bool WriteMessagesToChatLog(List<ChatMessage> chatHistory)
    {
        try
        {
#if UNITY_EDITOR
            using (StreamWriter writer = File.AppendText(filepath))
            {

                foreach (ChatMessage message in chatHistory)
                {
                    switch (message.Role)
                    {
                        case "assistant":
                            writer.Write("Patient: ");
                            writer.WriteLine(message.Content);
                            break;
                        case "user":
                            writer.Write("Pharmacist: ");
                            writer.WriteLine(message.Content);
                            break;
                        default:
                            // writer.WriteLine("----------Scenario prompt-----------");
                            // writer.WriteLine(message.Content);
                            writer.WriteLine("-----------Conversation------------");
                            break;
                    }
                }

            }
#elif !UNITY_EDITOR && UNITY_WEBGL
            foreach (ChatMessage message in chatHistory)
                {
                    switch (message.Role)
                    {
                        case "assistant":                            
                             BrowserHelper.JS_TextFile_Append("Patient: " + message.Content);
                            break;
                        case "user":
                            BrowserHelper.JS_TextFile_Append("Pharmacist: " + message.Content);
                            break;
                        default:                            
                            BrowserHelper.JS_TextFile_Append("-----------Conversation------------");
                            break;
                    }
                }
#endif
            return true;

        }
        catch (Exception err)
        {
            TextWriter errorWriter = Console.Error;
            errorWriter.WriteLine(err.Message);
            return false;
        }

    }


    public bool WriteOutcomeToChatLog(string outcome, string justification = null, string medication = null, string advice = null)
    {
        try
        {
#if UNITY_EDITOR
            using (StreamWriter writer = File.AppendText(filepath))
            {
                writer.WriteLine("----------Outcome-----------");
                writer.WriteLine($"The student has chosen to {outcome.ToUpper()} the patient");
                if (justification != null && justification.Length > 0)
                {
                    writer.WriteLine("----------Justification-----------");
                    writer.WriteLine(justification);
                }

                if (medication != null && medication.Length > 0)
                {
                    writer.WriteLine("----------Presribed Medication-----------");
                    writer.WriteLine(medication);
                }

                if (advice != null && advice.Length > 0)
                {
                    writer.WriteLine("----------Medication Advice-----------");
                    writer.WriteLine(advice);
                }
            }
#elif !UNITY_EDITOR && UNITY_WEBGL
BrowserHelper.JS_TextFile_Append("----------Outcome-----------");
BrowserHelper.JS_TextFile_Append($"The student has chosen to {outcome.ToUpper()} the patient");
                if (justification != null && justification.Length > 0)
                {
                   BrowserHelper.JS_TextFile_Append("----------Justification-----------");
                    BrowserHelper.JS_TextFile_Append(justification);
                }

                if (medication != null && medication.Length > 0)
                {
                    BrowserHelper.JS_TextFile_Append("----------Presribed Medication-----------");
                    BrowserHelper.JS_TextFile_Append(medication);
                }

                if (advice != null && advice.Length > 0)
                {
                    BrowserHelper.JS_TextFile_Append("----------Medication Advice-----------");
                    BrowserHelper.JS_TextFile_Append(advice);
                }

            
#endif
            return true;
        }
        catch (Exception err)
        {
            TextWriter errorWriter = Console.Error;
            errorWriter.WriteLine(err.Message);
            return false;
        }
    }



}