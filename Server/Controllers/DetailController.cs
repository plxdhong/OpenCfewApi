using CFEW.Server.Services;
using CFEW.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CFEW.Server.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "xray")]
public class DetailController : ControllerBase
{
    private readonly UserService _userService;
    private readonly XrayGrpcService _xrayGrpcService;
    private readonly XrayConfigService _xrayConfigService;
    public DetailController(UserService userService,
                        XrayGrpcService xrayGrpcService,
                        XrayConfigService xrayConfigService)
    {
        _userService = userService;
        _xrayConfigService = xrayConfigService;
        _xrayGrpcService = xrayGrpcService;
    }

    [HttpGet]
    public UserDetail Get()
    {
        string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        UserDetail userinfo = _userService.Get(userId);
        if(userinfo==null)
            throw new Exception($"{userId}未注册");
        return userinfo;
    }

    // POST api/<UserController>
    [HttpPost]
    public async Task<UserDetail> PostAsync(UserDetail value)
    {
        /*
        if (_userService.Get(value.Id) == null)
        {
            return new ReturnData<UserDetail> { Data = value, Success = false, Message = "用户不存在" };
        }
        */
        string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        UserDetail userinfo = _userService.Get(userId);
        if (userinfo != null)
            throw new Exception($"{userId}已注册");
        if (_userService.GetFormEmail(value.Email) != null)
        {
            throw new Exception($"{value.Email}已存在");
        }
        value.Id = userId;
        value.Uuid= Guid.NewGuid().ToString();
        value.Created = DateTime.Now;
        await _userService.CreateAsync(value);
        await _xrayGrpcService.AddUserOperation(value);
        _xrayConfigService.SyncXrayConfig(_userService.Get());
        return value;
    }

}
