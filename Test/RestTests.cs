using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using TG_Bot.BusinessLayer.CCUModels;
using Xunit;

namespace Test
{
    public class RestTests
    {
        /// <summary>
        /// Получение информации о контроллере
        /// </summary>
        /// <remarks>
        ///    {
        ///    "DeviceType ": "CCU825" ,
        ///    "DeviceMod ": "HOME+",
        ///!   "ExtBoard ": " E01 . 1 " ,
        ///!   " InputsCount ": 1 6 ,
        ///!   " Pa r ti tio n sCo u n t " : 4 ,
        ///    "HwVer " : " 1 0 . 0 2 " ,
        ///    "FwVer " : " 0 2 . 0 2 " ,
        ///    "BootVer " : " 0 1 . 0 2 " ,
        ///    "FwBuildDate ": " Aug 31 2015" ,
        ///    "CountryCode ": "RUS" ,
        ///    " S e r i a l ":"1414 FD09535605154EF8C306F5043213 " ,
        ///    "IMEI ":"869158123877455" ,
        ///    "uGuardVerCode ":17
        ///}
        /// http://localhost:8080/data.cgx?cmd={"Command": "GetStateAndEvents"}
        /// </remarks>
        [Fact]
        public void GetDeviceInfo()
        {
            var client = new RestClient("http://192.168.0.118:4040/data.cgx");
            client.Authenticator = new HttpBasicAuthenticator("Nick", "gala2013");
            var request = new RestRequest(@"?cmd={""Command"":""GetDeviceInfo""}", DataFormat.Json);
            var response = client.Get(request);
            var model = JsonConvert.DeserializeObject<DeviceModel>(response.Content);

            Assert.True(model != null);
        }

        [Fact]
        public void GetStateAndEvents()
        {
            var client = new RestClient("http://192.168.0.118:4040/data.cgx");
            client.Authenticator = new HttpBasicAuthenticator("Nick", "gala2013");
            var request = new RestRequest(@"?cmd={""Command"":""GetStateAndEvents""}", DataFormat.Json);
            var response = client.Get(request);
            var model = JsonConvert.DeserializeObject<CcuState>(response.Content);

            Assert.True(model != null);
        }
        //TODO написать тест с Команда SetOutputState

    }
}