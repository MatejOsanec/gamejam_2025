using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
using System;

// This class helps to serialize basic data structure in human readable format:
// key0=value0
// key1=value1
// ...
//
// Only float, int and bool values are supported.
public class ConfigSerializer {

	public static void SaveConfig(object config, string filePath) {

        List<string> lines = new List<string>();

		Type configType = config.GetType();
		FieldInfo[] fields = configType.GetFields();		
        
        for (int i = 0; i < fields.Length; i++) {
            Type fieldType = fields[i].FieldType;
            // Save only basic types
            if (fieldType == typeof(float) || fieldType == typeof(int) || fieldType == typeof(bool)) {
                lines.Add(fields[i].Name + "=" + fields[i].GetValue(config).ToString());
            }
            else if (fieldType == typeof(string)) {
                lines.Add(fields[i].Name + "=\"" + fields[i].GetValue(config) + "\"");
            }
		}
        File.WriteAllLines(filePath, lines.ToArray());
    }        

    public static bool LoadConfig(object config, string filePath) {

        try {            
            var lines = System.IO.File.ReadAllLines(filePath);
            foreach (var line in lines) {
                var split = line.Split('=');
                if (split.Length == 2) {
                    var key = split[0];
                    var field = config.GetType().GetField(key);
					if (field != null) {
						Type fieldType = field.FieldType;
						if (fieldType == typeof(float)) {
							field.SetValue(config, float.Parse(split[1]));
						}
						else if (fieldType == typeof(int)) {
							field.SetValue(config, int.Parse(split[1]));
						}
						else if (fieldType == typeof(bool)) {
							if (split[1].Length == 1) {
								field.SetValue(config, split[1] == "1");
							}
							else {
								field.SetValue(config, Convert.ToBoolean(split[1]));
							}
						}
                        else if (fieldType == typeof(string)) {
                            field.SetValue(config, split[1].Trim('"'));
                        } 
					}               
                }
            }      
        }
        catch { 
            return false;
        }

        return true;
    }
}
