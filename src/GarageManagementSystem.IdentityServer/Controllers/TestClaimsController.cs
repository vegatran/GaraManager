using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GarageManagementSystem.IdentityServer.Controllers
{
    [Authorize]
    public class TestClaimsController : Controller
    {
        /// <summary>
        /// Test endpoint để xem tất cả claims của user hiện tại
        /// </summary>
        public IActionResult Index()
        {
            var claims = User.Claims.ToList();
            
            var claimsInfo = new
            {
                UserId = User.FindFirst("sub")?.Value,
                Username = User.FindFirst("name")?.Value,
                Email = User.FindFirst("email")?.Value,
                Roles = User.FindAll("role").Select(c => c.Value).ToList(),
                AllClaims = claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList(),
                ClaimsCount = claims.Count
            };

            return Json(claimsInfo, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        /// <summary>
        /// Test endpoint để xem claims theo format đẹp
        /// </summary>
        public IActionResult Pretty()
        {
            return View();
        }

        /// <summary>
        /// API endpoint để lấy claims dạng JSON
        /// </summary>
        [HttpGet]
        public IActionResult Api()
        {
            var claims = User.Claims.ToList();
            
            // Helper method để tạo dictionary an toàn
            var allClaimsDict = new Dictionary<string, string>();
            foreach (var claim in claims)
            {
                if (allClaimsDict.ContainsKey(claim.Type))
                {
                    // Nếu đã có key, append value
                    allClaimsDict[claim.Type] += ", " + claim.Value;
                }
                else
                {
                    allClaimsDict[claim.Type] = claim.Value;
                }
            }

            var customClaimsDict = new Dictionary<string, string>();
            foreach (var claim in claims.Where(c => !IsStandardClaim(c.Type)))
            {
                if (customClaimsDict.ContainsKey(claim.Type))
                {
                    customClaimsDict[claim.Type] += ", " + claim.Value;
                }
                else
                {
                    customClaimsDict[claim.Type] = claim.Value;
                }
            }
            
            var result = new
            {
                user = new
                {
                    id = User.FindFirst("sub")?.Value,
                    username = User.FindFirst("name")?.Value,
                    email = User.FindFirst("email")?.Value,
                    emailVerified = User.FindFirst("email_verified")?.Value,
                    roles = User.FindAll("role").Select(c => c.Value).ToList()
                },
                customClaims = customClaimsDict,
                allClaims = allClaimsDict,
                summary = new
                {
                    totalClaims = claims.Count,
                    standardClaims = claims.Count(c => IsStandardClaim(c.Type)),
                    customClaims = claims.Count(c => !IsStandardClaim(c.Type))
                }
            };

            return Json(result, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        private bool IsStandardClaim(string claimType)
        {
            var standardClaims = new[]
            {
                "sub", "name", "given_name", "family_name", "middle_name",
                "nickname", "preferred_username", "profile", "picture", "website",
                "email", "email_verified", "gender", "birthdate", "zoneinfo",
                "locale", "phone_number", "phone_number_verified", "address",
                "updated_at", "role", "jti", "iss", "aud", "exp", "iat", "nbf"
            };

            return standardClaims.Contains(claimType);
        }
    }
}
