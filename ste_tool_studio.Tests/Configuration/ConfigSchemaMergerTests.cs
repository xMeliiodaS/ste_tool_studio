using Newtonsoft.Json.Linq;
using ste_tool_studio.Configuration;
using Xunit;

namespace ste_tool_studio.Tests.Configuration
{
    /// <summary>
    /// Locks in the config schema-merge behavior before release.
    /// Prevents regressions where published schema changes fail to reach
    /// existing APPDATA configs, or where a merge clobbers user/preset data.
    /// </summary>
    public class ConfigSchemaMergerTests
    {
        // --- Core behavior -------------------------------------------------

        [Fact]
        public void MergeMissingKeys_AddsMissingKey_AndReportsChanged()
        {
            var user = JObject.Parse(@"{ ""url"": ""https://x"" }");
            var defaults = JObject.Parse(@"{ ""url"": """", ""Normalized_protocol"": """" }");

            bool changed = ConfigSchemaMerger.MergeMissingKeys(user, defaults);

            Assert.True(changed);
            Assert.True(user.ContainsKey("Normalized_protocol"));
            Assert.Equal(string.Empty, user["Normalized_protocol"].ToString());
        }

        [Fact]
        public void MergeMissingKeys_DoesNotOverwriteExistingUserValue()
        {
            var user = JObject.Parse(@"{ ""std_name"": ""Ablation STD"" }");
            var defaults = JObject.Parse(@"{ ""std_name"": """" }");

            bool changed = ConfigSchemaMerger.MergeMissingKeys(user, defaults);

            Assert.False(changed);                                   // nothing added
            Assert.Equal("Ablation STD", user["std_name"].ToString()); // value preserved
        }

        [Fact]
        public void MergeMissingKeys_NoMissingKeys_ReturnsFalse()
        {
            var user = JObject.Parse(@"{ ""a"": ""1"", ""b"": ""2"" }");
            var defaults = JObject.Parse(@"{ ""a"": """", ""b"": """" }");

            bool changed = ConfigSchemaMerger.MergeMissingKeys(user, defaults);

            Assert.False(changed);
        }

        // --- Nested / preset objects --------------------------------------

        [Fact]
        public void MergeMissingKeys_CopiesNestedObjectByValue_NotReference()
        {
            var user = JObject.Parse(@"{ ""url"": """" }");
            var defaults = JObject.Parse(
                @"{ ""url"": """", ""cycle_1"": { ""protocol_number"": ""2473596580"", ""test_plan"": ""0685"" } }");

            bool changed = ConfigSchemaMerger.MergeMissingKeys(user, defaults);

            Assert.True(changed);
            Assert.Equal("2473596580", user["cycle_1"]["protocol_number"].ToString());
            Assert.Equal("0685", user["cycle_1"]["test_plan"].ToString());

            // Prove it's a deep clone: mutating the default must not affect the merged user copy.
            defaults["cycle_1"]["protocol_number"] = "CHANGED";
            Assert.Equal("2473596580", user["cycle_1"]["protocol_number"].ToString());
        }

        [Fact]
        public void MergeMissingKeys_PreservesExistingPresetValues_AndAddsMissingFields()
        {
            var user = JObject.Parse(
                @"{ ""cycle_1"": { ""protocol_number"": ""USER_VAL"", ""test_plan"": ""9999"" } }");
            var defaults = JObject.Parse(
                @"{ ""cycle_1"": { ""protocol_number"": ""2473596580"", ""report_number"": """", ""test_plan"": ""0685"", ""stx_number"": """", ""prepared_by"": """" } }");

            bool changed = ConfigSchemaMerger.MergeMissingKeys(user, defaults);

            Assert.True(changed);
            Assert.Equal("USER_VAL", user["cycle_1"]["protocol_number"]?.ToString());
            Assert.Equal("9999", user["cycle_1"]["test_plan"]?.ToString());
            Assert.NotNull(user["cycle_1"]["report_number"]);
            Assert.NotNull(user["cycle_1"]["stx_number"]);
            Assert.NotNull(user["cycle_1"]["prepared_by"]);
        }

        // --- Edge cases ----------------------------------------------------

        [Fact]
        public void MergeMissingKeys_EmptyUserConfig_GetsFullSchema()
        {
            var user = new JObject();
            var defaults = JObject.Parse(
                @"{ ""config_version"": 2, ""url"": """", ""std_name"": """" }");

            bool changed = ConfigSchemaMerger.MergeMissingKeys(user, defaults);

            Assert.True(changed);
            Assert.Equal(3, user.Properties().Count());
            Assert.Equal(2, (int)user["config_version"]);
        }

        [Fact]
        public void MergeMissingKeys_NullArguments_ReturnFalse_AndDoNotThrow()
        {
            Assert.False(ConfigSchemaMerger.MergeMissingKeys(null, new JObject()));
            Assert.False(ConfigSchemaMerger.MergeMissingKeys(new JObject(), null));
            Assert.False(ConfigSchemaMerger.MergeMissingKeys(null, null));
        }
    }
}
