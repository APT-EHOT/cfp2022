namespace hack;

public class RowCollector
{
    private List<Row> list = new List<Row>(); 
    
    private readonly DateTime minDateTime;
    private readonly DateTime maxDateTime;
    private readonly PreProcessor processor;

    public RowCollector(
        DateTime minDateTime, 
        DateTime maxDateTime,
        PreProcessor processor
        )
    {
        this.minDateTime = minDateTime;
        this.maxDateTime = maxDateTime;
        this.processor = processor;
    }

    public int GetWebPriceId()
    {
        return list.Count > 0 ? list[0].WebPriceId : 0;
    }

    public void AddRow(Row row)
    {
        if ((minDateTime >= row.DateObserve || row.DateObserve >= maxDateTime) 
            || (0.1 <= row.CurrentPrice && row.CurrentPrice <= 2) || row.CurrentPrice >= 10000)
        {
            return;
        }
        
        if (GetWebPriceId() == 0)
        {
            list.Add(row with {DateObserve = minDateTime});
            list.Add(row);
        }
        else
        {
            list.Add(row);
        }
    }

    private bool Prettify()
    {
        if (GetWebPriceId() == 0)
        {
            return false;
        }
        else
        {
            if (list[0].CurrentPrice == 0)
            {
                var value = FindFirstNotNullValue();
                if (value == 0)
                {
                    return false;
                }
                
                list[0] = list[0] with { CurrentPrice = value };
            }
        }
        
        for (var i = 0; i < list.Count; i++)
        {
            list[i] = list[i] with
            {
                DateObserve = DateHelper.FormatDecades(list[i].DateObserve),
            };
        }

        return true;
    }

    private float FindFirstNotNullValue()
    {
        foreach (var row in list)
        {
            if (row.CurrentPrice != 0)
            {
                return row.CurrentPrice;
            }
        }

        return 0;
    }

    private List<Row> MergeDecades()
    {
        return list
            .GroupBy(x => x.DateObserve)
            .Select(y =>
            {
                return y.First() with { CurrentPrice = y.Average(z => z.CurrentPrice) };
            })
            .ToList();
    }
    
    public List<Row>? BuildRowList()
    {
        if (GetWebPriceId() == 0)
        {
            return null;
        }
        
        if (!Prettify())
        {
            return null;
        }
        
        list.Add(list.Last() with {DateObserve = maxDateTime});
        
        var resultList = MergeDecades();
        if (processor.FilterByListSize(resultList) || !processor.FilterByCurrentPriceAverage(resultList))
        {
            return null;
        }

        return resultList;
    }
}