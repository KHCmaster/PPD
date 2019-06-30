using System;
using System.Collections.Generic;
using System.Text;

namespace testgame
{
    class settingreplacewriter
    {
        public settingreplacewriter()
        {

        }
        public string replace(string s, string meta, string writedata)
        {
            int a = s.IndexOf("[" + meta + "]");
            if (a == -1)
            {
                return s;
            }
            else
            {
                int b = s.IndexOf('\n', a + 1);
                if (b == -1)
                {
                    return s.Substring(0,a + meta.Length + 2) + writedata;
                }
                else
                {
                    return s.Substring(0, a + meta.Length + 2) + writedata + s.Substring(b);
                }
            }
        }
    }
}
