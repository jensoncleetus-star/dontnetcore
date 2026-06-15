using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace QuickSoft.Models
{
    public static class Security
    {
        public static string Encrypt(string encryptString, string EncryptionKey)
        {
            //string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
           // string EncryptionKey = General.keyval;
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encryptString;
        }

        public static string Decrypt(string cipherText, string EncryptionKey)
        {
            //string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            //string EncryptionKey = General.keyval;
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
        public static PhysicalAddress GetMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Only consider Ethernet network interfaces
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic.GetPhysicalAddress();
                }
            }
            return null;
        }
        // generate Key
        public static string kEYgEN()
        {
            var Mac = Convert.ToString(GetMacAddress());
            string result = "";
            foreach (var ch in Mac)
            {
                switch (ch)
                {
                    case '0':
                        result += "N9";
                        break;
                    case '1':
                        result += "YU";
                        break;
                    case '2':
                        result += "LH";
                        break;
                    case '3':
                        result += "5J";
                        break;
                    case '4':
                        result += "CZ";
                        break;
                    case '5':
                        result += "0X";
                        break;
                    case '6':
                        result += "PV";
                        break;
                    case '7':
                        result += "5T";
                        break;
                    case '8':
                        result += "C5";
                        break;
                    case '9':
                        result += "3W";
                        break;
                    case 'A':
                        result += "OF";
                        break;
                    case 'B':
                        result += "R2";
                        break;
                    case 'C':
                        result += "M7";
                        break;
                    case 'D':
                        result += "RH";
                        break;
                    case 'E':
                        result += "VJ";
                        break;
                    case 'F':
                        result += "QE";
                        break;
                    default:
                        result += "N9";
                        break;
                }
            }
           // var results = Encrypt(result, General.SecVal);
            var i = 0;
            var data = "";
            foreach (var cr in result)
            {
                if (i % 6 == 0 && i != 0)
                {
                    data += "-";
                }
                data += cr;
                i++;
            }
            return data;
        }
        // reverse Key
        public static string RevkEYgEN(string KeyCode)
        {
            var i = 0;
            var data = "";
            foreach (var cr in KeyCode)
            {
                if (i % 6 != 0 || i == 0)
                {
                    data += cr;
                    i++;
                }
                else
                {
                    i = 0;
                }
            }
           // var results = Decrypt(data, General.SecVal);
            string result = "";
            var let = "";
            i = 1;
            foreach (var ch in data)
            {
                let += ch;
                if (i % 2 == 0)
                {
                    switch (let)
                    {
                        case "N9":
                            result += "0";
                            break;
                        case "YU":
                            result += "1";
                            break;
                        case "LH":
                            result += "2";
                            break;
                        case "5J":
                            result += "3";
                            break;
                        case "CZ":
                            result += "4";
                            break;
                        case "0X":
                            result += "5";
                            break;
                        case "PV":
                            result += "6";
                            break;
                        case "5T":
                            result += "7";
                            break;
                        case "C5":
                            result += "8";
                            break;
                        case "3W":
                            result += "9";
                            break;
                        case "OF":
                            result += "A";
                            break;
                        case "R2":
                            result += "B";
                            break;
                        case "M7":
                            result += "C";
                            break;
                        case "RH":
                            result += "D";
                            break;
                        case "VJ":
                            result += "E";
                            break;
                        case "QE":
                            result += "F";
                            break;
                        default:
                            result += "0";
                            break;
                    }
                    let = "";
                }
                i++;
            }
            return result;
        }
        // generate Key
        public static string LicenceKey(String Mac)
        {
            string result = "";
            foreach (var ch in Mac)
            {
                switch (ch)
                {
                    case '0':
                        result += "6D";
                        break;
                    case '1':
                        result += "F2";
                        break;
                    case '2':
                        result += "VA";
                        break;
                    case '3':
                        result += "4A";
                        break;
                    case '4':
                        result += "L0";
                        break;
                    case '5':
                        result += "DQ";
                        break;
                    case '6':
                        result += "IC";
                        break;
                    case '7':
                        result += "06";
                        break;
                    case '8':
                        result += "T7";
                        break;
                    case '9':
                        result += "38";
                        break;
                    case 'A':
                        result += "O0";
                        break;
                    case 'B':
                        result += "2H";
                        break;
                    case 'C':
                        result += "MU";
                        break;
                    case 'D':
                        result += "RJ";
                        break;
                    case 'E':
                        result += "VT";
                        break;
                    case 'F':
                        result += "QT";
                        break;
                    default:
                        result += "NW";
                        break;
                }
            }
            // var results = Encrypt(result, General.SecVal);
            var i = 0;
            var data = "";
            foreach (var cr in result)
            {
                if (i % 4 == 0 && i != 0)
                {
                    data += "-";
                }
                data += cr;
                i++;
            }
            return data;
        }
        // reverse Key
        public static string RevLicKey(string KeyCode)
        {
            var i = 0;
            var data = "";
            foreach (var cr in KeyCode)
            {
                if (i % 4 != 0 || i == 0)
                {
                    data += cr;
                    i++;
                }
                else
                {
                    i = 0;
                }
            }
            // var results = Decrypt(data, General.SecVal);
            string result = "";
            var let = "";
            i = 1;
            foreach (var ch in data)
            {
                let += ch;
                if (i % 2 == 0)
                {
                    switch (let)
                    {
                        case "6D":
                            result += "0";
                            break;
                        case "F2":
                            result += "1";
                            break;
                        case "VA":
                            result += "2";
                            break;
                        case "4A":
                            result += "3";
                            break;
                        case "L0":
                            result += "4";
                            break;
                        case "DQ":
                            result += "5";
                            break;
                        case "IC":
                            result += "6";
                            break;
                        case "06":
                            result += "7";
                            break;
                        case "T7":
                            result += "8";
                            break;
                        case "38":
                            result += "9";
                            break;
                        case "O0":
                            result += "A";
                            break;
                        case "2H":
                            result += "B";
                            break;
                        case "MU":
                            result += "C";
                            break;
                        case "RJ":
                            result += "D";
                            break;
                        case "VT":
                            result += "E";
                            break;
                        case "QT":
                            result += "F";
                            break;
                        default:
                            result += "0";
                            break;
                    }
                    let = "";
                }
                i++;
            }
            return result;
        }

    }
    static class RandomLetter
    {
        static Random _random = new Random();
        public static char GetLetter()
        {
            // This method returns a random lowercase letter.
            // ... Between 'a' and 'z' inclusize.
            int num = _random.Next(0, 26); // Zero to 25
            char let = (char)('a' + num);
            return let;
        }
        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
    }
}
//usage
// Security.Encrypt(plaintext, General.keyval);
// Security.Decrypt(encryptedstring, General.keyval);
// https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp