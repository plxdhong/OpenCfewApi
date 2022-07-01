using CFEW.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CFEW.Server.Services
{
    public class XrayConfigService
    {
        private readonly IConfiguration _configuration;
        private string _xrayConfigPath;
        private string _xrayExamplePath;
        private string _xrayFlow;

        public XrayConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
            _xrayConfigPath=_configuration["XrayConfigPath"];
            _xrayExamplePath = _configuration["XrayExamplePath"];
            _xrayFlow = _configuration["XrayFlow"];
        }

        public async void SyncXrayConfig(List<UserDetail> userDetails)
        {
            JsonObject jsonConfig = await GetConfigAsync(_xrayExamplePath);
            if (jsonConfig == null) throw new Exception(_xrayExamplePath + "无XrayExampleConfig");
            jsonConfig["inbounds"][0]["settings"]["clients"] = DbToJson(userDetails,_xrayFlow);
            await WriteConfigAsync(jsonConfig, _xrayConfigPath);
        }

        public async Task<JsonObject> GetConfigAsync(string url)
        {
            string jsonString =await File.ReadAllTextAsync(url);
            JsonObject jsonObject = (JsonObject)JsonNode.Parse(jsonString);
            return jsonObject;
        }
        public async Task<JsonObject> WriteConfigAsync(JsonObject jsonObject,string outputFileName)
        {
            var writerOptions = new JsonWriterOptions
            {
                Indented = true
            };
            using FileStream fs = File.Create(outputFileName);
            using var writer = new Utf8JsonWriter(fs, options: writerOptions);
            jsonObject.WriteTo(writer);
            await writer.FlushAsync();
            return jsonObject;
        }
        public JsonArray DbToJson(List<UserDetail> userDetails,string flow= "xtls-rprx-direct",uint level=0)
        {
            var users=new JsonArray();
            foreach (var userDetail in userDetails)
            {
                users.Add(new JsonObject
                {
                    ["id"] = userDetail.Uuid,
                    ["flow"]= flow,
                    ["email"] = userDetail.Email,
                    ["level"] = level
                });
            }
            return users;
        }
    }
}
