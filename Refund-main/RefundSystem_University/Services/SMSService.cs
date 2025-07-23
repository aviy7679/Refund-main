using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace RefundSystem_University.Services
{
    public class SMSService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;

        public SMSService(string accountSid, string authToken, string fromNumber)
        {
            _accountSid = accountSid;
            _authToken = authToken;
            _fromNumber = fromNumber;
            TwilioClient.Init(_accountSid, _authToken);
        }

        public bool SendSMS(string phoneNumber, string message, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                phoneNumber = CleanPhoneNumber(phoneNumber);

                if (string.IsNullOrEmpty(phoneNumber))
                {
                    errorMessage = "מספר טלפון לא תקין";
                    return false;
                }

                var messageResource = MessageResource.Create(
                    body: message,
                    from: new PhoneNumber(_fromNumber),
                    to: new PhoneNumber("+" + phoneNumber)
                );

                if (messageResource.Status == MessageResource.StatusEnum.Failed)
                {
                    errorMessage = $"SMS נכשל: {messageResource.ErrorMessage}";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private string CleanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            phoneNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (phoneNumber.StartsWith("0"))
                phoneNumber = "972" + phoneNumber.Substring(1);

            if (!phoneNumber.StartsWith("972"))
                phoneNumber = "972" + phoneNumber;

            return phoneNumber;
        }
    }
}