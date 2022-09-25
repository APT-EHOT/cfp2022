using System.Text;
using hack;

DateTime startAt = DateTime.Now;
const int TAKE_IN_RESULT = 5000;
var filePath = "/Users/asmirnov/Downloads/hack/DS_train(2020-06--2022-06-01).csv";
var outputPath = "/Users/asmirnov/Downloads/hack/result.csv";
var yTrainPath = "/Users/asmirnov/Downloads/hack/y_train.csv";

var YHelper = new YTrain(yTrainPath);

var worker = new Worker();
var collector = NewCollector();
var collectors = new List<RowCollector>();
static RowCollector NewCollector()
{
    var processor = new PreProcessor(10, 1.4f, 10000);
    var minDate = new DateTime(2020, 6, 1);
    var maxDate = new DateTime(2022, 4, 1);
    return new RowCollector(minDate, maxDate, processor);
}

long i = 0;
Console.WriteLine("Начинаем читать файл");
const string OUT = "OutOfStock";
using (var fs = new StreamReader(new BufferedStream(File.OpenRead(filePath), 512 * 1024 * 1024)))
{
    string line;
    fs.ReadLine();

    var list = new List<Row>();
    while ((line = fs.ReadLine()) != null)
    {
        var values = line.Trim().Split();
        if (values[3] == OUT)
        {
            if (++i % 10000000 == 0) Console.WriteLine($"Прочитано {i} строк");
            continue;
        }

        var row = new Row()
        {
            WebPriceId = int.Parse(values[0]),
            DateObserve =  DateTime.Parse(values[1] + " " + values[2]),
            CurrentPrice = float.Parse(values.Length == 5 ? values[4] : "0"),
        };

        if (collector.GetWebPriceId() == 0 || collector.GetWebPriceId() == row.WebPriceId)
        {
            collector.AddRow(row);
        }
        else
        {
            collectors.Add(collector);
            
            collector = NewCollector();
            collector.AddRow(row);
        }
        
        if (++i % 10000000 == 0) Console.WriteLine($"Прочитано {i} строк");
    }
}

Console.WriteLine($"Файл прочитан за {DateTime.Now.Subtract(startAt)}");

Console.WriteLine("Начинаем обрабатывать группы строк");
var lists = collectors
    .Select(x => x.BuildRowList())
    .OfType<List<Row>>()
    .Select(x => worker.work(x))
    .Split(TAKE_IN_RESULT);

var tasks = new List<Task<List<DataSet>>>(); 
foreach (var list in lists)
    tasks.Add(Task.Run(() => GetDataSets(list)));

Task.WaitAll(tasks.ToArray());

var dss = new List<DataSet>();
foreach (var task in  tasks)
{
    dss.AddRange(task.Result);
}

using (var fs = new StreamWriter(outputPath, false))
{
    var sb = new StringBuilder();
    sb.Append("d,");
    sb.Append(string.Join("", Enumerable.Range(0, 37).Select(x => $"ibc{x},")));
    sb.Append('y');
    fs.WriteLine(sb.ToString());

    var test = dss
        .Where(ds => ds.Ibc.Count(x => x == 0) <= 3)
        .OrderBy(ds => ds.Ibc.Sum())
        .ToList();
    foreach (var ds in test.GetRange((int)(test.Count * 0.2), (int)(test.Count * 0.6)))
    {
        sb = new StringBuilder();
        sb.Append($"{ds.DecadeCount},");
        sb.Append(string.Join("", ds.Ibc.Select(x => $"{x.ToString("F6")},")));
        sb.Append(ds.Y.ToString("F6"));
        fs.WriteLine(sb.ToString());
    }
}

Console.WriteLine($"Сделано за {DateTime.Now.Subtract(startAt)}");
List<DataSet> GetDataSets(IEnumerable<List<Row>> rows)
{
    var result = new List<DataSet>();

    int mmax = rows.First().Count;
    List<float> ibcs = new List<float>();
    for (int i = 0; i < mmax; i++)
    {
        var prices = rows.Select(x => x[i].CurrentPrice);
        var ibc = prices.Sum() / prices.Count();
        
        ibcs.Add(ibc);
    }

    List<float> ibcPercent = new List<float>();
    for (int i = 2; i < ibcs.Count; i++)
    {
        ibcPercent.Add(ibcs[i] * 1.0f / ibcs[i - 1] - 1);
    }

    for (int i = 0; i + 40 < ibcs.Count; i += 3)
    {
        List<List<float>> af = new List<List<float>>();

        try
        {
            af.Add(ibcPercent.GetRange(i + 0, 37)); // 2
            af.Add(ibcPercent.GetRange(i + 1, 37)); // 1
            af.Add(ibcPercent.GetRange(i + 2, 37)); // 0
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.ToString());
        }

        var y = YHelper.getYByDecade(rows.First()[i + 39].DateObserve);
        
        for (int j = 0, d = 2; j < 3; j++, d--)
        {
            result.Add(new DataSet()
            {
                DecadeCount = d,
                Ibc = af[j],
                Y = y,
            });
        }
    }

    return result;
}

public struct DataSet
{
    public int DecadeCount;
    public List<float> Ibc;
    public float Y;

    public DataSet()
    {
        DecadeCount = 0;
        Ibc = new List<float>();
        Y = 0;
    }
}

public struct Row
{
    public int WebPriceId;
    public DateTime DateObserve;
    public float CurrentPrice;
}

class Worker
{
    private List<DateTime> beetwen(DateTime a, DateTime b)
    {
        var result = new List<DateTime>();

        if (a == b)
        {
            return new List<DateTime>() {b};
        }
        
        while (true)
        {
            a = DateHelper.FormatDecades(a.AddDays(10));
            result.Add(a);
            if (a >= b)
            {
                result.Add(b);
                break;
            }
        }

        return result;
    }

    private volatile int _i = 0;
    public List<Row> work(List<Row> list)
    {
        Interlocked.Add(ref this._i, 1);
        if (this._i % 100000 == 0)
        {
            Console.WriteLine($"Обработано {this._i} групп");
        }
        var result = new List<Row>();
        
        // Добавляем фиктивную строчку, чтобы добавилось все до нее
        list.Add(new Row()
        {
            DateObserve = new DateTime(9000, 1, 1),
        });

        Row? lastRow = null;
        DateTime? lastDateTime = null;
        foreach (var row in list)
        {
            if (lastRow == null)
            {
                lastRow = row;
                lastDateTime = DateHelper.FormatDecades(row.DateObserve);
                continue;
            }

            var dt = DateHelper.FormatDecades(row.DateObserve);
            if (dt == (DateHelper.FormatDecades(lastDateTime ?? DateTime.Now)))
            {
                lastRow = row;
                continue;
            }
            else
            {
                var resRow = lastRow ?? new Row();
                if (result.Count == 0)
                {
                    result.Add(new Row()
                    {
                        WebPriceId = resRow.WebPriceId,
                        DateObserve = DateHelper.FormatDecades(resRow.DateObserve),
                        CurrentPrice = resRow.CurrentPrice,
                    });
                    continue;
                }
                
                foreach (var date in beetwen(
                             DateHelper.FormatDecades(result.Last().DateObserve), 
                             DateHelper.FormatDecades(resRow.DateObserve))
                             .Distinct())
                {
                    result.Add(new Row()
                    {
                        WebPriceId = resRow.WebPriceId,
                        DateObserve = date,
                        CurrentPrice = resRow.CurrentPrice,
                    });
                }
                
                lastRow = row;
                lastDateTime = dt;
            }
        }

        return result.DistinctBy(x => x.DateObserve).ToList();
    }
}