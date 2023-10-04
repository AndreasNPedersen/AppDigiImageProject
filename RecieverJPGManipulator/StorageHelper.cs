using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RecieverJPGManipulator
{
    public static class StorageHelper
    {
        private static readonly string storageURL = "https://localhost:44303/api/Storage?fileName=";
        public async static Task<Stream> GetImageAsync (HttpClient httpClient,string fileName)
        {
            HttpResponseMessage response = await httpClient.GetAsync(storageURL+ fileName);
            return await response.Content.ReadAsStreamAsync();
        }

        public async static Task DeleteImageAsync(HttpClient httpClient, string fileName)
        {
            httpClient.DeleteAsync(storageURL + fileName);
        }
    }
}
