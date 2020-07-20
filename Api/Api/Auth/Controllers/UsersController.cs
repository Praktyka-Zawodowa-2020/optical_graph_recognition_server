using System;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        ///     Sign-ins user.
        /// </summary>
        /// <remarks>
        ///     Signs ins user based on Google Codes acquired by signing in to Google in Android Client.
        /// </remarks>
        /// <response code="200"> Returns access token in response body used to authorize this API along with refresh token, that needs to be stored for later - can be used to obtain new access token if one expires.</response>
        /// <response code="400">Invalid authCode.</response>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Invalid google token." });

            setTokenCookie(response.RefreshToken);

            return Ok(response);
        }
        /// <summary>
        ///     Refreshes access token and revokes used refresh token.
        /// </summary>
        /// <remarks>
        ///     Refreshes access token based on refresh token provided in "refreshToken" cookie. Each refresh token can be used only once.
        /// </remarks>
        /// <response code="200"> Returns refreshed access token along with a new refresh token.</response>
        /// <response code="400">Invalid token.</response>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _userService.RefreshToken(refreshToken);

            if (response == null)
                return BadRequest(new { message = "Invalid token" });

            //setTokenCookie(response.RefreshToken);

            return Ok(response);
        }
        /// <summary>
        ///     Makes specific refresh token not valid anymore.
        /// </summary>
        /// <remarks>
        ///     Revokes refresh token given in either body or "refreshToken" cookie. Such token can't be used to obtain new ones.
        /// </remarks>
        /// <response code="200"> Token succesfully revoked.</response>
        /// <response code="400">Token is required</response>
        /// <response code="400">Token not found</response>
        [HttpPost("revoke-token")]
        public IActionResult RevokeToken([FromBody] RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            var response = _userService.RevokeToken(token);

            if (!response)
                return BadRequest(new { message = "Token not found" });

            return Ok(new { message = "Token revoked" });
        }

        // helper methods
        private void setTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
    }
}