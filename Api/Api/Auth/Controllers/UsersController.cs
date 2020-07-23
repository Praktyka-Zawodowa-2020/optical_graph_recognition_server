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
    [Produces("application/json")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        ///     Signs-in user using Google APIs.
        /// </summary>
        /// <remarks>
        ///     Signs-in user based on Google tokens acquired by requesting the Google APIs in Client Application. To do so, check "Google Implicit Flow".
        /// </remarks>
        /// <response code="200"> Returns an access token and a refresh token.</response>
        /// <response code="400">Invalid token(s).</response>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(AuthenticateResponse), 200)]
        public IActionResult Authenticate([FromBody] AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Invalid google token." });

            return Ok(response);
        }

        /// <summary>
        ///     Refreshes access token and revokes used refresh token.
        /// </summary>
        /// <remarks>
        ///     Generates a new access token based on a refresh token. Each refresh token can be used only once, since previous one gets invalidated.
        /// </remarks>
        /// <response code="200"> Returns a new valid access token along with a new refresh token.</response>
        /// <response code="400">Invalid token.</response>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] TokenRequest refreshToken)
        {
            var response = _userService.RefreshToken(refreshToken.Token);

            if (response == null)
                return BadRequest(new { message = "Invalid token" });

            return Ok(response);
        }

        /// <summary>
        ///     Makes a specific refresh token not valid anymore.
        /// </summary>
        /// <remarks>
        ///     Invalidates a given refresh token. Such token can't be used to obtain new ones.
        /// </remarks>
        /// <response code="200">Token succesfully revoked.</response>
        /// <response code="400">Token is required</response>
        /// <response code="400">Token not found</response>
        [HttpPost("revoke-token")]
        public IActionResult RevokeToken([FromBody] TokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token;

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