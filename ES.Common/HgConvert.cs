using System;
using System.Text;

namespace ES.Common
{
    /// <summary>
    /// Converts a base data type to another base data type.
    /// </summary>
    public static class HgConvert
    {
        /// <summary>
        /// Returns the specified Boolean value
        /// </summary>
        public static bool ToBoolean(object o)
        {
            if (o == null || o == DBNull.Value || string.IsNullOrEmpty(o.ToString()))
            {
                return false;
            }
            return Convert.ToBoolean(o);
        }

        /// <summary>
        /// Returns the specified 8-bit unsigned integer
        /// </summary>
        public static byte ToByte(object o)
        {
            if (o == null || o == DBNull.Value)
            {
                return 0;
            }
            return Convert.ToByte(o);
        }

        /// <summary>
        /// Converts the value to a Unicode character.
        /// </summary>
        public static char ToChar(object o)
        {
            if (o == null || o == DBNull.Value)
            {
                return char.MinValue;
            }
            return Convert.ToChar(o);
        }

        /// <summary>
        /// Returns the specified System.DateTime;
        /// </summary>
        public static DateTime ToDateTime(object o)
        {
            if (o == null || o == DBNull.Value)
            {
                return DateTime.MinValue;
            }
            return Convert.ToDateTime(o);
        }

        /// <summary>
        /// Converts the value to an equivalent System.Decimal number.
        /// </summary>
        public static decimal ToDecimal(object o)
        {
            if (o == null || o == DBNull.Value)
            {
                return 0;
            }
            return Convert.ToDecimal(o);
        }

        /// <summary>
        /// Returns the specified double-precision floating point number
        /// </summary>
        public static double ToDouble(object o)
        {
            if (o == null || o == DBNull.Value)
            {
                return 0;
            }
            return Convert.ToDouble(o);
        }

        /// <summary>
        /// Converts the value to the equivalent 16-bit signed integer.
        /// </summary>
        public static Int16 ToInt16(object o)
        {
            if (o == null || o == DBNull.Value)
            {
                return 0;
            }
            return Convert.ToInt16(o);
        }

        /// <summary>
        ///  Converts the value to an equivalent 32-bit signed integer.
        /// </summary>
        public static Int32 ToInt32(object o)
        {
            int i;
            if (o != null && int.TryParse(o.ToString(), out i))
            {
                return i;
            }
            return 0;
        }

        /// <summary>
        /// Converts the value to an equivalent 64-bit signed integer.
        /// </summary>
        public static Int64 ToInt64(object o)
        {
            long i;
            if (o != null && long.TryParse(o.ToString(), out i))
            {
                return i;
            }
            return 0;
        }

        /// <summary>
        /// Converts the value to an equivalent single-precision floating point number.
        /// </summary>
        public static float ToFloat(object o)
        {
            if (o == null || o == DBNull.Value)
            {
                return 0;
            }
            return Convert.ToSingle(o);
        }

        /// <summary>
        /// Converts the value to its equivalent System.String representation.
        /// </summary>
        public static string ToString(object o)
        {
            if (o == null || o == DBNull.Value)
            {
                return string.Empty;
            }
            return Convert.ToString(o);
        }
        public static Guid ToGuid(object o)
        {
            if (o == null || o == DBNull.Value)
            {
                return new Guid();
            }
            try
            {
                return Guid.Parse(o.ToString());
            }
            catch (Exception)
            {
                return new Guid();
            }

        }
        public static string ToLatArm(string text)
        {
            var latArmText = string.Empty;

            for (int ind = 0; ind < text.Length; ind++)
            {
                var ntext = text.Substring(ind, 1);
                var utfText = Encoding.Unicode.GetBytes(ntext);
            if (utfText[1] != 5) { latArmText += ntext;continue; }
                switch (utfText[0])
                {
                        //Ա
                    case 49:
                        latArmText += "A";
                        break;
                    //ա
                    case 97:
                        latArmText += "a";
                        break;
                    //Բ
                    case 50:
                        latArmText += "B";
                        break;
                    //բ
                    case 98:
                        latArmText += "b";
                        break;
                   //Գ
                    case 51:
                        latArmText += "G";
                        break;
                    //գ
                    case 99:
                        latArmText += "g";
                        break;
                    //Դ
                    case 52:
                        latArmText += "D";
                        break;
                    //դ
                    case 100:
                        latArmText += "d";
                        break;
                    //Ե
                    case 53:
                        latArmText += "E";
                        break;
                    //ե
                    case 101:
                        latArmText += "e";
                        break;
                    //Զ
                    case 54:
                        latArmText += "Z";
                        break;
                    //զ
                    case 102:
                        latArmText += "z";
                        break;
                    //Է
                    case 55:
                        latArmText += "E";
                        break;
                    //է
                    case 103:
                        latArmText += "e";
                        break;
                    //Ը
                    case 56:
                        latArmText += "@";
                        break;
                    //ը
                    case 104:
                        latArmText += "@";
                        break;
                    //Թ
                    case 57:
                        latArmText += "T";
                        break;
                    //թ
                    case 105:
                        latArmText += "t";
                        break;
                    //Ժ
                    case 58:
                        latArmText += "J";
                        break;
                    //ժ
                    case 106:
                        latArmText += "j";
                        break;
                    //Ի
                    case 59:
                        latArmText += "I";
                        break;
                    //ի
                    case 107:
                        latArmText += "i";
                        break;
                    //Լ
                    case 60:
                        latArmText += "L";
                        break;
                    //լ
                    case 108:
                        latArmText += "l";
                        break;
                    //Խ
                    case 61:
                        latArmText += "Kh";
                        break;
                    //խ
                    case 109:
                        latArmText += "kh";
                        break;
                    //Ծ
                    case 62:
                        latArmText += "C";
                        break;
                    //ծ
                    case 110:
                        latArmText += "c";
                        break;
                    //Կ
                    case 63:
                        latArmText += "K";
                        break;
                    //կ
                    case 111:
                        latArmText += "k";
                        break;
                    //Հ
                    case 64:
                        latArmText += "H";
                        break;
                    //հ
                    case 112:
                        latArmText += "h";
                        break;
                    //Ձ
                    case 65:
                        latArmText += "Dz";
                        break;
                    //ձ
                    case 113:
                        latArmText += "dz";
                        break;
                    //Ղ
                    case 66:
                        latArmText += "X";
                        break;
                    //ղ
                    case 114:
                        latArmText += "x";
                        break;
                    //Ճ
                    case 67:
                        latArmText += "&";
                        break;
                    //ճ
                    case 115:
                        latArmText += "&";
                        break;
                    //Մ
                    case 68:
                        latArmText += "M";
                        break;
                    //մ
                    case 116:
                        latArmText += "m";
                        break;
                    //Յ
                    case 69:
                        latArmText += "Y";
                        break;
                    //յ
                    case 117:
                        latArmText += "y";
                        break;
                    //Ն
                    case 70:
                        latArmText += "N";
                        break;
                    //ն
                    case 118:
                        latArmText += "n";
                        break;
                    //Շ
                    case 71:
                        latArmText += "Sh";
                        break;
                    //շ
                    case 119:
                        latArmText += "sh";
                        break;
                    //Ո
                    case 72:
                        latArmText += "Vo";
                        break;
                    //ո
                    case 120:
                        latArmText += "o";
                        break;
                    //Չ
                    case 73:
                        latArmText += "Ch";
                        break;
                    //չ
                    case 121:
                        latArmText += "ch";
                        break;
                    //Պ
                    case 74:
                        latArmText += "P";
                        break;
                    //պ
                    case 122:
                        latArmText += "p";
                        break;
                    //Ջ
                    case 75:
                        latArmText += "J";
                        break;
                    //ջ
                    case 123:
                        latArmText += "j";
                        break;
                    //Ռ
                    case 76:
                        latArmText += "R";
                        break;
                    //ռ
                    case 124:
                        latArmText += "r";
                        break;
                    //Ս
                    case 77:
                        latArmText += "S";
                        break;
                    //ս
                    case 125:
                        latArmText += "s";
                        break;
                    //Վ
                    case 78:
                        latArmText += "V";
                        break;
                    //վ
                    case 126:
                        latArmText += "v";
                        break;
                    //Տ
                    case 79:
                        latArmText += "T";
                        break;
                    //տ
                    case 127:
                        latArmText += "t";
                        break;
                    //Ր
                    case 80:
                        latArmText += "R";
                        break;
                    //ր
                    case 128:
                        latArmText += "r";
                        break;
                    //Ց
                    case 81:
                        latArmText += "C";
                        break;
                    //ց
                    case 129:
                        latArmText += "c";
                        break;
                    //Ու
                    case 82:
                        latArmText += "U";
                        break;
                    //ու
                    case 130:
                        latArmText += "u";
                        break;
                    //Փ
                    case 83:
                        latArmText += "P";
                        break;
                    //փ
                    case 131:
                        latArmText += "p";
                        break;
                    //Ք
                    case 84:
                        latArmText += "Q";
                        break;
                    //ք
                    case 132:
                        latArmText += "q";
                        break;
                    //Եվ
                    case 85:
                        latArmText += "Ev";
                        break;
                    //և
                    case 133:
                        latArmText += "ev";
                        break;
                    //Օ
                    case 86:
                        latArmText += "O";
                        break;
                    //օ
                    case 134:
                        latArmText += "o";
                        break;
                    //Ֆ
                    case 87:
                        latArmText += "F";
                        break;
                    //ֆ
                    case 135:
                        latArmText += "f";
                        break;
                    default:
                        latArmText += ntext;
                        break;
                }
            }
            return latArmText;
            return text;
        }
    }
}
