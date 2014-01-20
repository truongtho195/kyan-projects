using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

public class GuidUtils
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

//public enum SequentialGuidType
//{
//    SequentialAsString,
//    SequentialAsBinary,
//    SequentialAtEnd
//}

//public static class SequentialGuidGenerator
//{

//    private static readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

//    public static Guid NewSequentialGuid(SequentialGuidType guidType)
//    {

//        byte[] randomBytes = new byte[10];
//        _rng.GetBytes(randomBytes);

//        long timestamp = DateTime.Now.Ticks / 10000L;
//        byte[] timestampBytes = BitConverter.GetBytes(timestamp);

//        if (BitConverter.IsLittleEndian)
//        {
//            Array.Reverse(timestampBytes);
//        }

//        byte[] guidBytes = new byte[16];

//        switch (guidType)
//        {

//            case SequentialGuidType.SequentialAsString:
//            case SequentialGuidType.SequentialAsBinary:

//                Buffer.BlockCopy(timestampBytes, 2, guidBytes, 0, 6);
//                Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10);

//                // If formatting as a string, we have to reverse the order
//                // of the Data1 and Data2 blocks on little-endian systems.
//                if (guidType == SequentialGuidType.SequentialAsString && BitConverter.IsLittleEndian)
//                {
//                    Array.Reverse(guidBytes, 0, 4);
//                    Array.Reverse(guidBytes, 4, 2);
//                }
//                break;

//            case SequentialGuidType.SequentialAtEnd:

//                Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
//                Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);
//                break;
//        }

//        return new Guid(guidBytes);
//    }
//}