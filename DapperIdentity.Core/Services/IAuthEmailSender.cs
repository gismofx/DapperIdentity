namespace CPE.DapperIdentity.Core.Services;

/// <summary>
/// Sends the transactional emails the authentication endpoints need: the password-reset link used
/// by both registration and "forgot password", and the notification sent after a reset succeeds.
/// </summary>
/// <remarks>
/// <para>
/// This exists so the library does not depend on <c>Microsoft.AspNetCore.Identity.UI</c>, which is
/// a Bootstrap5 Razor Pages package that would otherwise be dragged into every API that consumes
/// the JWT server - for one interface.
/// </para>
/// <para>
/// The framework alternative, <c>Microsoft.AspNetCore.Identity.IEmailSender&lt;TUser&gt;</c>, does
/// not fit: its three methods take no subject or body, so the registration and forgot-password
/// mails would become identical text, and the reset-succeeded notification has no method at all.
/// </para>
/// <para>
/// The name is deliberately NOT <c>IEmailSender</c>. A consumer that also has
/// <c>Microsoft.AspNetCore.Identity.UI.Services</c> in scope would get CS0104 on every file that
/// imported both, and the library renaming once is cheaper than every consumer aliasing forever.
/// </para>
/// </remarks>
public interface IAuthEmailSender
{
    /// <summary>
    /// Sends one email.
    /// </summary>
    /// <param name="email">Recipient address.</param>
    /// <param name="subject">Subject line.</param>
    /// <param name="htmlMessage">Body, as HTML.</param>
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}
