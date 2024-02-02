using IdentityModel.Client;
using Movies.Client.Models;
using Newtonsoft.Json;

namespace Movies.Client.ApiServices
{
    public class MovieApiService : IMovieApiService
    {
        public async Task<IEnumerable<Movie>> GetMovies()
        {
            //임시 Data 
            //var movieList = new List<Movie>();
            //movieList.Add(new Movie
            //{
            //    Id = 1,
            //    Genre = "Comic",
            //    Title = "10배의 법칙",
            //    ImageUrl = "images/src",
            //    ReleaseDate = DateTime.Now,
            //    Owner = "Kim",
            //    Rating = "9.9"
            //}
            //);

            //return await Task.FromResult( movieList );
                    

            // 1. "retrieve" our api credentials. This must be registered on Identity Server!
            // 1. API 자격 증명을 "retrieve"합니다. 이는 Identity Server에 등록되어야 합니다!
            var apiClientCredentials = new ClientCredentialsTokenRequest
            {
                Address = "https://localhost:5005/connect/token",

                ClientId = "movieClient",
                ClientSecret = "secret",

                // This is the scope our Protected API requires. 
                Scope = "movieAPI"
            };
           
            // IdentityServer(localhost:5005)와 통신하기 위해 새로운 HttpClient를 생성합니다.
            var client = new HttpClient();

            // Discovery 문서에 접근할 수 있는지 확인합니다. 100% 필요하지는 않지만..
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5005");
            if (disco.IsError)
            {
                return null; // throw 500 error
            }

            // 2. Identity Server에서 액세스 토큰을 인증하고 가져옵니다.            
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(apiClientCredentials);
            if (tokenResponse.IsError)
            {
                return null;
            }

            // 이제 보호된 API와 통신하기 위한 또 다른 HttpClient
            var apiClient = new HttpClient();

            // 3. 요청에 access_token을 설정합니다. Authorization: Bearer <token>            
            client.SetBearerToken(tokenResponse.AccessToken);

            // 4. 보호된 API에 요청 보내기
            var response = await client.GetAsync("https://localhost:5001/api/movies");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var movieList = JsonConvert.DeserializeObject<List<Movie>>(content);
            return movieList;
        }

        public Task<Movie> GetMovie(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Movie> CreateMovie(Movie movie)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMovie(int id)
        {
            throw new NotImplementedException();
        }

        

        public Task<Movie> UpdateMovie(Movie movie)
        {
            throw new NotImplementedException();
        }
    }
}
