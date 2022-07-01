using CFEW.Server.Services;
using CFEW.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CFEW.Server.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly XrayGrpcService _xrayGrpcService;
    private readonly XrayConfigService _xrayConfigService;
    public UserController(UserService userService,
                            XrayGrpcService xrayGrpcService,
                            XrayConfigService xrayConfigService)
    {
        _userService = userService;
        _xrayConfigService = xrayConfigService;
        _xrayGrpcService = xrayGrpcService;
    }

    // GET: api/<UserController>
    [HttpGet]
    public List<UserDetail> Get()
    {
        return _userService.Get();
    }

    // GET api/<UserController>/5
    [HttpGet("{id}")]
    public UserDetail Get(string id)
    {
        return _userService.Get(id.ToString()) ;
    }

    // POST api/<UserController>
    [HttpPost]
    public async Task<UserDetail> PostAsync(UserDetail value)
    {
        
        if (_userService.Get(value.Id) != null)
        {
            throw new Exception($"{value.Id}已存在");
        }
        
        if (_userService.GetFormEmail(value.Email) != null)
        {
            throw new Exception($"{value.Email}已存在");
        }
        await _userService.CreateAsync(value);
        await _xrayGrpcService.AddUserOperation(value);
        _xrayConfigService.SyncXrayConfig(_userService.Get());
        return value;
    }

    // PUT api/<UserController>/5
    [HttpPut("{id}")]
    public async Task<UserDetail> PutAsync(string id, UserDetail value)
    {

        if (_userService.Get(id.ToString()) == null)
        {
            throw new Exception($"{id}用户不存在");
        }

        await _xrayGrpcService.RemoveUserOperation(_userService.Get(id.ToString()));
        _userService.Update(id.ToString(), value);
        await _xrayGrpcService.AddUserOperation(_userService.Get(id.ToString()));
        _xrayConfigService.SyncXrayConfig(_userService.Get());
        return value;
    }

    // DELETE api/<UserController>/5
    [HttpDelete("{id}")]
    public async void Delete(string id)
    {
        await _xrayGrpcService.RemoveUserOperation(_userService.Get(id.ToString()));
        _userService.Delete(id.ToString());
        _xrayConfigService.SyncXrayConfig(_userService.Get());
    }

    // PATCH api/<UserController>
    [HttpPatch]
    public void Patch()
    {
        _xrayConfigService.SyncXrayConfig(_userService.Get());
    }
}
