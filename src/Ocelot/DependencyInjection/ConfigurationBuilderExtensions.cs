namespace Ocelot.DependencyInjection
{
    using Configuration.File;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Memory;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class ConfigurationBuilderExtensions
    {
        [Obsolete("Please set BaseUrl in ocelot.json GlobalConfiguration.BaseUrl")]
        public static IConfigurationBuilder AddOcelotBaseUrl(this IConfigurationBuilder builder, string baseUrl)
        {
            var memorySource = new MemoryConfigurationSource
            {
                InitialData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("BaseUrl", baseUrl)
                }
            };

            builder.Add(memorySource);

            return builder;
        }

        public static IConfigurationBuilder AddOcelot(this IConfigurationBuilder builder, IHostEnvironment env = null)
        {
            return builder.AddOcelot(".", env);
        }

        public static IConfigurationBuilder AddOcelot(this IConfigurationBuilder builder, string folder, IHostEnvironment env = null)
        {
            const string primaryConfigFile = "ocelot.json";

            const string globalConfigFile = "ocelot.global.json";

            const string subConfigPattern = @"^ocelot\.(.*?)\.json$";

            string excludeConfigName = env?.EnvironmentName != null ? $"ocelot.{env.EnvironmentName}.json" : string.Empty;

            var reg = new Regex(subConfigPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var files = new DirectoryInfo(folder)
                .EnumerateFiles()
                .Where(fi => reg.IsMatch(fi.Name) && (fi.Name != excludeConfigName))
                .ToList();

            var fileConfiguration = new FileConfiguration();

            foreach (var file in files)
            {
                if (files.Count > 1 && file.Name.Equals(primaryConfigFile, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var lines = File.ReadAllText(file.FullName);

                var config = JsonConvert.DeserializeObject<FileConfiguration>(lines);

                if (file.Name.Equals(globalConfigFile, StringComparison.OrdinalIgnoreCase))
                {
                    fileConfiguration.GlobalConfiguration = config.GlobalConfiguration;
                }

                fileConfiguration.Aggregates.AddRange(config.Aggregates);
                fileConfiguration.ReRoutes.AddRange(config.ReRoutes);
            }

            var json = JsonConvert.SerializeObject(fileConfiguration);

            File.WriteAllText(primaryConfigFile, json);

            builder.AddJsonFile(primaryConfigFile, false, false);

            return builder;
        }

        public static IConfigurationBuilder AddOcelotByEnvironment(this IConfigurationBuilder builder, string folder, IHostEnvironment env)
        {
            const string primaryConfigFile = "ocelot.json";

            const string globalConfigFile = "ocelot.global.json";

            string subConfigPattern = @"^ocelot\.(?<itm>.*?)(\.(?<env>" + env.EnvironmentName + @"))?\.json$";

            string excludeConfigName = env?.EnvironmentName != null ? $"ocelot.{env.EnvironmentName}.json" : string.Empty;

            var reg = new Regex(subConfigPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var files = new DirectoryInfo(folder)
                .EnumerateFiles()
                .OrderBy(itm => itm.Name)
                .ToList();

            Dictionary<string, (string Enviornment, FileInfo File)> filesWithEnvInfo = new Dictionary<string, (string Enviornment, FileInfo File)>();
            foreach (var file in files)
            {
                var matches = reg.Match(file.Name);
                if (!matches.Success)
                {
                    continue;
                }

                var itm = matches.Groups["itm"].Value;
                var fileEnv = matches.Groups.ContainsKey("env") ? matches.Groups["env"].Value : "";

                if (fileEnv == env.EnvironmentName || string.IsNullOrWhiteSpace(fileEnv))
                {
                    if (filesWithEnvInfo.ContainsKey(itm) && string.IsNullOrWhiteSpace(filesWithEnvInfo[itm].Enviornment) && !string.IsNullOrWhiteSpace(fileEnv))
                    {
                        // use more specific environemnt config
                        filesWithEnvInfo[itm] = (fileEnv, file);
                    }
                    else if (!filesWithEnvInfo.ContainsKey(itm))
                    {
                        filesWithEnvInfo.Add(itm, (fileEnv, file));
                    }
                }
            }

            var fileConfiguration = new FileConfiguration();

            foreach (var file in filesWithEnvInfo.Select(itm => itm.Value.File))
            {
                if (files.Count > 1 && file.Name.Equals(primaryConfigFile, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var lines = File.ReadAllText(file.FullName);

                var config = JsonConvert.DeserializeObject<FileConfiguration>(lines);

                if (file.Name.Equals(globalConfigFile, StringComparison.OrdinalIgnoreCase))
                {
                    fileConfiguration.GlobalConfiguration = config.GlobalConfiguration;
                }

                fileConfiguration.Aggregates.AddRange(config.Aggregates);
                fileConfiguration.ReRoutes.AddRange(config.ReRoutes);
            }

            var json = JsonConvert.SerializeObject(fileConfiguration);

            File.WriteAllText(primaryConfigFile, json);

            builder.AddJsonFile(primaryConfigFile, false, false);

            return builder;
        }
    }
}
