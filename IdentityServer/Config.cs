using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using IdentityServer4;
using System.Security.Claims;

namespace IdentityServer
{
    public class Config
    {
        /// <summary>
        /// 클라이언트 리스트 구하기
        /// </summary>
        /// <return>클라이언트 리스트</return>
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
                },
                //new Client
                //{
                //    ClientId = "movies_mvc_client",
                //    ClientName = "Movies MVC Web App",
                //    //하이브리드 타입 
                //    AllowedGrantTypes = GrantTypes.Hybrid,
                //    RequirePkce = false,
                //    AllowRememberConsent = false,
                //    RedirectUris = new List<string>()
                //    {
                //        "https://localhost:5002/signin-oidc"
                //    },
                //    PostLogoutRedirectUris = new List<string>()
                //    {
                //        "https://localhost:5002/signout-callback-oidc"
                //    },
                //    ClientSecrets = new List<Secret>
                //    {
                //        new Secret("secret".Sha256())
                //    },
                //    AllowedScopes = new List<string>
                //    {
                //        IdentityServerConstants.StandardScopes.OpenId,
                //        IdentityServerConstants.StandardScopes.Profile,
                //        IdentityServerConstants.StandardScopes.Address,
                //        IdentityServerConstants.StandardScopes.Email,
                //        "movieAPI",
                //        "roles"
                //    }
                //}
                new Client
                {
                    ClientId = "movies_mvc_client",
                    ClientName = "Movies MVC Web App",
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowRememberConsent = false,
                    RedirectUris = new List<string>()
                    {
                        "https://localhost:5002/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "https://localhost:5002/signout-callback-oidc"
                    },
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                }
            };

        /// <summary>
        /// API 범위 리스트 구하기
        /// </summary>
        /// <returns>API 범위 리스트</returns>
        public static IEnumerable<ApiScope> ApiScopes =>
           new ApiScope[]
           {
               new ApiScope("movieAPI", "Movie API")
           };

        /// <summary>
        /// 신원 리소스 리스트 구하기
        /// </summary>
        /// <returns>신원 리소스 리스트</returns>
        public static IEnumerable<ApiResource> ApiResources =>
          new ApiResource[]
          {
          };

        public static IEnumerable<IdentityResource> IdentityResources =>
          new IdentityResource[]
          {
              new IdentityResources.OpenId(),
              new IdentityResources.Profile()
          };

        public static List<TestUser> TestUsers =>
            new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "5BE86359-073C-434B-AD2D-A3932222DABE",
                    Username = "dwkim",
                    Password = "dwkim",
                    Claims = new List<Claim>
                    {
                        new Claim(JwtClaimTypes.GivenName, "daewoong"),
                        new Claim(JwtClaimTypes.FamilyName, "kim")
                    }
                }
            };
    }
}
