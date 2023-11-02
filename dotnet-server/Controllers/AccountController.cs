using Dotnet.Server.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;
    private readonly AccountRepository _accountRepository;

    public AccountController(ILogger<AccountController> logger, IConfiguration configuration, AccountRepository accountRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _accountRepository = accountRepository;
    }

    [HttpPost("Insert")]
    public IActionResult Insert([FromBody] Account account)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool accountAdded = _accountRepository.Insert(account);

            if (accountAdded)
            {
                return StatusCode(StatusCodes.Status201Created);
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK);
            }
        }
        catch (Exception ex)
        {   
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("Delete/{username}")]
    public IActionResult Delete([FromHeader] string globalAdminToken, [FromRoute] string username)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            if (globalAdminToken != _configuration[AppSettingsVariables.GlobalAdminToken])
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            bool isAccountDeleted = _accountRepository.Delete(username);

            if (isAccountDeleted)
            {
                return StatusCode(StatusCodes.Status200OK);
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Get/{username}")]
    public IActionResult Get([FromHeader] string globalAdminToken, [FromRoute] string username)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            if (globalAdminToken != _configuration[AppSettingsVariables.GlobalAdminToken])
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            Account account = _accountRepository.Get(username);

            if (account != null)
            {
                return StatusCode(StatusCodes.Status200OK, account);
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetAll")]
    public IActionResult GetAll([FromHeader] string adminToken)
    {
        try 
        {
            if (!ModelState.IsValid)
            {   
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            if (adminToken != _configuration[AppSettingsVariables.GlobalAdminToken])
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            IEnumerable<Account> accounts = _accountRepository.GetAll();

            return StatusCode(StatusCodes.Status200OK, accounts);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
