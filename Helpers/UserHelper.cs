namespace VocabLab.Helpers
{
    public static class UserHelper
    {
        public static string GetUserId(HttpContext context)
        {
            const string cookieName = "vocab_user";

            if (context.Request.Cookies.TryGetValue(cookieName, out var userId))
            {
                return userId;
            }

            userId = Guid.NewGuid().ToString();

            context.Response.Cookies.Append(
                cookieName,
                userId,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });

            return userId;
        }
    }
}