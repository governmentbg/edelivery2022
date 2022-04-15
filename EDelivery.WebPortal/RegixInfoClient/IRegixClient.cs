using RegixInfoClient.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegixInfoClient
{
    public interface IRegixClient
    {
        ValidPersonResponse GetValidPersonInfo(string egn);
    }
}
