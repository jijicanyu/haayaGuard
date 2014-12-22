using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Haaya.GuardClient
{
   internal class DefineTable
    {
       public static readonly string ServerHost = ConfigurationManager.AppSettings["ServerHost"];
       public static readonly int ServerPort =int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
    }
}
