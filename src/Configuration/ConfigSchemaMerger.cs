using Newtonsoft.Json.Linq;

namespace ste_tool_studio.Configuration
{
    /// <summary>
    /// Pure, I/O-free schema-merge logic for config.json.
    /// Extracted from AppConfiguration so it can be unit tested without touching APPDATA
    /// or the bundled default file. Keeps merge behavior auditable and deterministic.
    /// </summary>
    public static class ConfigSchemaMerger
    {
        /// <summary>
        /// Adds any keys present in <paramref name="defaults"/> that are missing from
        /// <paramref name="user"/>. Never overwrites existing user/preset values.
        /// </summary>
        /// <param name="user">The user's current config (from APPDATA). Mutated in place.</param>
        /// <param name="defaults">The bundled default/template config.</param>
        /// <returns>True if one or more keys were added (caller should persist); otherwise false.</returns>
        public static bool MergeMissingKeys(JObject user, JObject defaults)
        {
            if (user == null || defaults == null) return false;

            bool changed = false;
            foreach (var prop in defaults.Properties())
            {
                if (user[prop.Name] == null)              // only add missing keys
                {
                    user[prop.Name] = prop.Value.DeepClone();
                    changed = true;
                }
            }
            return changed;
        }
    }
}
