using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

public class AutoGeneration
{
    [DllImport("rpcrt4.dll", SetLastError = true)]
    private static extern int UuidCreateSequential(out Guid guid);
    private const int RPC_S_OK = 0;

    /// <summary>
    /// Generate a new sequential GUID. If UuidCreateSequential fails, it will fall back on standard random guids.
    /// </summary>
    /// <returns>A GUID</returns>
    public static Guid NewSeqGuid()
    {
        Guid sequentialGuid;
        int hResult = UuidCreateSequential(out sequentialGuid);
        if (hResult == RPC_S_OK)
        {
            return sequentialGuid;
        }
        else
        {
            //couldn't create sequential guid, fall back on random guid
            return Guid.NewGuid();
        }
    }
}
