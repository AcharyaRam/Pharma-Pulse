using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;

public class SmsService
{
    private readonly string accountSid;
    private readonly string authToken;
    private readonly string fromNumber;

    public SmsService(IConfiguration config)
    {
        accountSid = config["Twilio:AccountSid"];
        authToken = config["Twilio:AuthToken"];
        fromNumber = config["Twilio:FromNumber"];
    }

    public void SendSms(string to, string message)
    {
        TwilioClient.Init(accountSid, authToken);

        var msg = MessageResource.Create(
            body: message,
            from: new PhoneNumber(fromNumber),
            to: new PhoneNumber(to)
        );
    }
}