using Aula77.Redis.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Aula77.Redis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaisesController : ControllerBase
    {
        public readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistributedCache _distributedCache;
        private const string PaisesKey = "Paises";
        public PaisesController(IHttpClientFactory httpClientFactory, IDistributedCache distributedCache)
        {
            _httpClientFactory = httpClientFactory;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public async Task<List<PaisModel>> GetPaises()
        {
            var dadosRedis = await _distributedCache.GetStringAsync(PaisesKey);

            if (!string.IsNullOrWhiteSpace(dadosRedis))
            {
                return JsonConvert.DeserializeObject<List<PaisModel>>(dadosRedis);
            }
            else
            {
                List<PaisModel> dadosRetornoAPI = await DadosAPI();
                string dadosRetornoAPIJson = JsonConvert.SerializeObject(dadosRetornoAPI);

                var memoryCacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60),
                    SlidingExpiration = TimeSpan.FromMinutes(10),
                };

                await _distributedCache.SetStringAsync(PaisesKey, dadosRetornoAPIJson, memoryCacheEntryOptions);

                return dadosRetornoAPI;
            }

        }


        private async Task<List<PaisModel>> DadosAPI()
        {
            var httpClient = _httpClientFactory.CreateClient("APIDoNossoFornecedorDePaises");
            HttpResponseMessage response = await httpClient.GetAsync("v2/all");

            if (response.IsSuccessStatusCode)
            {

                return JsonConvert.DeserializeObject<List<PaisModel>>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }

        }
    }
}
