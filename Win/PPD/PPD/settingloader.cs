using System;
using System.Collections.Generic;
using System.Text;

namespace testgame
{
    class settingloader
    {
        public settingloader()
        {

        }

        public string readdata(string s, string meta)
        {
            int a = s.IndexOf("[" + meta + "]");
            if (a == -1)
            {
                return "";
            }
            else
            {
                int b = s.IndexOf('\n', a + 1);
                if (b == -1)
                {
                    return s.Substring(a + meta.Length + 2);
                }
                else
                {
                    return s.Substring(a + meta.Length + 2, b - a - meta.Length - 2);
                }
            }
        }
    }
}
