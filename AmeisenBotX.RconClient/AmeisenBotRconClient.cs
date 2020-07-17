using AmeisenBotX.RconClient.Enums;
using AmeisenBotX.RconClient.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.RconClient
{
    public class AmeisenBotRconClient
    {
        public AmeisenBotRconClient(string endpoint, string name, string wowRace, string wowGender, string wowClass, string wowRole, string image = "", string guid = "", bool validateCertificate = false)
        {
            Endpoint = endpoint;

            if (!validateCertificate)
            {
                HttpClientHandler handler = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
                };

                HttpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(1) };
            }
            else
            {
                HttpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(1) };
            }

            Guid = guid.Length > 0 ? guid : System.Guid.NewGuid().ToString();

            RegisterMessage = new RegisterMessage()
            {
                Guid = Guid,
                Name = name,
                Race = wowRace,
                Gender = wowGender,
                Class = wowClass,
                Role = wowRole,
                Image = image
            };
        }

        public string Endpoint { get; set; }

        public string Guid { get; set; }

        public bool NeedToRegister { get; private set; } = true;

        public List<ActionType> PendingActions { get; private set; } = new List<ActionType>();

        public RegisterMessage RegisterMessage { get; }

        private HttpClient HttpClient { get; set; }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        public async Task<bool> KeepAlive()
        {
            HttpResponseMessage dataResponse = await HttpClient.PostAsync
            (
                $"{Endpoint}/api/keepalive",
                new StringContent(JsonConvert.SerializeObject(new KeepAliveMessage() { Guid = Guid }),
                Encoding.UTF8,
                "application/json")
            );

            if (dataResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                NeedToRegister = true;
                return false;
            }
        }

        public async Task<bool> PullPendingActions()
        {
            HttpResponseMessage dataResponse = await HttpClient.GetAsync
            (
                $"{Endpoint}/api/action/{Guid}"
            );

            if (dataResponse.IsSuccessStatusCode)
            {
                PendingActions = JsonConvert.DeserializeObject<List<ActionType>>(await dataResponse.Content.ReadAsStringAsync());
                return true;
            }
            else
            {
                NeedToRegister = true;
                return false;
            }
        }

        public async Task<bool> Register()
        {
            HttpResponseMessage registerResponse = await HttpClient.PostAsync
            (
                $"{Endpoint}/api/register",
                new StringContent(JsonConvert.SerializeObject(RegisterMessage),
                Encoding.UTF8,
                "application/json")
            );

            NeedToRegister = false;

            if (registerResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> SendData(DataMessage dataMessage)
        {
            dataMessage.Guid = Guid;

            HttpResponseMessage dataResponse = await HttpClient.PostAsync
            (
                $"{Endpoint}/api/data",
                new StringContent(JsonConvert.SerializeObject(dataMessage),
                Encoding.UTF8,
                "application/json")
            );

            if (dataResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                NeedToRegister = true;
                return false;
            }
        }

        public async Task<bool> SendImage(string image)
        {
            HttpResponseMessage dataResponse = await HttpClient.PostAsync
            (
                $"{Endpoint}/api/image",
                new StringContent(JsonConvert.SerializeObject(new ImageMessage() { Guid = Guid, Image = image }),
                Encoding.UTF8,
                "application/json")
            );

            if (dataResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                NeedToRegister = true;
                return false;
            }
        }
    }
}