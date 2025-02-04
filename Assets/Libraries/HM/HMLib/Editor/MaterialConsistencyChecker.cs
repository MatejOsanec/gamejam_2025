using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;

public class MaterialConsistencyChecker {

    public enum KeywordType {
        Toggle,
        Enum
    }

    public struct KeywordInconsistency {

        public string text;
        public Material material;
        public int propertyNameId;
        public string currentKeyword;
        public bool currentlyOn;
        public string enumKeyword;
        public KeywordType keywordType;
        public int currentKeywordEnumId;
    }

    // great tool for testing this » http://regexstorm.net/tester
    private const string kToggleRegexTest = @"Toggle\w*\((\w*)";
    private const string kEnumRegexTest = @"\((((\w|\s)*),?\s?)+\)";
    private readonly Regex _toggleRegex;
    private readonly Regex _enumRegex;

    public MaterialConsistencyChecker() {

        _toggleRegex = new Regex(kToggleRegexTest, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        _enumRegex = new Regex(kEnumRegexTest, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    public bool Check(Material material, ref List<KeywordInconsistency> inconsistenciesList) {

        var shader = material.shader;
        int inconsistencies = 0;

        for (int propertyId = 0; propertyId < shader.GetPropertyCount(); propertyId++) {

            if (shader.GetPropertyType(propertyId) != ShaderPropertyType.Float) {
                continue;
            }

            foreach (var attr in shader.GetPropertyAttributes(propertyId)) {

                if (attr.Contains("Toggle") && !attr.Contains("FloatToggle")) {
                    var result = _toggleRegex.Match(attr);
                    if (!result.Success) {
                        continue;
                    }

                    string keyword = result.Groups[1].Value;
                    bool keywordValue = material.IsKeywordEnabled(keyword);
                    bool toggleValue = Convert.ToBoolean(material.GetFloat(shader.GetPropertyNameId(propertyId)));
                    bool defaultValue = Convert.ToBoolean(shader.GetPropertyDefaultFloatValue(propertyId));

                    if (toggleValue != keywordValue) {
                        var inconsistency = new KeywordInconsistency {
                            text = $"> Toggle {shader.GetPropertyDescription(propertyId)} is {toggleValue} while Keyword {keyword} is {keywordValue} (default: {defaultValue})\n",
                            material = material,
                            propertyNameId = shader.GetPropertyNameId(propertyId),
                            currentKeyword = keyword,
                            keywordType = KeywordType.Toggle,
                            currentlyOn = keywordValue
                        };

                        inconsistenciesList.Add(inconsistency);
                        inconsistencies++;
                    }

                    break;
                }

                else if (attr.Contains("Enum") && !attr.Contains("FloatEnum")) {
                    var result = _enumRegex.Match(attr);
                    if (result.Success) {

                        int propertyNameId = shader.GetPropertyNameId(propertyId);
                        string propertyName = shader.GetPropertyName(propertyId);

                        int ignoreLast = 0;
                        int ignoreFirst = 0;

                        if (attr.Contains("ShowIf")) {

                            // If we use more conditions, looking like this [2, enum1, enum2, condition1, condition2, condition3, condition4, ...]
                            if (int.TryParse(result.Groups[2].Captures[0].Value, out int numberOfEnums)) {
                                ignoreLast = result.Groups[2].Captures.Count - numberOfEnums - 2;
                                ignoreFirst = 1;
                            }

                            else {
                                ignoreLast = 1;
                            }
                        }

                        int keywordCount = result.Groups[2].Captures.Count - ignoreLast - 1 - ignoreFirst;
                        string[] keywords = new string[keywordCount];
                        //bool[] keywordValues = new bool[keywordCount];
                        int currentEnumId = (int)material.GetFloat(propertyNameId);
                        int currentKeywordEnumId = -999;
                        string currentKeyword = "None";

                        // First create array of keywords
                        for (int optionId = 0; optionId < keywordCount; optionId++) {

                            keywords[optionId] = $"{propertyName.ToUpper()}_{result.Groups[2].Captures[optionId + ignoreFirst].Value.ToUpper()}";

                            if (keywords[optionId].Contains(" ")) {
                                keywords[optionId] = keywords[optionId].Replace(" ", "_");
                            }

                            if (material.IsKeywordEnabled(keywords[optionId])) {
                                currentKeywordEnumId = optionId;
                                currentKeyword = keywords[optionId];
                            }
                        }

                        string currentEnumKeyword = keywords[currentEnumId];

                        if (currentKeywordEnumId != currentEnumId) {
                            if (currentEnumId == 0 && currentKeyword == "None") {
                                break;
                            }

                            var inconsistency = new KeywordInconsistency {
                                text = $"> Enum {propertyName} should set {currentEnumKeyword} but keyword is {currentKeyword}\n",
                                material = material,
                                keywordType = KeywordType.Enum,
                                propertyNameId = shader.GetPropertyNameId(propertyId),
                                enumKeyword = keywords[currentEnumId],
                                currentKeyword = currentKeyword,
                                currentKeywordEnumId = currentKeywordEnumId
                            };

                            inconsistenciesList.Add(inconsistency);
                            inconsistencies++;
                        }

                        break;
                    }
                }
            }
        }

        return inconsistencies == 0;
    }
}
