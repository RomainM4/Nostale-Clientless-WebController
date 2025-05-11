using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace NosCoreTCP.Client
{
    public class SimpleTcpClientProxySettings
    {
        #region Public-Members

        public string Host {  get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password {  get; set; }

        #endregion

  
        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SimpleTcpClientProxySettings()
        {

        }

        #endregion

    }
}
