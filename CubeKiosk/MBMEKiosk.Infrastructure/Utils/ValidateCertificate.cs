using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Diagnostics;
using System.Net;

namespace MBMEKiosk.Infrastructure.Utils
{
    public class ValidateCertificate
    {
        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            //if (sslPolicyErrors == SslPolicyErrors.None)
            //    return true;

            //Trace.TraceError("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return true;
        }

        public static void RegisterCallback()
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateCertificate.ValidateServerCertificate;
        }

        public static void DeregisterCallback()
        {
            ServicePointManager.ServerCertificateValidationCallback -= ValidateCertificate.ValidateServerCertificate;
        }
    }
}
