namespace hack;

public class YTrain
{
    public Dictionary<DateTime, float> Y = new Dictionary<DateTime, float>();

    public YTrain(string path)
    {
        using (var sr = new StreamReader(path))
        {
            sr.ReadLine();

            while (!sr.EndOfStream)
            {
                var afgd = sr.ReadLine()?.Trim().Split(',');
                Y.Add(DateTime.Parse(afgd[0]), float.Parse(afgd[1]) / 100.0f);
            }
        }
    }


    public float getYByDecade(DateTime d)
    {
        var dec = new DateTime(d.Year, d.Month, 1);
        
        return Y[dec];
    }
}