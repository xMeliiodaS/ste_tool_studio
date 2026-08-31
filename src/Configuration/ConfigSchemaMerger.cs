using Newtonsoft.Json.Linq;
using System;
using System.Linq;
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
                JToken? existingValue = user[prop.Name];

                if (existingValue == null)                // only add missing keys
                {
                    user[prop.Name] = prop.Value.DeepClone();
                    changed = true;
                    continue;
                }

                // Add newly introduced nested fields without touching existing user values.
                if (existingValue is JObject existingObject &&
                    prop.Value is JObject defaultObject)
                {
                    foreach (var nestedProperty in defaultObject.Properties())
                    {
                        if (existingObject[nestedProperty.Name] == null)
                        {
                            existingObject[nestedProperty.Name] = nestedProperty.Value.DeepClone();
                            changed = true;
                        }
                    }
                }
            }
            return changed;
        }

        /// <summary>
        /// Makes the user's cycle_* keys an exact mirror of the default:
        /// adds missing cycles, overwrites changed ones, and REMOVES cycles
        /// that no longer exist in the default. Non-cycle keys are untouched.
        /// </summary>
        /// <returns>True if anything changed (caller should persist); otherwise false.</returns>
        public static bool SyncCyclesToDefault(JObject user, JObject defaults)
        {
            if (user == null || defaults == null) return false;
            bool changed = false;
            const string prefix = "cycle_";

            var defaultCycleNames = defaults.Properties()
                .Where(p => p.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Name)
                .ToList();

            // 1) Remove user cycles that no longer exist in the default.
            var staleUserCycles = user.Properties()
                .Where(p => p.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Name)
                .Where(name => !defaultCycleNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                .ToList();
            foreach (var name in staleUserCycles)
            {
                user.Remove(name);
                changed = true;
            }

            // 2) Add or overwrite every default cycle so values always match the default.
            foreach (var name in defaultCycleNames)
            {
                var desired = defaults[name];
                if (!JToken.DeepEquals(user[name], desired))
                {
                    user[name] = desired.DeepClone();
                    changed = true;
                }
            }

            return changed;
        }
    }
}
