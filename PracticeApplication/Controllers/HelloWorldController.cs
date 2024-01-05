using Microsoft.AspNetCore.Mvc;

namespace PracticeApplication.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloWorldController
{
    [HttpGet(Name = "GetHelloWorld")]
    public string Get()
    {
        return "Hello World!";
    }
}