using System.Net;
using System.Text.Json;

namespace Ecommerce.API.Middleware
{
    // Ye class ek "middleware" hai - matlab ye har HTTP request ke beech mein baithta hai
    // aur request ko aage jaane deta hai, lekin agar koi exception aaye, use pakad leta hai
    public class ExceptionHandlingMiddleware
    {
        // RequestDelegate - ye "agla step" hai pipeline mein
        // (jaise ek chain hoti hai middlewares ki - ye batata hai "iske baad kya chalega")
        private readonly RequestDelegate _next;

        // ILogger - errors ko console/file mein "log" karne ke liye (taaki hum baad mein dekh sakein kya hua)
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Ye method har request pe automatically chalta hai
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pehle request ko normally chalne do (aage wale controllers/actions tak jaane do)
                await _next(context);
            }
            catch(Exception ex)
            {
                // Agar kahi bhi (kisi bhi controller mein) exception aaye, yahan pakda jayega

                // Pehle asli error ko LOG karo (server ke console/file mein - customer ko nahi dikhega)
                // Ye zaroori hai taaki hum khud dekh sakein production mein kya galat hua

                _logger.LogError(ex, "An unhandled exception occurred.");

                // Ab customer ko ek CLEAN, GENERIC response bhejo - koi technical detail nahi
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new
                {
                    message = "Something went wrong. Please try again later.",
                    // Development mein thoda zyada detail dikha sakte hain (debugging ke liye),
                    // production mein ye hata dena chahiye
                    detail = ex.Message
                };

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
