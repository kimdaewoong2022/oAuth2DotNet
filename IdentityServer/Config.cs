using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using IdentityServer4;
using System.Security.Claims;

namespace IdentityServer
{
    public class Config
    {
        public static IEnumerable<Client> Clients =>
            new Client[]
            {  
                new Client
                {
                    // 클라이언트 ID는 고유한 이름이며, 클라이언트 자격 증명을 사용하여 이 클라이언트를 인증
                    // 클라이언트 정의를 추가하여 보호된 영화 API 리소스에 접근하도록 허용
                    ClientId = "movieClient",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes ={"movieAPI"}
                    
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
           new ApiScope[]
           {
               new ApiScope("movieAPI", "Movie API")
           };

        public static IEnumerable<ApiResource> ApiResources =>
          new ApiResource[]
          {
          };

        public static IEnumerable<IdentityResource> IdentityResources =>
          new IdentityResource[]
          {
          };

        public static List<TestUser> TestUsers =>
            new List<TestUser>
            {
            };
    }
}
