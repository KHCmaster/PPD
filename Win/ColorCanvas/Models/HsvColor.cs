namespace ColorCanvas.Models
{
    internal struct HsvColor(double h, double s, double v)
    {
        public double H = h;
        public double S = s;
        public double V = v;
    }
}
