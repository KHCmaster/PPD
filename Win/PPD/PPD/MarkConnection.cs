using System;
using System.Collections.Generic;
using System.Text;

namespace PPD
{
    class MarkConnection
    {
        public delegate void DrawConnectionCallback(Mark mk1, Mark mk2);
        List<Mark> convexmarks;
        List<Mark> inmarks;
        public MarkConnection()
        {
            convexmarks = new List<Mark>();
            inmarks = new List<Mark>();
        }

        public void AddConvex(Mark mk)
        {
            convexmarks.Add(mk);
        }

        public void AddIn(Mark mk)
        {
            inmarks.Add(mk);
        }

        public void DrawConnection(DrawConnectionCallback callback)
        {
            Mark lastmk = convexmarks[0];
            for (int i = 1; i < convexmarks.Count; i++)
            {
                callback(lastmk, convexmarks[i]);
                lastmk = convexmarks[i];
            }
            if (convexmarks.Count >= 3)
            {
                callback(lastmk, convexmarks[0]);
            }
            foreach (Mark inmk in inmarks)
            {
                foreach (Mark convmk in convexmarks)
                {
                    callback(inmk, convmk);
                }
            }
        }

        public bool Contains(Mark mk)
        {
            return convexmarks.Contains(mk) || inmarks.Contains(mk);
        }

    }
}
