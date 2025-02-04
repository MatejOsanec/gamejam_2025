namespace BGLib.ShaderInspector.Tests {

    using System.Reflection;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;

    public class ShaderInspectorTests {

        [Test]
        public void DisplayFilter_KeywordsFilterWorks() {

            var property = new MaterialProperty();
            // We are using ShaderInspectorTestShader because IsKeywordEnabled does not work with keywords no defined in the shader
            // We could use Material.shaderKeywords but having a test shader is closer to actual usage
            var shader = Shader.Find("Hidden/ShaderInspectorTestShader");
            var material = new Material(shader);
            Object[] materials = { material };
            MaterialProperty[] properties = { property };

            FieldInfo targetsField = typeof(MaterialProperty).GetField("m_Targets", BindingFlags.NonPublic | BindingFlags.Instance);
            targetsField.SetValue(property, materials);

            Assert.IsTrue(properties.HasMaximumNKeywords(maximumCount: 0, "A", "B"));
            Assert.IsFalse(properties.HasAtLeastNKeywords(requiredCount: 1, "A", "B"));

            material.EnableKeyword("A");
            Assert.True(properties.HasAtLeastNKeywords(requiredCount: 0, "A", "B"));
            Assert.True(properties.HasAtLeastNKeywords(requiredCount: 1, "A", "B"));
            Assert.IsFalse(properties.HasMaximumNKeywords(maximumCount: 0, "A", "B"));
            Assert.True(properties.HasMaximumNKeywords(maximumCount: 1, "A", "B"));

            material.EnableKeyword("B");
            Assert.True(properties.HasAtLeastNKeywords(requiredCount: 1, "A", "B"));
            Assert.True(properties.HasAtLeastNKeywords(requiredCount: 2, "A", "B"));
            Assert.False(properties.HasMaximumNKeywords(maximumCount: 1, "A", "B"));
            Assert.True(properties.HasMaximumNKeywords(maximumCount: 2, "A", "B"));


            material.EnableKeyword("NotExistOnShader");
            Assert.False(properties.HasAtLeastNKeywords(requiredCount: 3, "A", "B", "NotExistOnShader"));

            material.EnableKeyword("C");
            Assert.False(properties.HasAtLeastNKeywords(requiredCount: 3, "A", "B", "NotExistOnShader"));
            Assert.True(properties.HasAtLeastNKeywords(requiredCount: 3, "A", "B", "C", "NotExistOnShader"));
            Assert.True(properties.HasMaximumNKeywords(maximumCount: 2, "A", "B", "NotExistOnShader"));
            Assert.False(properties.HasMaximumNKeywords(maximumCount: 2, "A", "B", "C", "NotExistOnShader"));

            Object.DestroyImmediate(material);
        }
    }
}
