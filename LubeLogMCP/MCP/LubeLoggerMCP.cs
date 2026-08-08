using LubeLogMCP.Helper;
using LubeLogMCP.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace LubeLogMCP.MCP
{
    [McpServerToolType]
    public class LubeLoggerMCP
    {
        private string instance { get; set; }
        private string username { get; set; }
        private string password { get; set; }
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LubeLoggerMCP(IConfiguration _config, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            instance = _config["LUBELOG_INSTANCE"] ?? string.Empty;
            username = _config["LUBELOG_USER"] ?? string.Empty;
            password = _config["LUBELOG_PASS"] ?? string.Empty;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        [McpServerTool, Description("Gets status of LubeLogger MCP.")]
        public async Task<string> GetLubeLoggerMCPStatus()
        {
            string result = $"MCP Version: {StaticHelper.VersionNumber}";
            result += Environment.NewLine;
            if (!string.IsNullOrWhiteSpace(instance))
            {
                result += $"MCP Server Configured for {instance}";
                result += Environment.NewLine;
                string endpoint = $"{instance}/api/version";
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                AddAuthHeaders(request);
                if (request.Headers.Contains("Authorization"))
                {
                    result += "Auth Configured";
                } else
                {
                    result += "Auth Not Configured";
                }
                result += Environment.NewLine;
                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var serverResponse = await httpClient.SendAsync(request).Result.Content.ReadFromJsonAsync<ServerVersion>();
                    if (!string.IsNullOrWhiteSpace(serverResponse?.CurrentVersion))
                    {
                        result += $"LubeLogger Version: {serverResponse.CurrentVersion}";
                    }
                }
                catch (Exception ex)
                {
                    result += $"Failed to connect to LubeLogger instance: {ex.Message}";
                }
            }
            else
            {
                result += "LubeLogger Instance not Configured";
            }
            return result;
        }
        [McpServerTool, Description("Gets vehicles in garage.")]
        public async Task<string> GetVehicles()
        {
            string endpoint = $"{instance}/api/vehicles";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadFromJsonAsync<List<Vehicle>>();
                var resultString = string.Empty;
                foreach(Vehicle vehicle in result ?? new List<Vehicle>())
                {
                    resultString += $"Id: {vehicle.Id} - {vehicle.Year} {vehicle.Make} {vehicle.Model}({vehicle.Identifier})";
                    resultString += Environment.NewLine;
                }
                return resultString;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Check if a vehicle is an electric vehicle")]
        public async Task<string> GetVehicleIsElectric([Description("id of the vehicle")] int vehicleId)
        {
            string endpoint = $"{instance}/api/vehicles";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadFromJsonAsync<List<Vehicle>>();
                foreach (Vehicle vehicle in result ?? new List<Vehicle>())
                {
                    if (vehicle.Id == vehicleId && vehicle.IsElectric)
                    {
                        return "this is an electric vehicle";
                    }
                }
                return "this is not an electric vehicle";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Adds a fuel record.")]
        public async Task<string> AddFuelRecord(
            [Description("id of the vehicle")] int vehicleId,
            [Description("Date of fuel up")] DateTime date,
            [Description("Odometer at time of fuel up")] int odometer,
            [Description("Volume of gas pumped")] decimal volume,
            [Description("Total cost of fuel up")] decimal cost,
            [Description("Is fueled up completely")] bool fillToFull,
            [Description("Any missed fuel ups")] bool missedFuelUp,
            [Description("State of Charge at beginning of charge session if charging an electric vehicle, default to 20 if not an electric vehicle")] int startingSoc,
            [Description("State of Charge at end of charge session if charging an electric vehicle, default to 80 if not an electric vehicle")] int endingSoc,
            [Description("Any extra fields configured for gasrecord")] List<ExtraField> extraFields)
        {
            var requestData = new PostRequestModel
            {
                Date = date.ToString("yyyy-MM-dd"),
                Odometer = odometer,
                FuelConsumed = volume,
                Cost = cost,
                IsFillToFull = fillToFull,
                MissedFuelUp = missedFuelUp,
                StartingSoc = startingSoc,
                EndingSoc = endingSoc
            };

            for (int i = 0; i < extraFields.Count; i++)
            {
                requestData.ExtraFields.Add(new ExtraFieldPostModel { Name = extraFields[i].Name, Value = extraFields[i].Value  });
            }

            string endpoint = $"{instance}/api/vehicle/gasrecords/add?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
            };
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Gets fuel records for a vehicle.")]
        public async Task<string> GetFuelRecords([Description("id of the vehicle")] int vehicleId)
        {
            string endpoint = $"{instance}/api/vehicle/gasrecords?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add("culture-invariant", "true");
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Adds a service record.")]
        public async Task<string> AddServiceRecord(
            [Description("id of the vehicle")] int vehicleId,
            [Description("Date serviced")] DateTime date,
            [Description("Odometer at time of service")] int odometer,
            [Description("Description of items serviced")] string description,
            [Description("Total cost of the service")] decimal cost,
            [Description("Any extra fields configured for servicerecord")] List<ExtraField> extraFields)
        {
            var requestData = new PostRequestModel
            {
                Date = date.ToString("yyyy-MM-dd"),
                Odometer = odometer,
                Description = description,
                Cost = cost
            };

            for (int i = 0; i < extraFields.Count; i++)
            {
                requestData.ExtraFields.Add(new ExtraFieldPostModel { Name = extraFields[i].Name, Value = extraFields[i].Value });
            }

            string endpoint = $"{instance}/api/vehicle/servicerecords/add?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
            };
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Gets service records for a vehicle.")]
        public async Task<string> GetServiceRecords([Description("id of the vehicle")] int vehicleId)
        {
            string endpoint = $"{instance}/api/vehicle/servicerecords?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add("culture-invariant", "true");
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Adds a repair record.")]
        public async Task<string> AddRepairRecord(
            [Description("id of the vehicle")] int vehicleId,
            [Description("Date repaired")] DateTime date,
            [Description("Odometer at time of repair")] int odometer,
            [Description("Description of items repaired")] string description,
            [Description("Total cost of the repair")] decimal cost,
            [Description("Any extra fields configured for repairrecord")] List<ExtraField> extraFields)
        {
            var requestData = new PostRequestModel
            {
                Date = date.ToString("yyyy-MM-dd"),
                Odometer = odometer,
                Description = description,
                Cost = cost
            };

            for (int i = 0; i < extraFields.Count; i++)
            {
                requestData.ExtraFields.Add(new ExtraFieldPostModel { Name = extraFields[i].Name, Value = extraFields[i].Value });
            }

            string endpoint = $"{instance}/api/vehicle/repairrecords/add?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
            };
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Gets repair records for a vehicle.")]
        public async Task<string> GetRepairRecords([Description("id of the vehicle")] int vehicleId)
        {
            string endpoint = $"{instance}/api/vehicle/repairrecords?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add("culture-invariant", "true");
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Adds an upgrade record.")]
        public async Task<string> AddUpgradeRecord(
            [Description("id of the vehicle")] int vehicleId,
            [Description("Date upgraded")] DateTime date,
            [Description("Odometer at time of upgrade")] int odometer,
            [Description("Description of items upgraded")] string description,
            [Description("Total cost of the upgrade")] decimal cost,
            [Description("Any extra fields configured for upgraderecord")] List<ExtraField> extraFields)
        {
            var requestData = new PostRequestModel
            {
                Date = date.ToString("yyyy-MM-dd"),
                Odometer = odometer,
                Description = description,
                Cost = cost
            };

            for (int i = 0; i < extraFields.Count; i++)
            {
                requestData.ExtraFields.Add(new ExtraFieldPostModel { Name = extraFields[i].Name, Value = extraFields[i].Value });
            }

            string endpoint = $"{instance}/api/vehicle/upgraderecords/add?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
            };
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Gets upgrade records for a vehicle.")]
        public async Task<string> GetUpgradeRecords([Description("id of the vehicle")] int vehicleId)
        {
            string endpoint = $"{instance}/api/vehicle/upgraderecords?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add("culture-invariant", "true");
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Adds an odometer record.")]
        public async Task<string> AddOdometerRecord(
            [Description("id of the vehicle")] int vehicleId,
            [Description("Date recorded")] DateTime date,
            [Description("Odometer recorded")] int odometer,
            [Description("Any extra fields configured for odometerrecord")] List<ExtraField> extraFields,
            [Description("Ids of equipment equipped for the vehicle")] List<int> equipmentRecordIds)
        {
            var requestData = new PostRequestModel
            {
                Date = date.ToString("yyyy-MM-dd"),
                Odometer = odometer
            };

            if (equipmentRecordIds.Any())
            {
                requestData.EquipmentRecordId = string.Join(' ', equipmentRecordIds);
            }

            for (int i = 0; i < extraFields.Count; i++)
            {
                requestData.ExtraFields.Add(new ExtraFieldPostModel { Name = extraFields[i].Name, Value = extraFields[i].Value });
            }

            string endpoint = $"{instance}/api/vehicle/odometerrecords/add?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
            };
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Gets odometer records for a vehicle.")]
        public async Task<string> GetOdometerRecords([Description("id of the vehicle")] int vehicleId)
        {
            string endpoint = $"{instance}/api/vehicle/odometerrecords?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add("culture-invariant", "true");
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Get Equipped Equipment for a vehicle")]
        public async Task<string> GetEquippedEquipment(
            [Description("id of the vehicle")] int vehicleId
            )
        {

            string endpoint = $"{instance}/api/vehicle/equipmentrecords?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add("culture-invariant", "true");
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadFromJsonAsync<List<Equipment>>();
                result?.RemoveAll(x => !x.IsEquipped);
                var serializedResult = JsonSerializer.Serialize(result);
                return serializedResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Adds a supply record.")]
        public async Task<string> AddSupplyRecord(
            [Description("id of the vehicle")] int vehicleId,
            [Description("Date purchased")] DateTime date,
            [Description("Description of the supply")] string description,
            [Description("Quantity purchased")] decimal quantity,
            [Description("Cost of the supply")] decimal cost,
            [Description("Part number")] string partNumber,
            [Description("Part supplier")] string partSupplier,
            [Description("Any extra fields configured for supplyrecord")] List<ExtraField> extraFields)
        {
            var requestData = new PostRequestModel
            {
                Date = date.ToString("yyyy-MM-dd"),
                Description = description,
                PartQuantity = quantity,
                Cost = cost,
                PartNumber = partNumber,
                PartSupplier = partSupplier
            };

            for (int i = 0; i < extraFields.Count; i++)
            {
                requestData.ExtraFields.Add(new ExtraFieldPostModel { Name = extraFields[i].Name, Value = extraFields[i].Value });
            }

            string endpoint = $"{instance}/api/vehicle/supplyrecords/add?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
            };
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Gets supply records for a vehicle.")]
        public async Task<string> GetSupplyRecords([Description("id of the vehicle")] int vehicleId)
        {
            string endpoint = $"{instance}/api/vehicle/supplyrecords?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add("culture-invariant", "true");
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Adds a shop supply record.")]
        public async Task<string> AddShopSupplyRecord(
            [Description("Date purchased")] DateTime date,
            [Description("Description of the supply")] string description,
            [Description("Quantity purchased")] decimal quantity,
            [Description("Cost of the supply")] decimal cost,
            [Description("Part number")] string partNumber,
            [Description("Part supplier")] string partSupplier,
            [Description("Any extra fields configured for supplyrecord")] List<ExtraField> extraFields)
        {
            var requestData = new PostRequestModel
            {
                Date = date.ToString("yyyy-MM-dd"),
                Description = description,
                PartQuantity = quantity,
                Cost = cost,
                PartNumber = partNumber,
                PartSupplier = partSupplier
            };

            for (int i = 0; i < extraFields.Count; i++)
            {
                requestData.ExtraFields.Add(new ExtraFieldPostModel { Name = extraFields[i].Name, Value = extraFields[i].Value });
            }

            string endpoint = $"{instance}/api/vehicle/supplyrecords/add?vehicleId=0";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
            };
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Gets shop supply records.")]
        public async Task<string> GetShopSupplyRecords()
        {
            string endpoint = $"{instance}/api/vehicle/supplyrecords?vehicleId=0";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add("culture-invariant", "true");
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Adds a vehicle record.")]
        public async Task<string> AddVehicleRecord(
            [Description("Model year of the vehicle")] int year,
            [Description("Make of the vehicle")] string make,
            [Description("Model of the vehicle")] string model,
            [Description("Optional license plate of the vehicle")] string licensePlate,
            [Description("Vehicle use engine hours")] bool useEngineHours,
            [Description("Odometer is optional for vehicle")] bool odometerOptional,
            [Description("Fuel type for the vehicle")] FuelType fuelType,
            [Description("Any extra fields configured for vehiclerecord")] List<ExtraField> extraFields)
        {
            var dataParams = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("year", year.ToString()),
                new KeyValuePair<string, string>("make", make),
                new KeyValuePair<string, string>("model", model),
                new KeyValuePair<string, string>("useEngineHours", useEngineHours.ToString()),
                new KeyValuePair<string, string>("odometerOptional", odometerOptional.ToString()),
                new KeyValuePair<string, string>("identifier", "LicensePlate"),
                new KeyValuePair<string, string>("licensePlate", licensePlate),
                new KeyValuePair<string, string>("fuelType", fuelType.ToString())
            };

            for (int i = 0; i < extraFields.Count; i++)
            {
                dataParams.Add(new KeyValuePair<string, string>($"extraFields[{i}][name]", extraFields[i].Name));
                dataParams.Add(new KeyValuePair<string, string>($"extraFields[{i}][value]", extraFields[i].Value));
            }

            string endpoint = $"{instance}/api/vehicles/add";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new FormUrlEncodedContent(dataParams)
            };
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Gets latest odometer reading for a vehicle.")]
        public async Task<string> GetLatestOdometer(
            [Description("id of the vehicle")] int vehicleId
            )
        {

            string endpoint = $"{instance}/api/vehicle/odometerrecords/latest?vehicleId={vehicleId}";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadAsStringAsync();

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [McpServerTool, Description("Get Extra Fields for a Record Type")]
        public async Task<string> GetExtraFields(
            [Description("Record type")] ImportMode importMode
            )
        {

            string endpoint = $"{instance}/api/extrafields";

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            AddAuthHeaders(request);
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var result = await httpClient.SendAsync(request).Result.Content.ReadFromJsonAsync<List<ExtraFieldsVM>>();
                result?.RemoveAll(x => x.RecordType != importMode.ToString());
                var serializedResult = JsonSerializer.Serialize(result);
                return serializedResult;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private void AddAuthHeaders(HttpRequestMessage request)
        {
            //check if we have headers
            if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("Authorization", out var authHeader) ?? false)
            {
                request.Headers.Add("Authorization", authHeader.FirstOrDefault());
            }
            else if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("x-api-key", out var apiKeyHeader) ?? false)
            {
                request.Headers.Add("x-api-key", apiKeyHeader.FirstOrDefault());
            }
            else if (_httpContextAccessor.HttpContext?.Request.Query.TryGetValue("apiKey", out var apiKey) ?? false)
            {
                request.Headers.Add("x-api-key", apiKey.FirstOrDefault());
            }
            else if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                var authenticationString = $"{username}:{password}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authenticationString));
                request.Headers.Add("Authorization", "Basic " + base64EncodedAuthenticationString);
            }
        }
    }
}
